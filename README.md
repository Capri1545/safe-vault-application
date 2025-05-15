# SafeVault Application

## Summary
SafeVault is a secure application suite designed to provide web-based and API-based storage and management of sensitive data. The solution is built using ASP.NET Core Razor Pages for the web interface, a minimal API project for extensibility, and follows modern web development best practices. It includes robust input validation, security-focused unit testing, and administrative tools for management.

- **SafeVault.Web**: The main web project, responsible for the user interface and input validation.
- **SafeVault.Test**: A unit test project using NUnit to ensure code quality, security, and reliability.
- **SafeVault.Api**: A minimal API project, responsible for all data/database logic and ready for secure API development and integration.

## Activity 1: Changes and Updates
- Created the root solution file: `safe-vault-application.sln`.
- Added the `SafeVault.Web` project to the solution, providing the main web application structure.
- Added the `SafeVault.Test` project to the solution, enabling unit testing with xUnit.
- Configured the web project with basic middleware and routing in `Program.cs`.
- Replaced the default Index page with a custom web form for username and email submission in `Index.cshtml`.
- (Removed) LocalDB connection string from web `appsettings.json` (now only in API).
- (Removed) `DatabaseInitializer` and all data logic from the web project. Data/database logic is now handled exclusively by the API.
- (Removed) Microsoft.Data.SqlClient NuGet package from the web project (now only in API).
- (Removed) Users table creation SQL and related files from the web project. These are now in the API project.
- (Removed) AddUser and all direct DB access from the web project. All user/data operations are now performed via the API.
- Added a new test file `Tests/TestInputValidation.cs` with unit tests for SQL Injection and XSS input validation using NUnit.
- Added an `InputValidator` class in `SafeVault.Web/Model` to sanitize user input and validate email addresses, helping to prevent users from entering potentially harmful scripts or queries (such as SQL injection or XSS).
- Updated the InputValidator.Sanitize method to also remove the phrase 'drop table' (case-insensitive), ensuring it passes the SQL injection test cases and further improves input sanitization.
- Updated the InputValidator.Sanitize method to also remove the word 'script' (case-insensitive), ensuring it passes the XSS test cases and further improves input sanitization.
- Added a unit test for XSS vulnerabilities in `TestInputValidation.cs` by injecting a malicious script and asserting that the sanitized input does not contain script tags or angle brackets.
- Added a standard `.gitignore` file to exclude build outputs, user files, logs, test results, IDE settings, and other common files from version control.

## Activity 2: Changes and Updates
- Added a new Razor Page `AdminTools` (AdminTools.cshtml and AdminTools.cshtml.cs) for administrative tools, allowing management of users, logs, and other admin tasks.
- Added the SafeVault.Api project to the root solution for unified API and web development.
- Cleaned up the SafeVault.Api project by removing all default code (weather, counter, etc.) and resetting configuration files to minimal/empty state for a bare minimum API setup.
- Added authentication and authorization middleware to the SafeVault.Api project to enable robust access control and prevent unauthorized access to sensitive data.
- Added an AuthController to the API project with a /api/auth/login endpoint that verifies user credentials during login (authentication). This is a placeholder for real authentication logic.
- Restricted access to the administrative tools page by requiring the user to have the 'Admin' role using the [Authorize(Roles = "Admin")] attribute.
- Added a new Login page (Login.cshtml and Login.cshtml.cs) to the web project, providing a UI for users to log in by calling the API's /api/auth/login endpoint and displaying validation results.
- Added a Login link to the navigation menu for easy access.
- Completed JWT authentication setup in `SafeVault.Api/Program.cs` using a symmetric security key and best practices for token validation.
- Updated `AuthController` in the API to generate and return a JWT token on successful login, including username and role claims.
- Added a sample protected endpoint (`/api/auth/protected`) that requires JWT authentication and returns the current user's name and role.

## How AI Has Contributed
- Guided the setup of the solution and projects using best practices for .NET and ASP.NET Core.
- Provided step-by-step terminal commands for adding and configuring projects in the solution.
- Generated boilerplate code for the test project and ensured it is correctly referenced in the solution.
- Offered explanations and summaries for each step, improving project documentation and onboarding.
- Automated the replacement of the Index page with a user-provided web form.
- Added a LocalDB connection string to the configuration for database support.
- Automated the creation of input validation and security-focused unit tests.
- Automated the addition of a .gitignore file for best practices in source control.

---
This README will be updated as the project evolves and new features or changes are introduced.
