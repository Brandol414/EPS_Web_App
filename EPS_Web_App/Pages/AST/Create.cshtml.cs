using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.AST;

public class CreateModel : PageModel
{
	private readonly ApplicationDbContext _context;


	public CreateModel(
		ApplicationDbContext context)
	{
		_context = context;
	}


	// =========================================================
	// FORM INPUT
	// =========================================================

	[BindProperty]
	public AstRecord Input { get; set; } = new();


	// =========================================================
	// POST
	// =========================================================

	public async Task<IActionResult> OnPostAsync()
	{
		// -----------------------------------------------------
		// SERVER-MANAGED FIELDS
		//
		// These values are NOT submitted by the browser.
		// Remove them from ModelState before validation.
		// -----------------------------------------------------

		ModelState.Remove(
			"Input.LinkageStatus");

		ModelState.Remove(
			"Input.CreatedAt");

		ModelState.Remove(
			"Input.UpdatedAt");


		// -----------------------------------------------------
		// NORMALIZATION
		// -----------------------------------------------------

		NormalizeIdentity();

		NormalizeDates();


		// -----------------------------------------------------
		// IDENTITY VALIDATION
		// -----------------------------------------------------

		ValidateIdentity();


		// -----------------------------------------------------
		// MODEL VALIDATION
		// -----------------------------------------------------

		if (!ModelState.IsValid)
		{
			return Page();
		}


		// =====================================================
		// QUESTIONNAIRE LINKAGE
		// =====================================================

		var questionnaire =
			await _context.QuestionnaireEntries
				.AsNoTracking()
				.FirstOrDefaultAsync(
					x =>
						x.SpecimenId ==
						Input.SpecimenId
				);


		Input.LinkageStatus =
			questionnaire == null
				? "Unlinked"
				: "Linked";


		// =====================================================
		// DUPLICATE PROTECTION
		// =====================================================

		var possibleDuplicate =
			await _context.AstRecords
				.AsNoTracking()
				.AnyAsync(
					x =>
						x.SpecimenId ==
						Input.SpecimenId
						&&
						x.BacterialIdentification ==
						Input.BacterialIdentification
						&&
						x.DiagnosticTest ==
						Input.DiagnosticTest
						&&
						x.MonthCollected ==
						Input.MonthCollected
				);


		if (possibleDuplicate)
		{
			ModelState.AddModelError(
				string.Empty,
				"A matching AST record already exists for this specimen, organism, diagnostic test, and collection month."
			);

			return Page();
		}


		// =====================================================
		// SYSTEM TIMESTAMPS
		// =====================================================

		Input.CreatedAt =
			DateTime.UtcNow;

		Input.UpdatedAt =
			null;


		// =====================================================
		// SAVE
		// =====================================================

		_context.AstRecords.Add(
			Input
		);


		await _context.SaveChangesAsync();


		// =====================================================
		// RETURN TO AST INDEX
		// =====================================================

		return RedirectToPage(
			"./Index"
		);
	}


	// =========================================================
	// NORMALIZE IDENTITY
	// =========================================================

	private void NormalizeIdentity()
	{
		if (
			!string.IsNullOrWhiteSpace(
				Input.SpecimenId)
		)
		{
			Input.SpecimenId =
				Input.SpecimenId
					.Trim()
					.ToUpperInvariant();
		}


		if (
			!string.IsNullOrWhiteSpace(
				Input.Lan)
		)
		{
			Input.Lan =
				Input.Lan
					.Trim()
					.ToUpperInvariant();
		}


		if (
			!string.IsNullOrWhiteSpace(
				Input.SiteCode)
		)
		{
			Input.SiteCode =
				Input.SiteCode
					.Trim()
					.ToUpperInvariant();
		}


		if (
			!string.IsNullOrWhiteSpace(
				Input.MonthCollected)
		)
		{
			Input.MonthCollected =
				Input.MonthCollected
					.Trim();
		}


		if (
			!string.IsNullOrWhiteSpace(
				Input.BacterialIdentification)
		)
		{
			Input.BacterialIdentification =
				Input.BacterialIdentification
					.Trim();
		}


		if (
			!string.IsNullOrWhiteSpace(
				Input.ViralIdentification)
		)
		{
			Input.ViralIdentification =
				Input.ViralIdentification
					.Trim();
		}


		if (
			!string.IsNullOrWhiteSpace(
				Input.ParasiteIdentification)
		)
		{
			Input.ParasiteIdentification =
				Input.ParasiteIdentification
					.Trim();
		}


		if (
			!string.IsNullOrWhiteSpace(
				Input.DiagnosticTest)
		)
		{
			Input.DiagnosticTest =
				Input.DiagnosticTest
					.Trim();
		}


		if (
			!string.IsNullOrWhiteSpace(
				Input.ParticipantType)
		)
		{
			Input.ParticipantType =
				Input.ParticipantType
					.Trim();
		}
	}


	// =========================================================
	// NORMALIZE AST DATE VALUES
	// =========================================================
	//
	// MonthCollected represents a MONTH, not an exact day.
	//
	// Database representation:
	//
	//     yyyy-MM-01
	//
	// HTML month input:
	//
	//     yyyy-MM
	//
	// Example:
	//
	//     2026-09
	//          ↓
	//     2026-09-01
	//
	// =========================================================

	private void NormalizeDates()
	{
		Input.MonthCollected =
			NormalizeMonthValue(
				Input.MonthCollected
			);
	}


	private static string? NormalizeMonthValue(
		string? value)
	{
		if (
			string.IsNullOrWhiteSpace(
				value)
		)
		{
			return null;
		}


		value =
			value.Trim();


		// -----------------------------------------------------
		// HTML <input type="month">
		// -----------------------------------------------------

		if (
			DateTime.TryParseExact(
				value,
				"yyyy-MM",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var monthValue
			)
		)
		{
			return new DateTime(
					monthValue.Year,
					monthValue.Month,
					1)
				.ToString(
					"yyyy-MM-dd",
					CultureInfo.InvariantCulture
				);
		}


		// -----------------------------------------------------
		// Already-normalized database value
		// -----------------------------------------------------

		if (
			DateTime.TryParseExact(
				value,
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var dateValue
			)
		)
		{
			return new DateTime(
					dateValue.Year,
					dateValue.Month,
					1)
				.ToString(
					"yyyy-MM-dd",
					CultureInfo.InvariantCulture
				);
		}


		// -----------------------------------------------------
		// Common historical formats
		// -----------------------------------------------------

		string[] formats =
		{
			"dd/MM/yyyy",
			"d/M/yyyy",
			"MM/dd/yyyy",
			"M/d/yyyy",
			"dd-MM-yyyy",
			"d-M-yyyy",
			"MM-dd-yyyy",
			"M-d-yyyy",
			"dd MMM yyyy",
			"d MMM yyyy",
			"dd-MMM-yyyy",
			"d-MMM-yyyy",
			"MMM yyyy",
			"MMMM yyyy"
		};


		if (
			DateTime.TryParseExact(
				value,
				formats,
				CultureInfo.InvariantCulture,
				DateTimeStyles.AllowWhiteSpaces,
				out var parsedDate
			)
		)
		{
			return new DateTime(
					parsedDate.Year,
					parsedDate.Month,
					1)
				.ToString(
					"yyyy-MM-dd",
					CultureInfo.InvariantCulture
				);
		}


		// -----------------------------------------------------
		// Excel serial date
		// -----------------------------------------------------

		if (
			double.TryParse(
				value,
				System.Globalization.NumberStyles.Number,
				CultureInfo.InvariantCulture,
				out var serial
			)
			&&
			serial >= 1
			&&
			serial <= 60000
		)
		{
			try
			{
				var excelDate =
					DateTime.FromOADate(
						serial
					);


				return new DateTime(
						excelDate.Year,
						excelDate.Month,
						1)
					.ToString(
						"yyyy-MM-dd",
						CultureInfo.InvariantCulture
					);
			}
			catch
			{
				// Invalid numeric value.
			}
		}


		return value;
	}


	// =========================================================
	// IDENTITY VALIDATION
	// =========================================================

	private void ValidateIdentity()
	{
		if (
			string.IsNullOrWhiteSpace(
				Input.SpecimenId)
		)
		{
			ModelState.AddModelError(
				"Input.SpecimenId",
				"Specimen ID is required."
			);

			return;
		}


		// -----------------------------------------------------
		// SPECIMEN FORMAT
		// -----------------------------------------------------

		if (
			!Regex.IsMatch(
				Input.SpecimenId,
				@"^EPS1549-[A-Z0-9]+-[0-9]{4}[AB]$"
			)
		)
		{
			ModelState.AddModelError(
				"Input.SpecimenId",
				"Specimen ID must follow the EPS1549-SITE-####A/B format."
			);

			return;
		}


		var parts =
			Input.SpecimenId.Split('-');


		if (
			parts.Length != 3
		)
		{
			ModelState.AddModelError(
				"Input.SpecimenId",
				"Specimen ID format is invalid."
			);

			return;
		}


		var derivedSite =
			parts[1];


		// -----------------------------------------------------
		// SITE CODE
		// -----------------------------------------------------

		if (
			string.IsNullOrWhiteSpace(
				Input.SiteCode)
		)
		{
			Input.SiteCode =
				derivedSite;
		}
		else if (
			!string.Equals(
				Input.SiteCode,
				derivedSite,
				StringComparison.OrdinalIgnoreCase
			)
		)
		{
			ModelState.AddModelError(
				"Input.SiteCode",
				$"Site code must match the site embedded in the Specimen ID ({derivedSite})."
			);
		}


		// -----------------------------------------------------
		// LAN
		// -----------------------------------------------------

		ValidateLan();


		// -----------------------------------------------------
		// MONTH COLLECTED
		// -----------------------------------------------------

		if (
			!string.IsNullOrWhiteSpace(
				Input.MonthCollected)
		)
		{
			if (
				!DateTime.TryParseExact(
					Input.MonthCollected,
					"yyyy-MM-dd",
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out var monthDate
				)
				||
				monthDate.Day != 1
			)
			{
				ModelState.AddModelError(
					"Input.MonthCollected",
					"Collection month must be a valid month."
				);
			}
		}
	}


	// =========================================================
	// LAN VALIDATION
	// =========================================================

	private void ValidateLan()
	{
		if (
			string.IsNullOrWhiteSpace(
				Input.Lan)
		)
		{
			return;
		}


		if (
			Regex.IsMatch(
				Input.Lan,
				@"^MHK[0-9]{5}$"
			)
		)
		{
			return;
		}


		var historicalExceptions =
			new HashSet<string>(
				StringComparer.OrdinalIgnoreCase)
			{
				"NOSAMPLE",
				"REJECTED",
				"DISQUALIFIED"
			};


		if (
			historicalExceptions.Contains(
				Input.Lan)
		)
		{
			return;
		}


		ModelState.AddModelError(
			"Input.Lan",
			"LAN must follow MHK##### or be a recognized historical exception."
		);
	}
}