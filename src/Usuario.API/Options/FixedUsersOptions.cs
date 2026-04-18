namespace Usuario.API.Options;

public sealed class FixedUsersOptions {
	public const string SectionName = "FixedUsers";

	/// <summary>
	/// Ruta relativa al directorio de contenido (por defecto Data/users.json).
	/// </summary>
	public string JsonRelativePath { get; set; } = "Data/users.json";

	/// <summary>
	/// Ruta absoluta opcional (p. ej. volumen Docker). Tiene prioridad sobre JsonRelativePath.
	/// </summary>
	public string? JsonAbsolutePath { get; set; }
}
