# Current State Review

## Overview
The project follows a clean **Layered Architecture** (Controller -> Service -> Repository -> Data), adhering to modern ASP.NET Core MVC best practices. Dependency Injection is correctly implemented, and the Separation of Concerns is well-maintained.

## Strengths
- **Architecture**: The `Service -> Repository` pattern is correctly implemented, allowing for good testability and logic separation.
- **Validation**: `FluentValidation` is integrated, properly separating validation logic from Controllers and Entities.
- **Authentication**: ASP.NET Core Identity with 2FA is correctly configured.
- **Design**: Use of ViewModels successfully prevents leaking Entity models to Views.

## Areas for Improvement
- **Object Mapping**: Object mapping (Entity <-> ViewModel) is currently done manually in Controllers. This introduces boilerplate code and increases maintenance overhead.
- **Testing**: A test project `dotnet_mvc_test.Tests` exists using xUnit and EfCore InMemory database for Service layer testing. However, Controller unit tests and Integration tests are currently limited or missing.
- **Logging**: The project relies on the default `ILogger`. For production scenarios, structured logging is recommended.

## Recommended Libraries

1.  **AutoMapper** (or Mapster)
    -   *Purpose*: Automate mapping between Entities and ViewModels.
    -   *Benefit*: Reduces boilerplate code in Controllers and Services.
2.  **Serilog**
    -   *Purpose*: Structured Logging.
    -   *Benefit*: Provides powerful sinks (File, Console, Seq, etc.) helps with easier debugging.
3.  **Bogus**
    -   *Purpose*: Fake data generation.
    -   *Benefit*: Simplifies database seeding and test data creation.

## Recommended Features & Changes

The following features are recommended for the next phase of development:

*   **Expand Test Coverage**
    *   *Goal*: Ensure comprehensive system stability.
    *   *Changes*:
        *   Add Controller unit tests to verify valid/invalid model states and redirection logic.
        *   Consider adding Integration Tests using `WebApplicationFactory` to test end-to-end flows.
        *   Remove the empty `UnitTest1.cs` file.

*   **Caching Layer**
    *   *Goal*: Improve performance for high-traffic pages (specifically the public article list).
    *   *Changes*:
        *   Register `IMemoryCache` in `Program.cs`.
        *   Implement caching logic in `ArticleService` (or use a Decorator pattern) for `GetPublishedArticlesAsync`.

*   **Rich Text Editor (WYSIWYG)**
    *   *Goal*: Improve the authoring experience for admins (currently plain text/markdown source).
    *   *Changes*:
        *   Integrate a JavaScript library like **EasyMDE** (for Markdown) or **TinyMCE** in the Admin Article Create/Edit Views.
        *   Adjust views to load the editor scripts.

*   **Structured Logging**
    *   *Goal*: better observability in production.
    *   *Changes*:
        *   Install `Serilog.AspNetCore`.
        *   Configure `Log.Logger` in `Program.cs` and replace the default logging builder.
