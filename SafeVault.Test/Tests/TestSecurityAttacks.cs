using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace SafeVault.Test.Tests
{
    [TestFixture]
    public class TestSecurityAttacks
    {
        private readonly string _apiBase = "http://localhost:5145/api/auth";
        private HttpClient? _client;

        [SetUp]
        public void Setup()
        {
            _client = new HttpClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client?.Dispose();
            _client = null;
        }

        private async Task<string> GetAdminJwtToken()
        {
            Assert.That(_client, Is.Not.Null, "_client should not be null");
            var login = await _client!.PostAsJsonAsync(
                _apiBase + "/login",
                new { Username = "admin", Password = "admin" }
            );
            var loginResult = await login.Content.ReadFromJsonAsync<LoginResult>();
            Assert.That(loginResult, Is.Not.Null, "Login result should not be null");
            return loginResult!.Token;
        }

        [TestCase("testuser' OR 1=1;--", "test@example.com")]
        [TestCase("admin; DROP TABLE Users;--", "attacker@example.com")]
        public async Task AddUser_ShouldNotAllowSqlInjection(string username, string email)
        {
            var jwt = await GetAdminJwtToken();
            var payload = new
            {
                username = username,
                password = "Test123!",
                role = "User",
                email = email,
            };
            var content = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Post, _apiBase + "/add-user");
            request.Headers.Add("Authorization", "Bearer " + jwt);
            request.Content = content;
            var response = await _client!.SendAsync(request);
            Assert.That(
                response.StatusCode,
                Is.Not.EqualTo(System.Net.HttpStatusCode.InternalServerError),
                "SQL injection caused server error"
            );
            Assert.That(
                response.IsSuccessStatusCode,
                Is.False,
                "Malicious user creation should not succeed!"
            );
            // Now check that the user was NOT added to the database
            var usersRequest = new HttpRequestMessage(HttpMethod.Get, _apiBase + "/users");
            usersRequest.Headers.Add("Authorization", "Bearer " + jwt);
            var usersResponse = await _client!.SendAsync(usersRequest);
            var usersJson = await usersResponse.Content.ReadAsStringAsync();
            Assert.That(
                usersJson,
                Does.Not.Contain(username),
                $"Malicious user '{username}' was inserted into the database!"
            );
        }

        [TestCase("<script>alert('xss')</script>", "xss@example.com")]
        [TestCase("xssuser", "<img src=x onerror=alert('xss')>")]
        public async Task AddUser_ShouldNotAllowXss(string username, string email)
        {
            var jwt = await GetAdminJwtToken();
            var payload = new
            {
                username = username,
                password = "Test123!",
                role = "User",
                email = email,
            };
            var content = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );
            var request = new HttpRequestMessage(HttpMethod.Post, _apiBase + "/add-user");
            request.Headers.Add("Authorization", "Bearer " + jwt);
            request.Content = content;
            var response = await _client!.SendAsync(request);
            Assert.That(
                response.StatusCode,
                Is.Not.EqualTo(System.Net.HttpStatusCode.InternalServerError),
                "XSS input caused server error"
            );
            Assert.That(
                response.IsSuccessStatusCode,
                Is.False,
                "Malicious user creation (XSS) should not succeed!"
            );
            // Now check that the user was NOT added to the database
            var usersRequest = new HttpRequestMessage(HttpMethod.Get, _apiBase + "/users");
            usersRequest.Headers.Add("Authorization", "Bearer " + jwt);
            var usersResponse = await _client!.SendAsync(usersRequest);
            var usersJson = await usersResponse.Content.ReadAsStringAsync();
            Assert.That(
                usersJson,
                Does.Not.Contain(username),
                $"Malicious user '{username}' was inserted into the database!"
            );
        }

        private class LoginResult
        {
            public string Token { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}
