using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Administration.Lookups;

public class CreateModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public CreateModel(ApplicationDbContext context)
	{
		_context = context;
	}


	// =========================================================
	// INPUT
	// =========================================================

	[BindProperty]
	public LookupInput Input { get; set; } =
		new();


	// =========================================================
	// CREATE
	// =========================================================

	public async Task<IActionResult> OnPostAsync()
	{
		Normalize();


		if (!ModelState.IsValid)
		{
			return Page();
		}


		// -----------------------------------------------------
		// DUPLICATE CHECK
		// -----------------------------------------------------

		var exists =
			await _context.LookupValues
				.AsNoTracking()
				.AnyAsync(
					x =>
						x.LookupGroup ==
						Input.LookupGroup
						&&
						x.LookupCode ==
						Input.LookupCode
				);


		if (exists)
		{
			ModelState.AddModelError(
				string.Empty,
				"A lookup value with this Group and Code already exists."
			);


			return Page();
		}


		// -----------------------------------------------------
		// CREATE
		// -----------------------------------------------------

		var lookup =
			new LookupValue
			{
				LookupGroup =
					Input.LookupGroup!,

				LookupCode =
					Input.LookupCode!,

				LookupLabel =
					Input.LookupLabel!,

				DisplayOrder =
					Input.DisplayOrder,

				IsActive =
					Input.IsActive
			};


		_context.LookupValues.Add(
			lookup
		);


		await _context.SaveChangesAsync();


		return RedirectToPage(
			"./Index"
		);
	}


	// =========================================================
	// NORMALIZATION
	// =========================================================

	private void Normalize()
	{
		Input.LookupGroup =
			NormalizeValue(
				Input.LookupGroup
			);

		Input.LookupCode =
			NormalizeValue(
				Input.LookupCode
			);

		Input.LookupLabel =
			NormalizeValue(
				Input.LookupLabel
			);
	}


	private static string? NormalizeValue(
		string? value)
	{
		return
			string.IsNullOrWhiteSpace(value)
				? null
				: value.Trim();
	}


	// =========================================================
	// INPUT MODEL
	// =========================================================

	public sealed class LookupInput
	{
		public string? LookupGroup { get; set; }

		public string? LookupCode { get; set; }

		public string? LookupLabel { get; set; }

		public int? DisplayOrder { get; set; }

		public bool IsActive { get; set; } = true;
	}
}