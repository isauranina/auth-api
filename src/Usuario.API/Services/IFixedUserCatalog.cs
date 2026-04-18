using Usuario.API.Models;

namespace Usuario.API.Services;

public interface IFixedUserCatalog {
	IReadOnlyList<FixedUser> Users { get; }

	FixedUser? FindByCredentials(string email, string password);
}
