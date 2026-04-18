using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Usuario.Tests;

public class AuthLoginTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable {
	private readonly WebApplicationFactory<Program> _factory;
	private readonly HttpClient _client;

	public AuthLoginTests(WebApplicationFactory<Program> factory) {
		_factory = factory.WithWebHostBuilder(builder => {
			builder.UseSetting("Bearer:SecretKey", "test-secret-key-at-least-32-chars!!");
			builder.UseSetting("FixedUsers:JsonAbsolutePath",
				Path.Combine(AppContext.BaseDirectory, "test-users.json"));
		});
		_client = _factory.CreateClient();
	}

	public void Dispose() => _client.Dispose();

	[Fact]
	public async Task Login_con_credenciales_validas_devuelve_token() {
		var response = await _client.PostAsJsonAsync("/api/auth/login", new {
			email = "tester@local",
			password = "secret123"
		});

		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
		Assert.True(doc.RootElement.TryGetProperty("token", out var token));
		Assert.False(string.IsNullOrEmpty(token.GetString()));
		Assert.True(doc.RootElement.TryGetProperty("user", out var user));
		Assert.Equal("Tester", user.GetProperty("name").GetString());
		Assert.True(user.GetProperty("isHost").GetBoolean());
	}

	[Fact]
	public async Task Login_con_credenciales_invalidas_devuelve_401() {
		var response = await _client.PostAsJsonAsync("/api/auth/login", new {
			email = "tester@local",
			password = "mala"
		});

		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}
}
