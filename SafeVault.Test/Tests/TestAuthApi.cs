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
                new { Username = "admin", Password = "admin" }
            );
            var loginResult = await login.Content.ReadFromJsonAsync<LoginResult>();
            Assert.That(loginResult, Is.Not.Null, "Login result should not be null");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
            var response = await _client.GetAsync(_apiBase + "/users");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task AddAndDeleteUser_WorksWithDatabase()
        {
            // Login as admin
            Assert.That(_client, Is.Not.Null, "_client should not be null");
            var login = await _client!.PostAsJsonAsync(
                _apiBase + "/login",
                new { Username = "admin", Password = "admin" }
            );
            var loginResult = await login.Content.ReadFromJsonAsync<LoginResult>();
            Assert.That(loginResult, Is.Not.Null, "Login result should not be null");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

            // Add user
            var newUser = new
            {
                username = "testuser_db",
                password = "Test123!",
                role = "User",
                email = "testuser_db@example.com"
            };
            var addResp = await _client.PostAsJsonAsync(_apiBase + "/add-user", newUser);
            Assert.That(addResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), "User creation failed");

            // Verify user appears in list
            var usersResp = await _client.GetAsync(_apiBase + "/users");
            Assert.That(usersResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var usersJson = await usersResp.Content.ReadAsStringAsync();
            Assert.That(
                usersJson,
                Does.Contain("testuser_db"),
                "User not found in list after creation"
            );

            // Delete user
            var delResp = await _client.DeleteAsync(_apiBase + "/delete-user/testuser_db");
            Assert.That(delResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), "User deletion failed");

            // Verify user no longer in list
            var usersResp2 = await _client.GetAsync(_apiBase + "/users");
            Assert.That(usersResp2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var usersJson2 = await usersResp2.Content.ReadAsStringAsync();
            Assert.That(
                usersJson2,
                Does.Not.Contain("testuser_db"),
                "User still found in list after deletion"
            );
        }

        [Test]
        public async Task AddUser_ThenUserAccess_AdminEndpoint_ReturnsForbidden_AndDeleteUser()
        {
            // Login as admin
            Assert.That(_client, Is.Not.Null, "_client should not be null");
            var login = await _client!.PostAsJsonAsync(
                _apiBase + "/login",
                new { Username = "admin", Password = "admin" }
            );
            var loginResult = await login.Content.ReadFromJsonAsync<LoginResult>();
            Assert.That(loginResult, Is.Not.Null, "Login result should not be null");
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);

            // Add user
            var newUser = new
            {
                username = "testuser_forbidden",
                password = "Test123!",
                role = "User",
                email = "testuser_forbidden@example.com"
            };
            var addResp = await _client.PostAsJsonAsync(_apiBase + "/add-user", newUser);
            Assert.That(addResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), "User creation failed");

            // Login as the new user
            var userClient = new HttpClient();
            var userLogin = await userClient.PostAsJsonAsync(
                _apiBase + "/login",
                new { Username = "testuser_forbidden", Password = "Test123!" }
            );
            var userLoginResult = await userLogin.Content.ReadFromJsonAsync<LoginResult>();
            Assert.That(userLoginResult, Is.Not.Null, "User login result should not be null");
            userClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userLoginResult!.Token);

            // Attempt to access admin endpoint as user
            var forbiddenResp = await userClient.GetAsync(_apiBase + "/users");
            Assert.That(
                forbiddenResp.StatusCode,
                Is.EqualTo(HttpStatusCode.Forbidden),
                "User should not access admin endpoint"
            );
            userClient.Dispose();

            // Delete user as admin
            var delResp = await _client.DeleteAsync(_apiBase + "/delete-user/testuser_forbidden");
            Assert.That(delResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), "User deletion failed");

            // Verify user no longer in list
            var usersResp2 = await _client.GetAsync(_apiBase + "/users");
            Assert.That(usersResp2.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var usersJson2 = await usersResp2.Content.ReadAsStringAsync();
            Assert.That(
                usersJson2,
                Does.Not.Contain("testuser_forbidden"),
                "User still found in list after deletion"
            );
        }

        [TearDown]
        public async Task CleanupTestUsers()
        {
            if (_client == null)
                return;
            // Login as admin to get JWT
            var login = await _client.PostAsJsonAsync(
                _apiBase + "/login",
                new { Username = "admin", Password = "admin" }
            );
            var loginResult = await login.Content.ReadFromJsonAsync<LoginResult>();
            if (loginResult == null)
                return;
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.Token);
            // Try to delete known test users (ignore errors)
            await _client.DeleteAsync(_apiBase + "/delete-user/testuser_db");
            await _client.DeleteAsync(_apiBase + "/delete-user/testuser_forbidden");
        }

        private class LoginResult
        {
            public string Token { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }
    }
}
