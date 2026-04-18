using Microsoft.Extensions.Diagnostics.HealthChecks;
using Usuario.API.Services;

namespace Usuario.API.Health;

public sealed class FixedUsersHealthCheck : IHealthCheck {
	private readonly IFixedUserCatalog _catalog;

	public FixedUsersHealthCheck(IFixedUserCatalog catalog) {
		_catalog = catalog;
	}

	public Task<HealthCheckResult> CheckHealthAsync(
		HealthCheckContext context,
		CancellationToken cancellationToken = default) {
		if (_catalog.Users.Count == 0) {
			return Task.FromResult(HealthCheckResult.Degraded("No hay usuarios definidos en el JSON."));
		}

		return Task.FromResult(HealthCheckResult.Healthy());
	}
}
