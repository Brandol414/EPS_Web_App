using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Administration.Lookups;

public class EditModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public EditModel(ApplicationDbContext context)
	{
		_context = context;
	}


	// =========================================================
	// ROUTE ID
	// =========================================================

	[BindProperty(SupportsGet = true)]
	public long Id { get; set; }


	// =========================================================
	// INPUT
	// =========================================================

	[BindProperty]
	public LookupInput Input { get; set; } =
		new();


	// =========================================================
	// GET
	// =========================================================

	public async Task<IActionResult> OnGetAsync()
	{
		var lookup =
			await _context.LookupValues
				.AsNoTracking()
				.FirstOrDefaultAsync(
					x =>
						x.LookupId ==
						Id
				);


		if (lookup == null)
		{
			return NotFound();
		}


		Input =
			new LookupInput
			{
				LookupGroup =
					lookup.LookupGroup,

				LookupCode =
					lookup.LookupCode,

				LookupLabel =
					lookup.LookupLabel,

				DisplayOrder =
					lookup.DisplayOrder,

				IsActive =
					lookup.IsActive
			};


		return Page();
	}


	// =========================================================
	// UPDATE
	// =========================================================

	public async Task<IActionResult> OnPostAsync()
	{
		Normalize();


		if (!ModelState.IsValid)
		{
			return Page();
		}


		var lookup =
			await _context.LookupValues
				.FirstOrDefaultAsync(
					x =>
						x.LookupId ==
						Id
				);


		if (lookup == null)
		{
			return NotFound();
		}


		// -----------------------------------------------------
		// DUPLICATE CHECK
		//
		// Ignore the current row when checking uniqueness.
		// -----------------------------------------------------

		var duplicateExists =
			await _context.LookupValues
				.AsNoTracking()
				.AnyAsync(
					x =>
						x.LookupId != Id
						&&
						x.LookupGroup ==
						Input.LookupGroup
						&&
						x.LookupCode ==
						Input.LookupCode
				);


		if (duplicateExists)
		{
			ModelState.AddModelError(
				string.Empty,
				"Another lookup value with this Group and Code already exists."
			);


			return Page();
		}


		// -----------------------------------------------------
		// UPDATE
		// -----------------------------------------------------

		lookup.LookupGroup =
			Input.LookupGroup!;

		lookup.LookupCode =
			Input.LookupCode!;

		lookup.LookupLabel =
			Input.LookupLabel!;

		lookup.DisplayOrder =
			Input.DisplayOrder;

		lookup.IsActive =
			Input.IsActive;


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