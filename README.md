# Dallas Historical Weather Coding Exercise

Production-oriented Clean Architecture sample using ASP.NET Core on .NET 10 and an Angular 21 standalone UI with Angular Material.

The app reads dates from `dates.txt`, safely parses multiple date formats, fetches Dallas historical weather from Open-Meteo for valid dates, stores each response as local JSON, avoids duplicate API calls when files already exist, and exposes the aggregate data through `GET /api/weather`.

## Project Structure

```text
.
|-- WeatherCodingExercise.slnx
|-- src
|   |-- Weather.Domain
|   |   |-- Common
|   |   |   |-- Error.cs
|   |   |   `-- Result.cs
|   |   `-- Weather
|   |       `-- WeatherDataPoint.cs
|   |-- Weather.Application
|   |   |-- Abstractions
|   |   |-- DateParsing
|   |   `-- Weather
|   |-- Weather.Infrastructure
|   |   |-- Configuration
|   |   |-- ExternalWeather/OpenMeteo
|   |   |-- Input
|   |   |-- Persistence
|   |   `-- Serialization
|   `-- Weather.Api
|       |-- Controllers
|       |-- dates.txt
|       |-- Program.cs
|       `-- appsettings.json
|-- tests
|   |-- Weather.Application.Tests
|   `-- Weather.Infrastructure.Tests
|-- frontend
|   `-- weather-ui
|       |-- proxy.conf.json
|       `-- src/app/weather
|-- AI_NOTES.md
`-- README.md
```

## Architecture

This solution follows Clean Architecture dependency rules:

- `Weather.Domain` contains the core model and `Result` primitives. It has no dependency on infrastructure, ASP.NET Core, or Angular.
- `Weather.Application` owns use-case orchestration, DTOs, date parsing strategies, and interfaces for input, persistence, and external weather data.
- `Weather.Infrastructure` implements file reading, JSON file persistence, and the typed Open-Meteo `HttpClient` with Polly retries.
- `Weather.Api` wires dependency injection, Serilog, CORS, OpenAPI, and the thin `WeatherController`.
- `frontend/weather-ui` is a separate Angular 21 standalone app that calls the API through a typed Angular service.

Business validation uses `Result<T>` instead of throwing exceptions. Infrastructure still catches IO, HTTP, timeout, and JSON exceptions at boundaries and converts them into explicit failure results.

## Key Backend Types

- Domain model: `WeatherDataPoint` validates required min/max/precipitation values and impossible temperature ranges.
- Result pattern: `Result` and `Result<T>` carry validation and operational failures without using exceptions for expected business outcomes.
- Date parsing interfaces: `IDateParser` and `IDateParsingStrategy`.
- Date strategies: `SlashDateParsingStrategy`, `LongMonthDateParsingStrategy`, and `AbbreviatedMonthDateParsingStrategy`.
- Repository interface: `IWeatherRepository`.
- External API interface: `IWeatherApiClient`.
- Input interface: `IDateFileReader`.
- Use case: `WeatherReportService`.
- API controller: `WeatherController` at `GET /api/weather`.

## API Behavior

`GET /api/weather` returns a list of entries:

```json
[
  {
    "input": "02/27/2021",
    "date": "2021-02-27",
    "minTemperatureC": -3.4,
    "maxTemperatureC": 4.8,
    "precipitationMm": 1.2,
    "status": "Fetched",
    "errorMessage": null,
    "isCached": false
  }
]
```

Invalid dates, such as `April 31, 2022`, return an entry with `status: "InvalidDate"` and no weather values. A failed Open-Meteo call returns `status: "WeatherUnavailable"`.

## Local Storage

Valid weather results are written under:

```text
src/Weather.Api/weather-data/yyyy-MM-dd.json
```

The repository checks for an existing file before the application calls Open-Meteo again.

## Run The Backend

Requirements:

- .NET SDK 10

```powershell
dotnet restore WeatherCodingExercise.slnx
dotnet run --project src/Weather.Api/Weather.Api.csproj --launch-profile http
```

Backend URL:

```text
http://localhost:5012/api/weather
```

## Run The Angular UI

Requirements:

- Node.js compatible with Angular 21
- npm

```powershell
cd frontend/weather-ui
npm install
npm start
```

UI URL:

```text
http://localhost:4200
```

The Angular dev server proxies `/api` to `http://localhost:5012`.

## Tests

Backend:

```powershell
dotnet test WeatherCodingExercise.slnx
```

Frontend:

```powershell
cd frontend/weather-ui
npm test
```

The backend tests cover date parsing, invalid date handling, cache avoidance, fetch-and-save orchestration, file persistence, file date reading, and Open-Meteo response mapping.

## Angular UI

The UI uses Angular standalone components and Angular Material:

- `WeatherService` uses typed `HttpClient.get<WeatherEntry[]>`.
- `WeatherDashboardComponent` shows loading, error, retry, summary metrics, filtering, Material table sorting, and row detail selection.
- The layout is responsive with horizontal table scrolling on narrow screens and stacked summary/detail bands on mobile.

## Assumptions

- Dallas, TX coordinates are `32.78, -96.8`, matching the exercise prompt.
- Temperatures are displayed in Celsius because Open-Meteo returns metric units by default.
- `dates.txt` is part of the API project and is copied to output for publish/run scenarios.
- Local JSON storage is adequate for the exercise; it is not intended as a multi-instance production cache.

## Suggested Commit Breakdown

1. Scaffold Clean Architecture solution and project references.
2. Add domain result model and weather data validation.
3. Implement application date parsing strategies and weather use case.
4. Add infrastructure file storage, date input, Open-Meteo client, Polly, and Serilog wiring.
5. Add API controller, configuration, and sample `dates.txt`.
6. Add backend unit tests.
7. Scaffold Angular 21 UI with Material.
8. Implement Angular weather service, dashboard table, loading, error, filter, and sorting.
9. Add README and AI notes.
10. Final verification and cleanup.

## Production Improvement Ideas

- Replace local file storage with a durable store such as PostgreSQL, SQL Server, or cloud blob storage.
- Add distributed cache locking so multiple API instances do not fetch/write the same date concurrently.
- Add OpenTelemetry traces and metrics around cache hit rate, API latency, retry count, and failed dates.
- Add request correlation IDs and structured logs for each date processed.
- Add API contract tests and UI end-to-end tests with Playwright.
- Add rate limiting and a background refresh job for known date sets.
- Add configurable units and location selection.
- Add health checks for the dates file path, storage path, and Open-Meteo connectivity.
