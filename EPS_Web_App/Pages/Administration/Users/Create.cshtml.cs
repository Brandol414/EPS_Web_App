using System.ComponentModel.DataAnnotations;
using EPS_Web_App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EPS_Web_App.Pages.Administration.Users;

public class CreateModel : PageModel
{
	private readonly AdminAuthenticationService
		_authenticationService;


	public CreateModel(
		AdminAuthenticationService authenticationService)
	{
		_authenticationService =
			authenticationService;
	}


	// =========================================================
	// INPUT
	// =========================================================

	[BindProperty]
	public UserInput Input { get; set; } =
		new();


	// =========================================================
	// CREATE USER
	// =========================================================

	public async Task<IActionResult> OnPostAsync()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}


		var result =
			await _authenticationService
				.CreateUserAsync(
					Input.Username,
					Input.DisplayName,
					Input.Password,
					Input.IsAdministrator
				);


		if (!result.Success)
		{
			ModelState.AddModelError(
				string.Empty,
				result.Error ??
				"Unable to create the administrator account."
			);

			return Page();
		}


		return RedirectToPage(
			"./Index"
		);
	}


	// =========================================================
	// INPUT MODEL
	// =========================================================

	public sealed class UserInput
	{
		[Required(
			ErrorMessage =
				"Display name is required."
		)]
		[StringLength(
			150,
			ErrorMessage =
				"Display name cannot exceed 150 characters."
		)]
		public string DisplayName { get; set; } =
			string.Empty;


		[Required(
			ErrorMessage =
				"Username is required."
		)]
		[StringLength(
			100,
			ErrorMessage =
				"Username cannot exceed 100 characters."
		)]
		public string Username { get; set; } =
			string.Empty;


		[Required(
			ErrorMessage =
				"Password is required."
		)]
		[DataType(DataType.Password)]
		public string Password { get; set; } =
			string.Empty;


		[Required(
			ErrorMessage =
				"Please confirm the password."
		)]
		[Compare(
			nameof(Password),
			ErrorMessage =
				"The passwords do not match."
		)]
		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; } =
			string.Empty;


		public bool IsAdministrator { get; set; } =
			true;
	}
}