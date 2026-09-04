using System.ComponentModel.DataAnnotations;
using EPS_Web_App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EPS_Web_App.Pages.Account;

public class LoginModel : PageModel
{
	private readonly AdminAuthenticationService
		_authenticationService;


	public LoginModel(
		AdminAuthenticationService authenticationService)
	{
		_authenticationService =
			authenticationService;
	}


	// =========================================================
	// RETURN URL
	// =========================================================

	[BindProperty(SupportsGet = true)]
	public string? ReturnUrl { get; set; }


	// =========================================================
	// LOGIN INPUT
	// =========================================================

	[BindProperty]
	public LoginInput Input { get; set; } =
		new();


	// =========================================================
	// LOGIN
	// =========================================================

	public async Task<IActionResult> OnPostAsync()
	{
		// -----------------------------------------------------
		// VALIDATION
		// -----------------------------------------------------

		if (!ModelState.IsValid)
		{
			return Page();
		}


		// -----------------------------------------------------
		// VALIDATE CREDENTIALS
		// -----------------------------------------------------

		var result =
			await _authenticationService
				.ValidateCredentialsAsync(
					Input.Username.Trim(),
					Input.Password
				);


		if (!result.Success)
		{
			ModelState.AddModelError(
				string.Empty,
				"Invalid username or password."
			);

			return Page();
		}


		// -----------------------------------------------------
		// SIGN IN
		// -----------------------------------------------------

		await _authenticationService.SignInAsync(
			HttpContext,
			result.User!,
			Input.RememberMe
		);


		// -----------------------------------------------------
		// RETURN TO ORIGINAL REQUESTED PAGE
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(ReturnUrl)
			&&
			Url.IsLocalUrl(ReturnUrl))
		{
			return LocalRedirect(ReturnUrl);
		}


		// -----------------------------------------------------
		// DEFAULT
		// -----------------------------------------------------

		return RedirectToPage(
			"/Index"
		);
	}


	// =========================================================
	// LOGIN INPUT MODEL
	// =========================================================

	public sealed class LoginInput
	{
		[Required(
			ErrorMessage =
				"Please enter your username."
		)]
		[StringLength(
			15,
			ErrorMessage =
				"Username cannot exceed 15 characters."
		)]
		public string Username { get; set; } =
			string.Empty;


		[Required(
			ErrorMessage =
				"Please enter your password."
		)]
		public string Password { get; set; } =
			string.Empty;


		public bool RememberMe { get; set; }
	}
}