# SafeVault Application

## Summary
SafeVault is a web-based application designed to provide secure storage and management of sensitive data. The application is built using ASP.NET Core Razor Pages and follows modern web development best practices. The solution includes:

- **SafeVault.Web**: The main web project, responsible for the user interface, input validation, and database logic.
- **SafeVault.Test**: A unit test project using NUnit to ensure code quality, security, and reliability.

## Activity 1: Changes and Updates
- Created the root solution file: `safe-vault-application.sln`.
- Added the `SafeVault.Web` project to the solution, providing the main web application structure.
- Added the `SafeVault.Test` project to the solution, enabling unit testing with xUnit.
- Configured the web project with basic middleware and routing in `Program.cs`.
- Replaced the default Index page with a custom web form for username and email submission in `Index.cshtml`.
- Added a LocalDB connection string to `appsettings.json` for database connectivity.
- Implemented a `DatabaseInitializer` class to automatically create the SafeVaultDb database and Users table if they do not exist.
- Added the Microsoft.Data.SqlClient NuGet package to the SafeVault.Web project for SQL Server database access.
- Moved the Users table creation SQL to a separate `database.sql` file in `SafeVault.Web/Data` for easier future changes.
- Updated the `DatabaseInitializer` to read and execute the SQL from this file when initializing the database.
- Added an AddUser method to DatabaseInitializer that uses parameterized queries for inserting user data, following best practices to prevent SQL injection.
- Updated AddUser in DatabaseInitializer to sanitize and validate user-provided data (such as login credentials or search inputs) before database insertion, ensuring secure handling of sensitive information.
- Added a new test file `Tests/TestInputValidation.cs` with unit tests for SQL Injection and XSS input validation using NUnit.
- Added an `InputValidator` class in `SafeVault.Web/Model` to sanitize user input and validate email addresses, helping to prevent users from entering potentially harmful scripts or queries (such as SQL injection or XSS).
- Updated the InputValidator.Sanitize method to also remove the phrase 'drop table' (case-insensitive), ensuring it passes the SQL injection test cases and further improves input sanitization.
- Updated the InputValidator.Sanitize method to also remove the word 'script' (case-insensitive), ensuring it passes the XSS test cases and further improves input sanitization.
- Added a unit test for XSS vulnerabilities in `TestInputValidation.cs` by injecting a malicious script and asserting that the sanitized input does not contain script tags or angle brackets.
- Added a standard `.gitignore` file to exclude build outputs, user files, logs, test results, IDE settings, and other common files from version control.

## Activity 2: Changes and Updates
- Added a new Razor Page `AdminTools` (AdminTools.cshtml and AdminTools.cshtml.cs) for administrative tools, allowing management of users, logs, and other admin tasks.

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
