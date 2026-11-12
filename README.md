# Service Health Analyzer (.NET 9 Console App)

A small command-line tool for the Enterprise Automation Services technical challenge.
It helps an engineer spot services with repeated errors in a text log.

## Problem and purpose
Looking through raw application logs takes time. This app accepts a log file,
counts INFO/WARN/ERROR messages per service, and highlights services that meet an
operator-configurable error threshold. It also reports malformed lines.

## Requirements
- .NET 9 SDK (Visual Studio with .NET 9 support, or the `dotnet` CLI)
- No external NuGet packages or cloud services

## How to run
From this folder:

```bash
dotnet run -- --file sample-data/service.log --threshold 2
```

On Windows, an absolute path also works:

```powershell
dotnet run -- --file "C:\\logs\\service.log" --threshold 3
```

Run built-in tests:

```bash
dotnet run -- --self-test
```

## Input format
One entry per line, four pipe-separated fields:

`YYYY-MM-DDTHH:mm:ssZ|LEVEL|service|message`

Levels: INFO, WARN, ERROR (case-insensitive). Timestamp can also contain a timezone offset.
The message itself may include `|` characters. Blank lines are ignored; malformed
lines are counted and skipped. Example input is in `sample-data/service.log`.

## Output and exit codes
The tool prints counts per service and an actionable investigation suggestion when
ERROR count meets or exceeds the threshold.

- Exit code 0: no alert
- Exit code 1: at least one alert
- Exit code 2: invalid command-line threshold, missing file or file access failure

**Important:** thresholds apply to all entries in the input file, not a rolling time window.
The tool is an aid to triage, not a production monitoring replacement.

## How it works
1. Read lines lazily from the file.
2. Validate timestamp, severity, service and message.
3. Aggregate severity counts by service.
4. Compare ERROR counts with the chosen threshold.
5. Print the report and suggested next steps.

## Testing
The `--self-test` mode checks severity counts, malformed input, messages containing
`|`, threshold changes and blank lines. To test manually, add another ERROR entry
for `orders` and rerun the command.

## Improvements with more time
- Make the threshold a time-window rate instead of an all-file count.
- Add JSON output for CI and automated monitoring integrations.
- Add separate xUnit tests and run them with GitHub Actions.
- Add configurable severity mappings and structured JSON log support.
- Add alert routing with rate limiting to prevent alert fatigue.

## Challenge submission
Publish this folder as a **public Git repository** and make a few meaningful commits
(e.g. first parser, then reporting, then tests/documentation). Do not invent commit history.
The challenge also asks for `RESEARCH.md` and `AI.md`; see those files.
