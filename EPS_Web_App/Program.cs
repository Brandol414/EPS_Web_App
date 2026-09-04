using EPS_Web_App.Data;
using EPS_Web_App.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// DATABASE CONNECTION
// ============================================================

var connectionString =
	builder.Configuration.GetConnectionString(
		"EntericSurveillance"
	);

if (string.IsNullOrWhiteSpace(connectionString))
{
	throw new InvalidOperationException(
		"The 'EntericSurveillance' connection string was not found."
	);
}


// ============================================================
// ENTITY FRAMEWORK CORE
// ============================================================

builder.Services.AddDbContext<ApplicationDbContext>(
	options =>
		options.UseSqlServer(connectionString)
);


// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<LookupCatalog>();

builder.Services.AddScoped<
	AdminAuthenticationService
>();

builder.Services.AddScoped<
	ReconciliationService
>();


// ============================================================
// AUTHENTICATION
// ============================================================

builder.Services
	.AddAuthentication(
		CookieAuthenticationDefaults.AuthenticationScheme
	)
	.AddCookie(
		options =>
		{
			// -------------------------------------------------
			// AUTHENTICATION ROUTES
			// -------------------------------------------------

			options.LoginPath =
				"/Account/Login";

			options.AccessDeniedPath =
				"/Account/AccessDenied";

			options.LogoutPath =
				"/Account/Logout";


			// -------------------------------------------------
			// INACTIVITY / SESSION WINDOW
			//
			// The browser-side inactivity timer in the shared
			// layout enforces the exact 10-minute inactivity
			// requirement.
			//
			// The cookie expiration provides a server-side
			// backstop.
			// -------------------------------------------------

			options.ExpireTimeSpan =
				TimeSpan.FromMinutes(10);


			// -------------------------------------------------
			// SLIDING EXPIRATION
			//
			// Active requests renew the authentication cookie.
			// -------------------------------------------------

			options.SlidingExpiration =
				true;
		}
	);


// ============================================================
// AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization(
	options =>
	{
		options.AddPolicy(
			"AdministratorOnly",
			policy =>
			{
				policy.RequireAuthenticatedUser();

				policy.RequireRole(
					"Administrator"
				);
			}
		);
	}
);


// ============================================================
// RAZOR PAGES
// ============================================================

builder.Services.AddRazorPages(
	options =>
	{
		// ----------------------------------------------------
		// ADMINISTRATION AREA
		// ----------------------------------------------------

		options.Conventions.AuthorizeFolder(
			"/Administration",
			"AdministratorOnly"
		);


		// ----------------------------------------------------
		// ACCOUNT PAGES
		// ----------------------------------------------------

		options.Conventions.AllowAnonymousToPage(
			"/Account/Login"
		);

		options.Conventions.AllowAnonymousToPage(
			"/Account/Setup"
		);

		options.Conventions.AllowAnonymousToPage(
			"/Account/AccessDenied"
		);
	}
);


// ============================================================
// BUILD
// ============================================================

var app = builder.Build();


// ============================================================
// LOOKUP INITIALIZATION
// ============================================================

using (var scope = app.Services.CreateScope())
{
	var lookupCatalog =
		scope.ServiceProvider
			.GetRequiredService<LookupCatalog>();

	await lookupCatalog.EnsureDefaultsAsync();
}


// ============================================================
// HTTP PIPELINE
// ============================================================

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");

	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapRazorPages();

app.Run();