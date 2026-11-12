# RESEARCH.md — Learning and decisions

This file is a **starting draft**. Before submitting, update it with research you
personally performed, real links you visited, problems you encountered and the
results of your own testing. Do not represent template notes as personal research.

## Why this problem?
A log checker is relevant to DevOps because it turns raw application output into
an operational signal and an actionable recommendation.

## Design decisions to review
- **Console app:** Simple to deploy and invoke from scripts; no web server required.
- **Text log format:** Human-readable and easy to generate for demonstration.
- **`File.ReadLines`:** Streams lines rather than loading the whole file at once.
- **`Dictionary`:** Stores counts per service with case-insensitive keys.
- **`DateTimeOffset.TryParseExact`:** Rejects invalid timestamps explicitly.
- **Exit codes:** Allow shell scripts or CI jobs to react to alerts.
- **No third-party packages:** Keeps setup and dependencies minimal.

## Research to do and document (add your own findings)
1. Read Microsoft's documentation for `File.ReadLines` and `DateTimeOffset.TryParseExact`.
2. Review the .NET CLI documentation for `dotnet run` and exit codes.
3. Compare file-wide thresholds with rolling-window monitoring.
4. Run sample and malformed input tests, record what worked and what did not.

## Problems encountered
Fill in after running the project. Example prompts: unexpected timestamp handling,
invalid file path, case-insensitive levels, or a false alert from an entire historical log.

## What I learned
Write your own short summary once you have tested and understood the program.
