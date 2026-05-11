# AI Notes

## Tools Used

- ChatGPT / Codex was used to scaffold and implement the solution in this workspace.
- The local terminal was used to verify SDK versions, extract the PDF requirements, install packages, build, and run tests.

## Helpful Prompts

1. "Act as a senior .NET 10 and Angular 21 architect. Design and implement a production-quality coding exercise using Clean Architecture."
2. "Use repository pattern, exact date parsing, Result pattern, typed HttpClient, Polly, Serilog, xUnit, Moq, FluentAssertions, and Angular Material."
3. "Generate README, AI_NOTES, suggested commit breakdown, and production improvement ideas."

## AI Suggestion That Needed Correction

An initial Angular Material install attempted to pin `@angular/animations` to `21.2.10`, but the generated Angular app resolved core packages to `21.2.12`. npm correctly reported a peer dependency conflict. I corrected the install by using `@angular/animations@21.2.12` while keeping Material/CDK on the latest available `21.2.x` package.

Another small correction: .NET 10 generated a `.slnx` solution file instead of a classic `.sln`; project additions were adjusted to target `WeatherCodingExercise.slnx`.

## Code Written Deliberately

- The `Result` model and weather orchestration were kept small and explicit so failures are easy to explain during a walkthrough.
- The date parser now uses a single exact-format parser with `DateTime.TryParseExact` to keep parsing behavior compact and explicit.
- The file repository writes through a temporary file before replacing the target JSON file to reduce the chance of partially written cache files.
- The Angular dashboard was hand-shaped instead of leaving generated starter content, because the exercise evaluates UI clarity and loading/error behavior.
