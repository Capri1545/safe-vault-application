using NUnit.Framework;
using SafeVault.Web.Model;
using static NUnit.Framework.Assert;

namespace SafeVault.Test.Tests
{
    [TestFixture]
    public class TestInputValidation
    {
        [Test]
        public void TestForSQLInjection()
        {
            // Simulate SQL injection input
            string maliciousInput = "test'; DROP TABLE Users;--";
            string sanitized = InputValidator.Sanitize(maliciousInput);
            // The sanitized input should not contain SQL injection characters
            Assert.That(sanitized.Contains("'"), Is.False);
            Assert.That(sanitized.Contains(";"), Is.False);
            Assert.That(sanitized.ToLower().Contains("drop table"), Is.False);
        }

        [Test]
        public void TestForXSS()
        {
            // Simulate XSS input
            string maliciousInput = "<script>alert('xss')</script>";
            string sanitized = InputValidator.Sanitize(maliciousInput);
            // The sanitized input should not contain script tags or angle brackets
            Assert.That(sanitized.Contains("<"), Is.False);
            Assert.That(sanitized.Contains(">"), Is.False);
            Assert.That(sanitized.ToLower().Contains("script"), Is.False);
        }
    }
}
