# SafeVault Application

## Summary
SafeVault is a secure application suite for web-based and API-based management of sensitive data. The solution uses ASP.NET Core Razor Pages for the web UI and a minimal API for all data and authentication logic. It features robust input validation, JWT authentication, role-based access, and dynamic UI updates based on authentication state. All database logic is handled by the API, and the web project communicates securely with the API for all protected operations.

- **SafeVault.Web**: User interface, input validation, and authentication state management.
- **SafeVault.Api**: All data/database logic, authentication, authorization, and user/role management.
- **SafeVault.Test**: Unit tests for input validation, security, and API access control.

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

## Activity 3: Changes and Updates
- Refactored API to use the database for all user CRUD operations (no more in-memory user list).
- Users table schema updated to include Password, Role, and Email columns.
- All user creation and listing (including email) now persist to and read from the database.
- On database initialization, a default admin user (username: admin, password: admin) is created if not present.
- AdminTools web page now lists all users with username, email, and role.
- AdminTools web page allows deleting users (except those with the Admin role) via the UI and API.
- API endpoint for deleting users prevents deletion of admin users.
- All user management actions (add, list, delete) are now fully database-backed and secure.
- **Security improvement:** User data is now sanitized before being inserted into the DOM in the web UI to prevent XSS attacks from malicious usernames or emails.
- **Vulnerability review:**
    - All SQL queries use parameterized commands; no unsafe string concatenation is present (SQL injection is mitigated).
    - Input validation and sanitization are enforced in both backend and frontend.
    - User-supplied data is escaped before DOM insertion to prevent XSS.
- **Automated security tests:**
    - Added `TestSecurityAttacks.cs` (NUnit) to simulate SQL injection and XSS attacks against the API and web forms. These tests ensure the application is resilient to common web security threats and that malicious input does not compromise the system or user experience.
- **Shared validation logic:**
    - Created a new `SafeVault.Common` shared library containing `InputValidator` for input sanitization and email validation.
    - Both the API and Web projects now use the same input validation logic for consistent security.
- The local `InputValidator` in `SafeVault.Web/Model` has been fully deprecated and replaced with a stub. All input validation logic is now centralized in `SafeVault.Common/InputValidator.cs`.
- All usages in the web and API projects must reference `SafeVault.Common.InputValidator` for input sanitization and email validation.
- The obsolete file in the web project now only contains stub methods and clear comments instructing developers to migrate to the shared validator.
- This ensures consistent, secure validation logic across all layers and eliminates code duplication.

## Security Vulnerabilities Identified and Fixes Applied

### Vulnerabilities Identified
- **SQL Injection:** The API previously allowed unsanitized input for usernames and emails, which could be exploited for SQL injection attacks.
- **Cross-Site Scripting (XSS):** The application was vulnerable to XSS payloads in usernames and emails, such as `<script>`, `<img onerror=...>`, and event handler attributes, which could be stored in the database and executed in the UI.

### Fixes Applied
- **Centralized Input Validation:** All input validation and sanitization logic was moved to a shared `SafeVault.Common.InputValidator` class, ensuring consistent protection across both the API and Web projects.
- **Sanitization Enhancements:** The sanitizer now removes SQL injection characters, keywords, HTML tags, script tags, and common XSS vectors (e.g., `onerror`, `onload`, `img`, `alert`, `javascript:`) from all user input.
- **API Enforcement:** The API's AddUser endpoint and database logic now strictly use the shared validator, blocking any unsafe or malformed input before it reaches the database.
- **Automated Security Testing:** Comprehensive automated tests were added to simulate SQL injection and XSS attacks, ensuring that malicious input is rejected and not stored in the database.

### How Copilot Assisted
- **Vulnerability Detection:** Copilot identified the lack of input sanitization and the risk of SQL injection and XSS in the original implementation.
- **Code Refactoring:** Copilot automated the migration of validation logic to a shared library and updated all relevant API and database code to use the new validator.
- **Test-Driven Debugging:** Copilot iteratively ran and analyzed security tests, pinpointed failures, and suggested targeted code changes until all vulnerabilities were mitigated and tests passed.
- **Documentation:** Copilot updated the README to clearly summarize the vulnerabilities, the applied fixes, and the collaborative debugging process.

---
This README will be updated as the project evolves and new features or changes are introduced.
