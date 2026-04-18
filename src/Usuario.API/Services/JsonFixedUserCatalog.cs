using System.Text.Json;
using Microsoft.Extensions.Options;
using Usuario.API.Models;
using Usuario.API.Options;

namespace Usuario.API.Services;

public sealed class JsonFixedUserCatalog : IFixedUserCatalog {
	private readonly IReadOnlyList<FixedUser> _users;

	public JsonFixedUserCatalog(
		IWebHostEnvironment environment,
		IOptions<FixedUsersOptions> optionsAccessor,
		ILogger<JsonFixedUserCatalog> logger) {
		var options = optionsAccessor.Value;
		var envPath = Environment.GetEnvironmentVariable("FIXED_USERS_JSON_PATH");
		var path = !string.IsNullOrWhiteSpace(envPath)
			? envPath
			: !string.IsNullOrWhiteSpace(options.JsonAbsolutePath)
				? options.JsonAbsolutePath
				: Path.Combine(environment.ContentRootPath, options.JsonRelativePath);

		if (!File.Exists(path)) {
			throw new InvalidOperationException(
				$"No se encontró el archivo de usuarios fijos: '{path}'. " +
				"Configura FixedUsers:JsonRelativePath / JsonAbsolutePath o la variable FIXED_USERS_JSON_PATH.");
		}

		var json = File.ReadAllText(path);
		var list = JsonSerializer.Deserialize<List<FixedUser>>(json, new JsonSerializerOptions {
			PropertyNameCaseInsensitive = true
		}) ?? [];

		var grouped = list
			.GroupBy(u => u.Email, StringComparer.OrdinalIgnoreCase)
			.ToList();

		foreach (var g in grouped.Where(g => g.Count() > 1)) {
			logger.LogWarning("Email duplicado en usuarios fijos, se usa la primera entrada: {Email}", g.Key);
		}

		_users = grouped
			.Select(g => g.First())
			.ToList()
			.AsReadOnly();

		logger.LogInformation("Catálogo de usuarios fijos cargado: {Count} usuario(s).", _users.Count);
	}

	public IReadOnlyList<FixedUser> Users => _users;

	public FixedUser? FindByCredentials(string email, string password) {
		if (string.IsNullOrWhiteSpace(email) || password is null) {
			return null;
		}

		return _users.FirstOrDefault(u =>
			string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)
			&& u.Password == password);
	}
}
