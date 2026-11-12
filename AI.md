# AI.md — AI assistance disclosure

AI assistance was used to draft an initial C# .NET 9 implementation, project
structure, sample input and documentation. The developer must personally review,
run, test and explain the code before submitting it.

## Tool and purpose
- ChatGPT: generated an initial solution to the open-ended EAS coding challenge,
  including log parsing, severity aggregation, CLI arguments and self-test cases.

## Verification still required
- Open the solution in Visual Studio or run `dotnet run -- --self-test`.
- Confirm the output matches `README.md` with sample log input.
- Manually test a missing file and invalid `--threshold`.
- Check the parsing and alert threshold logic line by line.

## Incorrect or unhelpful suggestions
Add any real issues you discover during verification. Do not invent examples.

## Final review
Update this section with specific changes made after your own independent review.
