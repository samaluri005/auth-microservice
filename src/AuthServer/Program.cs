using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

// CONFIG: read DB connection from env or appsettings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
					   ?? "Host=localhost;Port=5432;Database=authdb;Username=postgres;Password=postgres";

// Add DbContext (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseNpgsql(connectionString);
	// register the OpenIddict entities
	options.UseOpenIddict();
});

// Identity-like user store (simple custom)
builder.Services.AddScoped<IUserStore, EfUserStore>();

// Authentication (cookie for interactive sign-in)
builder.Services.AddAuthentication(options =>
{
	options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
	options.LoginPath = "/account/login";
});

// Configure OpenIddict
builder.Services.AddOpenIddict()
	.AddCore(options =>
	{
		// Use the EF Core stores/models
		options.UseEntityFrameworkCore()
			   .UseDbContext<ApplicationDbContext>();
	})
	.AddServer(options =>
	{
		// Enable the authorization, token, introspection, revocation, userinfo endpoints
		options.SetAuthorizationEndpointUris("/connect/authorize")
			   .SetTokenEndpointUris("/connect/token")
			   .SetUserinfoEndpointUris("/connect/userinfo")
			   .SetIntrospectionEndpointUris("/connect/introspect")
			   .SetRevocationEndpointUris("/connect/revoke");

		// Allow authorization code + PKCE and refresh token
		options.AllowAuthorizationCodeFlow()
			   .RequireProofKeyForCodeExchange()
			   .AllowRefreshTokenFlow();

		// Accept form post & client credentials (for machine clients)
		options.AcceptAnonymousClients(); // for dev; in prod configure clients explicitly

		// Encryption & signing credentials
		// For production: use Azure Key Vault / HSM
		options.AddDevelopmentEncryptionCertificate()
			   .AddDevelopmentSigningCertificate();

		// Register ASP.NET Core host and enable token endpoint passthrough for custom logic if needed
		options.UseAspNetCore()
			   .EnableTokenEndpointPassthrough()
			   .EnableAuthorizationEndpointPassthrough()
			   .EnableStatusCodePagesIntegration();
	})
	.AddValidation(options =>
	{
		// Use the default token validation (local)
		options.UseLocalServer();
		options.UseAspNetCore();
	});

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Run DB migrations at startup (development convenience — replace for prod)
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	db.Database.Migrate();

	// Seed a test client if not present (dev convenience)
	var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
	if (await manager.FindByClientIdAsync("react_app") == null)
	{
		await manager.CreateAsync(new OpenIddictApplicationDescriptor
		{
			ClientId = "react_app",
			DisplayName = "React SPA (Dev)",
			RedirectUris = { new Uri("https://localhost:3000/callback") },
			Permissions =
			{
				Permissions.Endpoints.Authorization,
				Permissions.Endpoints.Token,
				Permissions.GrantTypes.AuthorizationCode,
				Permissions.GrantTypes.RefreshToken,
				Permissions.ResponseTypes.Code
			},
			Requirements = { Requirements.Features.ProofKeyForCodeExchange }
		});
	}
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Minimal endpoint for well-known discovery (OpenIddict serves metadata automatically at /.well-known/openid-configuration)
// Expose healthcheck
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
