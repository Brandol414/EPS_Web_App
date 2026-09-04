using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Identity.Core;
using System.Security.Claims;

namespace EPS_Web_App.Services;

public sealed class AdminAuthenticationService
{
	private readonly ApplicationDbContext _context;

	private readonly PasswordHasher<AdminUser>
		_passwordHasher;


	public AdminAuthenticationService(
		ApplicationDbContext context)
	{
		_context = context;

		_passwordHasher =
			new PasswordHasher<AdminUser>();
	}


	// =========================================================
	// CREATE USER
	// =========================================================

	public async Task<
		(bool Success, string? Error)>
		CreateUserAsync(
			string username,
			string displayName,
			string password,
			bool administrator)
	{
		username =
			username.Trim();

		displayName =
			displayName.Trim();


		if (string.IsNullOrWhiteSpace(username))
		{
			return (
				false,
				"Username is required."
			);
		}


		if (string.IsNullOrWhiteSpace(displayName))
		{
			return (
				false,
				"Display name is required."
			);
		}


		if (string.IsNullOrWhiteSpace(password))
		{
			return (
				false,
				"Password is required."
			);
		}


		var normalizedUsername =
			username.ToUpperInvariant();


		var exists =
			await _context.AdminUsers
				.AsNoTracking()
				.AnyAsync(
					x =>
						x.Username.ToUpper()
						==
						normalizedUsername
				);


		if (exists)
		{
			return (
				false,
				"That username already exists."
			);
		}


		var user =
			new AdminUser
			{
				Username =
					username,

				DisplayName =
					displayName,

				IsAdministrator =
					administrator,

				IsActive =
					true,

				CreatedAt =
					DateTime.UtcNow
			};


		user.PasswordHash =
			_passwordHasher.HashPassword(
				user,
				password
			);


		_context.AdminUsers.Add(
			user
		);


		await _context.SaveChangesAsync();


		return (
			true,
			null
		);
	}


	// =========================================================
	// VALIDATE LOGIN
	// =========================================================

	public async Task<
		(bool Success, AdminUser? User)>
		ValidateCredentialsAsync(
			string username,
			string password)
	{
		var user =
			await _context.AdminUsers
				.FirstOrDefaultAsync(
					x =>
						x.Username ==
						username.Trim()
				);


		if (
			user == null
			||
			!user.IsActive
		)
		{
			return (
				false,
				null
			);
		}


		var result =
			_passwordHasher.VerifyHashedPassword(
				user,
				user.PasswordHash,
				password
			);


		if (
			result ==
			PasswordVerificationResult.Failed
		)
		{
			return (
				false,
				null
			);
		}


		user.LastLoginAt =
			DateTime.UtcNow;


		await _context.SaveChangesAsync();


		return (
			true,
			user
		);
	}


	// =========================================================
	// SIGN IN
	// =========================================================

	public async Task SignInAsync(
		HttpContext httpContext,
		AdminUser user,
		bool rememberMe)
	{
		var claims =
			new List<Claim>
			{
				new(
					ClaimTypes.NameIdentifier,
					user.AdminUserId.ToString()
				),

				new(
					ClaimTypes.Name,
					user.Username
				),

				new(
					"DisplayName",
					user.DisplayName
				)
			};


		if (user.IsAdministrator)
		{
			claims.Add(
				new Claim(
					ClaimTypes.Role,
					"Administrator"
				)
			);
		}


		var identity =
			new ClaimsIdentity(
				claims,
				CookieAuthenticationDefaults.AuthenticationScheme
			);


		var principal =
			new ClaimsPrincipal(
				identity
			);


		await httpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			principal,
			new AuthenticationProperties
			{
				IsPersistent =
					rememberMe,

				ExpiresUtc =
					rememberMe
						? DateTimeOffset.UtcNow.AddDays(14)
						: DateTimeOffset.UtcNow.AddHours(8),

				AllowRefresh =
					true
			}
		);
	}


	// =========================================================
	// SIGN OUT
	// =========================================================

	public async Task SignOutAsync(
		HttpContext httpContext)
	{
		await httpContext.SignOutAsync(
			CookieAuthenticationDefaults.AuthenticationScheme
		);
	}
}