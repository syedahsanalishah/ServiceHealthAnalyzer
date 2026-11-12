using System.Globalization;

// Example:
// dotnet run -- --file sample-data/service.log --threshold 2
// dotnet run -- --self-test

if (args.Contains("--self-test", StringComparer.OrdinalIgnoreCase))
{
    RunSelfTests();
    return;
}

string? path = ReadOption(args, "--file");
string? thresholdText = ReadOption(args, "--threshold");

if (args.Contains("--help") || path is null)
{
    Console.WriteLine("Service Health Analyzer (.NET 9)");
    Console.WriteLine("Usage: dotnet run -- --file <path> [--threshold <positive integer>]");
    Console.WriteLine("       dotnet run -- --self-test");
    return;
}

int threshold = 2;
if (thresholdText is not null && (!int.TryParse(thresholdText, out threshold) || threshold < 1))
{
    Console.Error.WriteLine("Error: --threshold must be a positive integer.");
    Environment.ExitCode = 2;
    return;
}

if (!File.Exists(path))
{
    Console.Error.WriteLine($"Error: File not found: {path}");
    Environment.ExitCode = 2;
    return;
}

try
{
    Analysis report = Analyze(File.ReadLines(path), threshold);
    PrintReport(report, threshold);
    // The nonzero status lets a CI job or shell script detect an alert.
    if (report.Alerts.Count > 0) Environment.ExitCode = 1;
}
catch (IOException ex)
{
    Console.Error.WriteLine($"Could not read log: {ex.Message}");
    Environment.ExitCode = 2;
}
catch (UnauthorizedAccessException ex)
{
    Console.Error.WriteLine($"Access denied: {ex.Message}");
    Environment.ExitCode = 2;
}

static string? ReadOption(string[] arguments, string option)
{
    for (int i = 0; i < arguments.Length; i++)
        if (string.Equals(arguments[i], option, StringComparison.OrdinalIgnoreCase))
            return i + 1 < arguments.Length ? arguments[i + 1] : null;
    return null;
}

static Analysis Analyze(IEnumerable<string> lines, int threshold)
{
    var counts = new Dictionary<string, LevelCounts>(StringComparer.OrdinalIgnoreCase);
    int valid = 0, invalid = 0;

    foreach (string line in lines)
    {
        if (string.IsNullOrWhiteSpace(line)) continue;
        // Limit splits to 4 fields, so messages may contain the '|' character.
        string[] fields = line.Split('|', 4);
        if (fields.Length != 4 ||
            !DateTimeOffset.TryParseExact(fields[0].Trim(), "yyyy-MM-ddTHH:mm:ssK",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out _) ||
            !Enum.TryParse<LogLevel>(fields[1].Trim(), true, out var level) ||
            !Enum.IsDefined(level) ||
            string.IsNullOrWhiteSpace(fields[2]) ||
            string.IsNullOrWhiteSpace(fields[3]))
        {
            invalid++;
            continue;
        }

        string service = fields[2].Trim();
        if (!counts.TryGetValue(service, out var count))
        {
            count = new LevelCounts();
            counts[service] = count;
        }
        count.Add(level);
        valid++;
    }

    var alerts = counts
        .Where(pair => pair.Value.Errors >= threshold)
        .OrderByDescending(pair => pair.Value.Errors)
        .ThenBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
        .Select(pair => pair.Key)
        .ToList();

    return new Analysis(counts, valid, invalid, alerts);
}

static void PrintReport(Analysis report, int threshold)
{
    Console.WriteLine("=== SERVICE HEALTH REPORT ===");
    Console.WriteLine($"Valid entries: {report.ValidEntries} | Invalid entries skipped: {report.InvalidEntries}");
    Console.WriteLine($"Alert threshold: {threshold} ERROR entries per service");
    Console.WriteLine();

    if (report.Counts.Count == 0)
        Console.WriteLine("No valid log entries found.");

    foreach (var item in report.Counts.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
        Console.WriteLine($"{item.Key,-18} INFO: {item.Value.Info,3}  WARN: {item.Value.Warn,3}  ERROR: {item.Value.Errors,3}");

    Console.WriteLine();
    if (report.Alerts.Count == 0)
    {
        Console.WriteLine("STATUS: OK (no service meets the error threshold)");
    }
    else
    {
        Console.WriteLine("STATUS: ATTENTION REQUIRED");
        foreach (string service in report.Alerts)
            Console.WriteLine($"ACTION: Investigate {service} - error count exceeded threshold. Check recent deployments, dependencies and detailed logs.");
    }
    if (report.InvalidEntries > 0)
        Console.WriteLine("ACTION: Review skipped lines; malformed logs can hide problems.");
}

static void RunSelfTests()
{
    string[] data =
    [
        "2026-11-12T08:00:00Z|INFO|orders|Started",
        "2026-11-12T08:00:01Z|ERROR|payments|Timeout",
        "2026-11-12T08:00:02Z|ERROR|payments|HTTP 500 | upstream",
        "2026-11-12T08:00:03Z|WARN|orders|Retry",
        "invalid line"
    ];
    Analysis report = Analyze(data, 2);
    Assert(report.ValidEntries == 4, "Valid entries count");
    Assert(report.InvalidEntries == 1, "Malformed entries count");
    Assert(report.Alerts.SequenceEqual(["payments"]), "Threshold alert");
    Assert(report.Counts["orders"].Info == 1 && report.Counts["orders"].Warn == 1, "Level counts");
    Assert(Analyze(data, 3).Alerts.Count == 0, "Different threshold");
    Assert(Analyze([""], 1).ValidEntries == 0, "Empty line");
    Console.WriteLine("All 6 self-tests passed.");
}

static void Assert(bool condition, string description)
{
    if (!condition) throw new Exception($"Self-test failed: {description}");
}

enum LogLevel { INFO, WARN, ERROR }

sealed class LevelCounts
{
    public int Info { get; private set; }
    public int Warn { get; private set; }
    public int Errors { get; private set; }

    public void Add(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.INFO: Info++; break;
            case LogLevel.WARN: Warn++; break;
            case LogLevel.ERROR: Errors++; break;
        }
    }
}

sealed record Analysis(
    Dictionary<string, LevelCounts> Counts,
    int ValidEntries,
    int InvalidEntries,
    List<string> Alerts);
