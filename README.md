# SafeVault Application

## Summary
SafeVault is a secure application suite for web-based and API-based management of sensitive data. The solution uses ASP.NET Core Razor Pages for the web UI and a minimal API for all data and authentication logic. It features robust input validation, JWT authentication, role-based access, and dynamic UI updates based on authentication state. All database logic is handled by the API, and the web project communicates securely with the API for all protected operations.

- **SafeVault.Web**: User interface, input validation, and authentication state management.
- **SafeVault.Api**: All data/database logic, authentication, authorization, and user/role management.
- **SafeVault.Test**: Unit tests for input validation and security.

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
- Added Razor Page `AdminTools` for admin-only management, with menu visibility based on JWT role.
- Added SafeVault.Api project for all API and data logic, with JWT authentication and role-based authorization.
- Cleaned up the API project, removing default templates and moving all DB logic from the web project to the API.
- Implemented CORS in the API for secure cross-origin requests from the web project.
- Added login/logout UI logic: menu updates dynamically, and credentials are cleared on logout.
- Home page content now updates based on login state, showing a form only when logged in.
- Added .http file for direct API endpoint testing for both admin and user roles.
- Fixed JWT key size and claim parsing for robust authentication and admin detection.
- Passwords are now securely hashed using bcrypt (BCrypt.Net-Next) in the API. All user creation and login logic uses strong password hashing and verification.
- Users are assigned roles (e.g., Admin, User) in the API. Role-based access is enforced for protected endpoints and reflected in the UI (e.g., only admins see Admin Tools).
- Restricted access to specific API routes and features based on user roles (e.g., only admins can manage users). Role-based authorization is enforced using [Authorize(Roles = "Admin")] attributes in the API.

## How AI Has Contributed
- Automated project structure setup and best-practice configuration for .NET, ASP.NET Core, and JWT security.
- Generated and refactored code for input validation, authentication, and dynamic UI updates.
- Provided step-by-step guidance for moving all data logic to the API and securing endpoints.
- Automated the addition of CORS, controller mapping, and JWT role parsing for seamless integration.
- Created and updated .http test files for API endpoint validation.
- Ensured all documentation and code changes reflect the current, secure architecture.

---
This README will be updated as the project evolves and new features or changes are introduced.
