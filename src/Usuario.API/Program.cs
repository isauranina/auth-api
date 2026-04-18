using System.Text;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using Usuario.API.Health;
using Usuario.API.Middlewares;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Usuario.API.Options;
using Usuario.API.Services;

var builder = WebApplication.CreateBuilder(args);

var lokiUrl =
	Environment.GetEnvironmentVariable("LOKI_URL")
	?? builder.Configuration["Loki:Url"]
	?? "http://localhost:3100";

builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
	.ReadFrom.Configuration(context.Configuration)
	.ReadFrom.Services(services)
	.Enrich.FromLogContext()
	.Enrich.WithMachineName()
	.Enrich.WithProcessId()
	.Enrich.WithThreadId()
	.WriteTo.Console()
	.WriteTo.GrafanaLoki(lokiUrl));

builder.Services.AddControllers();
builder.Services.Configure<FixedUsersOptions>(builder.Configuration.GetSection(FixedUsersOptions.SectionName));
builder.Services.AddSingleton<IFixedUserCatalog, JsonFixedUserCatalog>();

builder.Services.AddApiVersioning(config => {
	config.AssumeDefaultVersionWhenUnspecified = true;
	config.DefaultApiVersion = new ApiVersion(1, 0);
	config.ReportApiVersions = true;
	config.ApiVersionReader = ApiVersionReader.Combine(
		new QueryStringApiVersionReader("api-version"),
		new HeaderApiVersionReader("X-Version"),
		new MediaTypeApiVersionReader("ver"));
});

builder.Services.AddVersionedApiExplorer(options => {
	options.GroupNameFormat = "'v'VVV";
	options.SubstituteApiVersionInUrl = false;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, Usuario.API.Swagger.ConfigureSwaggerOptions>();

var jwtSecretKey =
	Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
	?? builder.Configuration["Bearer:SecretKey"]
	?? throw new InvalidOperationException("La clave secreta JWT no está configurada. Define JWT_SECRET_KEY o Bearer:SecretKey.");

builder.Services.AddAuthentication("Bearer")
	.AddJwtBearer("Bearer", options => {
		options.TokenValidationParameters = new TokenValidationParameters {
			ValidateIssuer = false,
			ValidateAudience = false,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
			ClockSkew = TimeSpan.Zero
		};

		if (builder.Environment.IsDevelopment()) {
			options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents {
				OnAuthenticationFailed = context => {
					Console.WriteLine($"Error de autenticación: {context.Exception.Message}");
					return Task.CompletedTask;
				}
			};
		}
	});

builder.Services.AddAuthorization();

builder.Services.AddHealthChecks()
	.AddCheck("live", () => HealthCheckResult.Healthy(), tags: ["live"])
	.AddCheck<FixedUsersHealthCheck>("fixed_users", tags: ["ready"]);

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy", policy =>
	policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.WebHost.UseUrls("http://0.0.0.0:5000");

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseCors("CorsPolicy");
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSwagger();
var apiExplorer = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwaggerUI(options => {
	foreach (var description in apiExplorer.ApiVersionDescriptions) {
		options.SwaggerEndpoint(
			$"/swagger/{description.GroupName}/swagger.json",
			description.GroupName.ToUpperInvariant());
	}

	options.RoutePrefix = "swagger";
	options.DocumentTitle = "Usuario API — Swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions {
	Predicate = r => r.Tags.Contains("live")
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions {
	Predicate = r => r.Tags.Contains("ready")
}).AllowAnonymous();

app.MapControllers();

app.Run();

// Para pruebas de integración con WebApplicationFactory
public partial class Program { }
