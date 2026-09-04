using EPS_Web_App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EPS_Web_App.Pages.Account;

public class LogoutModel : PageModel
{
	private readonly AdminAuthenticationService
		_authenticationService;


	public LogoutModel(
		AdminAuthenticationService authenticationService)
	{
		_authenticationService =
			authenticationService;
	}


	public async Task<IActionResult> OnPostAsync()
	{
		await _authenticationService
			.SignOutAsync(
				HttpContext
			);

		return RedirectToPage(
			"/Account/Login"
		);
	}
}