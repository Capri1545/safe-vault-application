using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SafeVault.Test.Tests
{
    [TestFixture]
    public class AuthApiTests
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

        [Test]
        public async Task InvalidLogin_ReturnsUnauthorized()
        {
            Assert.That(_client, Is.Not.Null, "_client should not be null");
            var response = await _client.PostAsJsonAsync(
                _apiBase + "/login",
                new { Username = "notarealuser", Password = "wrongpass" }
            );
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task EmptyLogin_ReturnsBadRequest()
        {
            Assert.That(_client, Is.Not.Null, "_client should not be null");
            var response = await _client.PostAsJsonAsync(
                _apiBase + "/login",
                new { Username = "", Password = "" }
            );
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task UnauthorizedAccess_AdminEndpoint_ReturnsUnauthorized()
        {
            Assert.That(_client, Is.Not.Null, "_client should not be null");
            var response = await _client.GetAsync(_apiBase + "/users");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task AdminAccess_AdminEndpoint_ReturnsOk()
        {
            Assert.That(_client, Is.Not.Null, "_client should not be null");
            var login = await _client!.PostAsJsonAsync(
                _apiBase + "/login",
                new { Username = "admin", Password = "password123" }
            );
            var loginResult = await login.Content.ReadFromJsonAsync<LoginResult>();
            Assert.That(loginResult, Is.Not.Null, "Login result should not be null");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
            var response = await _client.GetAsync(_apiBase + "/users");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task UserAccess_AdminEndpoint_ReturnsForbidden()
        {
            Assert.That(_client, Is.Not.Null, "_client should not be null");
            var login = await _client!.PostAsJsonAsync(
                _apiBase + "/login",
                new { Username = "user1", Password = "userpass" }
            );
            var loginResult = await login.Content.ReadFromJsonAsync<LoginResult>();
            Assert.That(loginResult, Is.Not.Null, "Login result should not be null");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
            var response = await _client.GetAsync(_apiBase + "/users");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        }

        private class LoginResult
        {
            public string Token { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}
