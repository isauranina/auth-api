using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Usuario.API.Swagger;

public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions> {
	private readonly IApiVersionDescriptionProvider _provider;

	public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) {
		_provider = provider;
	}

	public void Configure(SwaggerGenOptions options) {
		foreach (var description in _provider.ApiVersionDescriptions) {
			options.SwaggerDoc(description.GroupName, new OpenApiInfo {
				Title = "Usuario API — Autenticación",
				Version = description.ApiVersion.ToString(),
				Description = "Microservicio de login y emisión de tokens JWT (usuarios fijos en JSON)."
			});
		}

		options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
			Description = "JWT en el header Authorization: Bearer {token}",
			Name = "Authorization",
			In = ParameterLocation.Header,
			Type = SecuritySchemeType.Http,
			Scheme = "Bearer",
			BearerFormat = "JWT"
		});

		options.AddSecurityRequirement(new OpenApiSecurityRequirement {
			{
				new OpenApiSecurityScheme {
					Reference = new OpenApiReference {
						Type = ReferenceType.SecurityScheme,
						Id = "Bearer"
					}
				},
				Array.Empty<string>()
			}
		});
	}
}
