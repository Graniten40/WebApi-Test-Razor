User seacret
Password=str0ng!Passw0rd123


## Reflections / Challenges

During development, the following parts were the most challenging:

- **User-based DbContext configuration**  
  Selecting database connections dynamically based on JWT claims required careful handling of HttpContext availability, especially for design-time operations (EF migrations) and background execution.

- **EF Core design-time configuration**  
  Since Program.cs is not executed during EF Core design-time, a custom setup was needed to load configuration and secrets correctly for migrations.

- **Separation between API and Razor client**  
  Keeping consistent routes, DTOs, and error handling between the Web API and the Razor Pages client required extra attention to avoid runtime mismatches.

These challenges improved my understanding of dependency injection, configuration pipelines, and environment-specific behavior in ASP.NET Core.

If more time was available, the error handling could be improved by returning more precise HTTP status codes (e.g. 404 instead of 400) and introducing global exception handling.
