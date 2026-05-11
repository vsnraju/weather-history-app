Use .NET 10 and Angular 21.

Design and implement a production-quality using Clean Architecture.

Requirements:
- Backend: ASP.NET Core .NET 10 Web API
- Frontend: Angular 21 standalone components with Angular Material
- Use Clean Architecture with Domain, Application, Infrastructure, and API layers
- Use SOLID principles
- Use async/await properly
- Use typed HttpClient
- Use repository pattern
- Use strategy pattern for date parsing
- Use Result pattern instead of exceptions for business validation
- Use dependency injection everywhere
- Use xUnit + Moq + FluentAssertions for unit tests
- Use Serilog logging
- Use Polly retry policies for external API calls
- Use Open-Meteo historical weather API
- Read dates from dates.txt
- Parse multiple date formats safely
- Handle invalid dates gracefully
- Store weather JSON files locally
- Avoid duplicate API calls using cache/file existence checks
- Expose GET /api/weather endpoint
- Angular UI should:
  - show weather table
  - loading spinner
  - error handling
  - sorting/filtering
  - responsive layout

Generate:
1. Full project structure
2. Architecture explanation
3. Domain models
4. Interfaces
5. Service implementations
6. API controllers
7. Angular services/components
8. Unit tests
9. README.md
10. AI_NOTES.md 
11. Suggested commit breakdown 
12. Production improvement ideas