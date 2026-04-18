namespace Usuario.API.Models;

public sealed class FixedUser {
	public string Name { get; init; } = string.Empty;
	public string Lastname { get; init; } = string.Empty;
	public string Address { get; init; } = string.Empty;
	public string Email { get; init; } = string.Empty;
	public string Password { get; init; } = string.Empty;
	public bool IsHost { get; init; }
	public bool IsGuest { get; init; }

	public FixedUserPublic ToPublic() => new(
		Name,
		Lastname,
		Address,
		Email,
		IsHost,
		IsGuest);
}

public sealed record FixedUserPublic(
	string Name,
	string Lastname,
	string Address,
	string Email,
	bool IsHost,
	bool IsGuest);
