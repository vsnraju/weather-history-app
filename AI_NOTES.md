# AI Notes

## Tools Used

- ChatGPT / Codex was used to help design, scaffold, and refine the solution architecture and implementation.
- 
- The local terminal was used to verify SDK versions, manage packages, build the solution, and run tests.

---

# Helpful Prompts

1. "Act as a senior .NET 10 and Angular 21 architect. Design and implement a production-quality coding exercise using Clean Architecture."

2. "Use repository pattern, Result pattern,Strategy pattern typed HttpClient, Polly, Serilog, xUnit, Moq, FluentAssertions, and Angular Material while keeping the design simple."

3. "Generate production improvement ideas, testing strategy, commit breakdowns, and architecture refinement suggestions."

---

# AI Suggestions That Needed Correction

An initial Angular Material installation attempted to pin `@angular/animations` to `21.2.10`, while the generated Angular application resolved core Angular packages to `21.2.12`. npm reported a peer dependency conflict, which I corrected by aligning package versions with the generated Angular workspace.

Another small correction involved the .NET solution format. .NET 10 generated a `.slnx` solution file instead of the traditional `.sln` format, so project references and CLI commands were adjusted accordingly.

I also initially explored using the Strategy Pattern for date parsing because multiple input formats were provided. After reviewing the actual requirements and expected scale, I simplified the implementation to use `DateTime.TryParseExact(...)` with a supported formats array. This reduced unnecessary abstraction while keeping the solution extensible and testable.

---

# Code Written Deliberately

- The `Result<T>` model and orchestration flow were intentionally kept small and explicit so that failures are predictable and easy to explain during a walkthrough.
- The date parsing implementation was simplified to use `DateTime.TryParseExact(...)` directly rather than introducing multiple parser strategies for a small fixed set of formats.
- The repository implementation writes through a temporary file before replacing the target JSON file to reduce the risk of partially written cache files.
- Swagger was enabled only for non-production environments to align with common production security practices and reduce unnecessary API surface exposure in deployed environments.
- The backend flow was intentionally designed to remain mostly sequential because the dataset is very small and readability/debuggability were prioritized over premature optimization.
- The Angular dashboard UI was hand-shaped instead of relying on generated starter templates so loading states, error handling, sorting, and filtering behavior could be demonstrated clearly.

---

# Future Improvements

If this were expanded into a production-scale feature, I would consider:

- Introducing bounded parallel processing using `Parallel.ForEachAsync`
- Adding rate-limit handling for external API calls
- Moving cached weather storage to cloud object storage
- Adding Redis distributed caching
- Adding OpenTelemetry tracing and health checks
- Containerizing the solution with Docker and CI/CD pipelines
