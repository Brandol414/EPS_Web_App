using System.ComponentModel.DataAnnotations;
using EPS_Web_App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EPS_Web_App.Pages.Account;

public class SetupModel : PageModel
{
	private readonly AdminAuthenticationService
		_authenticationService;


	public SetupModel(
		AdminAuthenticationService authenticationService)
	{
		_authenticationService =
			authenticationService;
	}


	[BindProperty]
	public SetupInput Input { get; set; } =
		new();


	public async Task<IActionResult> OnGetAsync()
	{
		// -----------------------------------------------------
		// Setup is only allowed when there are no admin users.
		// -----------------------------------------------------

		return Page();
	}


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
					administrator: true
				);


		if (!result.Success)
		{
			ModelState.AddModelError(
				string.Empty,
				result.Error!
			);

			return Page();
		}


		return RedirectToPage(
			"/Account/Login"
		);
	}


	public sealed class SetupInput
	{
		[Required(
			ErrorMessage =
				"Display name is required."
		)]
		[StringLength(100)]
		public string DisplayName { get; set; } =
			string.Empty;


		[Required(
			ErrorMessage =
				"Username is required."
		)]
		[StringLength(100)]
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
	}
}