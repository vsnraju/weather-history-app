
# V1 Design Notes

## Goal

Build a small full-stack application that:

- Reads dates from a text file
- Parses multiple date formats
- Calls Open-Meteo historical weather API
- Stores weather responses locally as JSON
- Exposes a backend API
- Displays weather data in Angular UI

---

# Initial Architecture

## Backend Stack

- ASP.NET Core .NET 10 Web API
- Clean Architecture
- xUnit + Moq
- Serilog
- Polly retry policies

## Frontend Stack

- Angular 21
- Angular Material
- Standalone Components
- RxJS

---

# Initial Project Structure

src/
 ├── WeatherApp.Api
 ├── WeatherApp.Application
 ├── WeatherApp.Domain
 ├── WeatherApp.Infrastructure
 └── WeatherApp.UI

tests/
 ├── WeatherApp.Application.Tests
 ├── WeatherApp.Infrastructure.Tests
 └── WeatherApp.Api.Tests

---

# Initial Design Decisions

## 1. Clean Architecture

I chose Clean Architecture to separate:

* Business logic
* Infrastructure concerns
* API concerns
* UI concerns

This improves:

* maintainability
* testability
* separation of concerns

---

# 2. Strategy Pattern For Date Parsing (Initial Idea)

Initially, I considered using the Strategy Pattern because the input file contains multiple date formats.

Example formats:

* 02/27/2021
* June 2, 2022
* Jul-13-2020

Initial design:

IDateParserStrategy
 ├── SlashDateParser
 ├── LongMonthDateParser
 └── ShortMonthDateParser

The parsing service would iterate through strategies until one succeeded.

Example flow:

foreach (var strategy in _strategies)
{
    var result = strategy.TryParse(input);

    if (result.IsSuccess)
        return result;
}

---

# Why I Reconsidered This

After reviewing the requirements, I realized:

* Only 3 known formats exist
* Parsing complexity is low
* Strategy Pattern added unnecessary abstraction
* Performance impact was negligible but design became more complex

I later simplified this in V2 using:


DateTime.TryParseExact(...)

with a supported formats array.

This reduced:

* code complexity
* number of classes
* dependency injection registrations

while keeping the solution extensible.

---

# 3. Repository Pattern

I planned to use Repository Pattern for local file storage.

Interface:


IWeatherRepository


Responsibilities:

* Save weather JSON
* Read cached weather JSON
* Check if file exists

Benefits:

* testability
* separation from infrastructure
* future extensibility

---

# 4. External API Adapter

To isolate Open-Meteo integration:


IWeatherApiClient


Implementation:


OpenMeteoApiClient

Benefits:

* mockable in tests
* replaceable
* cleaner architecture boundary

---

# 5. Result Pattern

Instead of throwing exceptions for validation errors:

Result<T>

Used for:

* invalid dates
* API failures
* missing data

Benefits:

* predictable flow
* cleaner error handling
* easier unit testing

---

# 6. Startup Processing Flow

Initial plan:

Application Startup
    ↓
Read dates.txt
    ↓
Parse dates
    ↓
Validate
    ↓
Check cache
    ↓
Call Open-Meteo API
    ↓
Store JSON locally


The backend API would then aggregate results from local storage.

---

# 7. UI Design

Initial Angular UI requirements:

* Load weather data from backend
* Display table
* Show loading spinner
* Show API error message
* Allow sorting by date or temperature

Planned Angular Material usage:

* mat-table
* mat-sort
* mat-spinner
* snackbar

---

# 8. Initial Error Handling Plan

Handle gracefully:

* invalid dates
* network failures
* API timeouts
* empty API responses
* malformed JSON

No crashes should occur during processing.

---

# 9. Initial Testing Plan

Planned tests:

## Unit Tests

* Date parsing
* Weather service
* Repository
* API client

## Integration Tests

* API endpoint tests

## Edge Cases

* invalid dates
* empty files
* duplicate cached files
* API failures

---

# 10. Production Improvements Identified

If this were production-ready, I would add:

* Redis distributed cache
* Docker support
* CI/CD pipeline
* OpenTelemetry tracing
* Health checks
* Background queue processing
* Authentication/authorization
* Distributed file/object storage

---

# Lessons From V1

Main realization:

Not every problem requires a design pattern.

Although Strategy Pattern was technically valid, simplifying the date parsing logic improved:

* readability
* maintainability
* development speed

while still satisfying the requirements cleanly.
