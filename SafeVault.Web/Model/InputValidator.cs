using System.Text.RegularExpressions;

namespace SafeVault.Web.Model
{
    public static class InputValidator
    {
        public static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            // Remove common SQL injection and XSS characters
            string sanitized = Regex.Replace(input, "[;'\"<>\\\\]", string.Empty); // Remove ; ' " < > \
            sanitized = Regex.Replace(sanitized, "--", string.Empty); // Remove SQL comment
            // Remove common SQL keywords (e.g., drop table)
            sanitized = Regex.Replace(sanitized, "(?i)drop table", string.Empty); // Remove 'drop table' case-insensitive
            // Remove script tags (case-insensitive)
            sanitized = Regex.Replace(sanitized, "(?i)script", string.Empty); // Remove 'script' case-insensitive
            sanitized = sanitized.Trim();
            return sanitized;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;
            // Simple email validation
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}
