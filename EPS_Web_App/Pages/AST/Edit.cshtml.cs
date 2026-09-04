using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using EPS_Web_App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.AST;

[Authorize(Policy = "AdministratorOnly")]
public class EditModel : PageModel
{
	private readonly ApplicationDbContext _context;

	private readonly ReconciliationService
		_reconciliationService;


	public EditModel(
		ApplicationDbContext context,
		ReconciliationService reconciliationService)
	{
		_context = context;

		_reconciliationService =
			reconciliationService;
	}


	// =========================================================
	// ROUTE
	// =========================================================

	[BindProperty(SupportsGet = true)]
	public long Id { get; set; }


	// =========================================================
	// RECORD
	// =========================================================

	public AstRecord Record { get; private set; } = null!;


	// =========================================================
	// EDIT VALUES
	// =========================================================

	[BindProperty]
	public Dictionary<string, string?> Values { get; set; } =
		new(
			StringComparer.OrdinalIgnoreCase);


	// =========================================================
	// REQUIRED EDIT REASON
	// =========================================================

	[BindProperty]
	public string Reason { get; set; } =
		string.Empty;


	// =========================================================
	// SECTIONS
	// =========================================================

	public List<EditSection> Sections { get; private set; } =
		[];


	// =========================================================
	// CURRENT USER
	// =========================================================

	public string CurrentUserName =>
		User.Identity?.Name
		??
		User.FindFirstValue(ClaimTypes.Email)
		??
		User.FindFirstValue(ClaimTypes.Name)
		??
		"Unknown Administrator";


	// =========================================================
	// GET
	// =========================================================

	public async Task<IActionResult> OnGetAsync(
		long id,
		CancellationToken cancellationToken = default)
	{
		Id = id;


		var record =
			await _context.AstRecords
				.AsNoTracking()
				.FirstOrDefaultAsync(
					x =>
						x.AstRecordId == Id,
					cancellationToken);


		if (record == null)
		{
			return NotFound();
		}


		Record =
			record;


		LoadValues(record);

		BuildSections();


		return Page();
	}


	// =========================================================
	// POST
	// =========================================================

	public async Task<IActionResult> OnPostAsync(
		long id,
		CancellationToken cancellationToken = default)
	{
		Id = id;


		var record =
			await _context.AstRecords
				.FirstOrDefaultAsync(
					x =>
						x.AstRecordId == Id,
					cancellationToken);


		if (record == null)
		{
			return NotFound();
		}


		Record =
			record;


		// -----------------------------------------------------
		// REQUIRED REASON
		// -----------------------------------------------------

		if (
			string.IsNullOrWhiteSpace(
				Reason)
		)
		{
			ModelState.AddModelError(
				nameof(Reason),
				"A reason for the correction is required.");
		}


		// -----------------------------------------------------
		// ORIGINAL SPECIMEN ID
		// -----------------------------------------------------

		var oldSpecimenId =
			NormalizeSpecimenId(
				record.SpecimenId);


		// -----------------------------------------------------
		// SPECIMEN ID VALIDATION
		// -----------------------------------------------------

		Values.TryGetValue(
			nameof(AstRecord.SpecimenId),
			out var submittedSpecimenId);


		submittedSpecimenId =
			NormalizeSpecimenId(
				submittedSpecimenId);


		if (
			string.IsNullOrWhiteSpace(
				submittedSpecimenId)
		)
		{
			ModelState.AddModelError(
				"Values[SpecimenId]",
				"Specimen ID is required.");
		}
		else if (
			!IsValidSpecimenId(
				submittedSpecimenId)
		)
		{
			ModelState.AddModelError(
				"Values[SpecimenId]",
				"Specimen ID must follow the EPS1549-SITE-####A/B format.");
		}
		else
		{
			var duplicate =
				await _context.AstRecords
					.AsNoTracking()
					.AnyAsync(
						x =>
							x.AstRecordId != Id
							&&
							x.SpecimenId ==
								submittedSpecimenId,
						cancellationToken);


			if (duplicate)
			{
				ModelState.AddModelError(
					"Values[SpecimenId]",
					"That Specimen ID already exists in the AST database.");
			}
		}


		// -----------------------------------------------------
		// SITE CODE CONSISTENCY
		// -----------------------------------------------------

		Values.TryGetValue(
			nameof(AstRecord.SiteCode),
			out var submittedSiteCode);


		submittedSiteCode =
			NormalizeSiteCode(
				submittedSiteCode);


		if (
			!string.IsNullOrWhiteSpace(
				submittedSpecimenId)
			&&
			IsValidSpecimenId(
				submittedSpecimenId)
		)
		{
			var parts =
				submittedSpecimenId.Split('-');


			if (
				parts.Length == 3
			)
			{
				var derivedSite =
					parts[1];


				if (
					!string.IsNullOrWhiteSpace(
						submittedSiteCode)
					&&
					!string.Equals(
						submittedSiteCode,
						derivedSite,
						StringComparison.OrdinalIgnoreCase)
				)
				{
					ModelState.AddModelError(
						"Values[SiteCode]",
						$"Site code must match the site embedded in the Specimen ID ({derivedSite}).");
				}
			}
		}


		// -----------------------------------------------------
		// LAN NORMALIZATION
		// -----------------------------------------------------

		Values.TryGetValue(
			nameof(AstRecord.Lan),
			out var submittedLan);


		submittedLan =
			NormalizeLan(
				submittedLan);


		// -----------------------------------------------------
		// MONTH VALIDATION
		// -----------------------------------------------------

		Values.TryGetValue(
			nameof(AstRecord.MonthCollected),
			out var submittedMonth);


		submittedMonth =
			NormalizeMonthValue(
				submittedMonth);


		if (
			!string.IsNullOrWhiteSpace(
				submittedMonth)
			&&
			!IsValidCanonicalMonth(
				submittedMonth)
		)
		{
			ModelState.AddModelError(
				"Values[MonthCollected]",
				"Collection month must be a valid month.");
		}


		// -----------------------------------------------------
		// STOP ON VALIDATION ERRORS
		// -----------------------------------------------------

		if (
			!ModelState.IsValid
		)
		{
			Values[
				nameof(AstRecord.SpecimenId)] =
				submittedSpecimenId;

			Values[
				nameof(AstRecord.SiteCode)] =
				submittedSiteCode;

			Values[
				nameof(AstRecord.Lan)] =
				submittedLan;

			Values[
				nameof(AstRecord.MonthCollected)] =
				submittedMonth;

			BuildSections();

			return Page();
		}


		// -----------------------------------------------------
		// AUDIT USER / TIMESTAMP
		// -----------------------------------------------------

		var username =
			User.Identity?.Name
			??
			User.FindFirstValue(
				ClaimTypes.Email)
			??
			User.FindFirstValue(
				ClaimTypes.Name)
			??
			"Unknown Administrator";


		var changedAt =
			DateTime.UtcNow;


		var reason =
			Reason.Trim();


		// -----------------------------------------------------
		// PROTECTED FIELDS
		// -----------------------------------------------------

		var protectedFields =
			new HashSet<string>(
				StringComparer.OrdinalIgnoreCase)
			{
				nameof(AstRecord.AstRecordId),
				nameof(AstRecord.CreatedAt),
				nameof(AstRecord.UpdatedAt),
				nameof(AstRecord.LinkageStatus)
			};


		// -----------------------------------------------------
		// EDITABLE STRING PROPERTIES
		// -----------------------------------------------------

		var editableProperties =
			typeof(AstRecord)
				.GetProperties(
					BindingFlags.Public |
					BindingFlags.Instance)
				.Where(
					x =>
						x.PropertyType ==
							typeof(string)
						&&
						x.CanRead
						&&
						x.CanWrite
						&&
						!protectedFields.Contains(
							x.Name))
				.ToList();


		var auditEntries =
			new List<DataEntryAudit>();


		// -----------------------------------------------------
		// APPLY VALUES + BUILD AUDIT
		// -----------------------------------------------------

		foreach (
			var property
			in editableProperties)
		{
			var propertyName =
				property.Name;


			var oldValue =
				property.GetValue(
					record)
				as string;


			Values.TryGetValue(
				propertyName,
				out var newValue);


			newValue =
				NormalizeValue(
					newValue);


			// -------------------------------------------------
			// AST MONTH NORMALIZATION
			// -------------------------------------------------

			if (
				propertyName.Equals(
					nameof(AstRecord.MonthCollected),
					StringComparison.OrdinalIgnoreCase))
			{
				newValue =
					NormalizeMonthValue(
						newValue);
			}


			// -------------------------------------------------
			// IDENTIFIER NORMALIZATION
			// -------------------------------------------------

			if (
				propertyName ==
				nameof(AstRecord.SpecimenId)
			)
			{
				newValue =
					NormalizeSpecimenId(
						newValue);
			}


			if (
				propertyName ==
				nameof(AstRecord.SiteCode)
			)
			{
				newValue =
					NormalizeSiteCode(
						newValue);
			}


			if (
				propertyName ==
				nameof(AstRecord.Lan)
			)
			{
				newValue =
					NormalizeLan(
						newValue);
			}


			// -------------------------------------------------
			// NO CHANGE
			// -------------------------------------------------

			if (
				string.Equals(
					oldValue,
					newValue,
					StringComparison.Ordinal)
			)
			{
				continue;
			}


			property.SetValue(
				record,
				newValue);


			auditEntries.Add(
				new DataEntryAudit
				{
					RecordType =
						"AST",

					RecordKey =
						record.AstRecordId
							.ToString(),

					FieldName =
						propertyName,

					OldValue =
						oldValue,

					NewValue =
						newValue,

					ChangedBy =
						username,

					ChangedAt =
						changedAt,

					Reason =
						reason
				});
		}


		// -----------------------------------------------------
		// NOTHING ACTUALLY CHANGED
		// -----------------------------------------------------

		if (
			auditEntries.Count == 0
		)
		{
			ModelState.AddModelError(
				string.Empty,
				"No changes were detected.");

			BuildSections();

			return Page();
		}


		// -----------------------------------------------------
		// NEW SPECIMEN ID
		// -----------------------------------------------------

		var newSpecimenId =
			NormalizeSpecimenId(
				record.SpecimenId);


		var specimenIdChanged =
			!string.Equals(
				oldSpecimenId,
				newSpecimenId,
				StringComparison.OrdinalIgnoreCase);


		// -----------------------------------------------------
		// UPDATE TIMESTAMP
		// -----------------------------------------------------

		record.UpdatedAt =
			changedAt;


		// =====================================================
		// TRANSACTION
		// =====================================================

		await using var transaction =
			await _context.Database
				.BeginTransactionAsync(
					cancellationToken);


		try
		{
			// -------------------------------------------------
			// SAVE AST + AUDIT
			// -------------------------------------------------

			_context.DataEntryAudits.AddRange(
				auditEntries);


			await _context.SaveChangesAsync(
				cancellationToken);


			// -------------------------------------------------
			// RECONCILIATION
			// -------------------------------------------------

			if (
				specimenIdChanged
				&&
				!string.IsNullOrWhiteSpace(
					oldSpecimenId)
				&&
				!string.IsNullOrWhiteSpace(
					newSpecimenId)
			)
			{
				await _reconciliationService
					.ReconcileSpecimenIdChangeAsync(
						oldSpecimenId,
						newSpecimenId,
						username,
						reason,
						cancellationToken);
			}
			else
			{
				await _reconciliationService
					.RunForSpecimenAsync(
						newSpecimenId
						??
						string.Empty,
						cancellationToken);
			}


			await transaction.CommitAsync(
				cancellationToken);
		}
		catch
		{
			await transaction.RollbackAsync(
				cancellationToken);

			throw;
		}


		// -----------------------------------------------------
		// RETURN TO AST VIEW
		// -----------------------------------------------------

		return RedirectToPage(
			"./View",
			new
			{
				id =
					record.AstRecordId
			});
	}


	// =========================================================
	// LOAD VALUES
	// =========================================================

	private void LoadValues(
		AstRecord record)
	{
		Values =
			new Dictionary<string, string?>(
				StringComparer.OrdinalIgnoreCase);


		var protectedFields =
			new HashSet<string>(
				StringComparer.OrdinalIgnoreCase)
			{
				nameof(AstRecord.AstRecordId),
				nameof(AstRecord.CreatedAt),
				nameof(AstRecord.UpdatedAt),
				nameof(AstRecord.LinkageStatus)
			};


		var properties =
			typeof(AstRecord)
				.GetProperties(
					BindingFlags.Public |
					BindingFlags.Instance)
				.Where(
					x =>
						x.PropertyType ==
							typeof(string)
						&&
						x.CanRead
						&&
						!protectedFields.Contains(
							x.Name));


		foreach (
			var property
			in properties)
		{
			Values[property.Name] =
				property.GetValue(
					record)
				as string;
		}
	}


	// =========================================================
	// BUILD SECTIONS
	// =========================================================

	private void BuildSections()
	{
		Sections =
			new List<EditSection>
			{
				new(
					"01",
					"Record Identity",
					new[]
					{
						nameof(
							AstRecord.MonthCollected),

						nameof(
							AstRecord.SpecimenId),

						nameof(
							AstRecord.Lan),

						nameof(
							AstRecord.SiteCode)
					}),

				new(
					"02",
					"Participant / Clinical Context",
					new[]
					{
						nameof(
							AstRecord.ParticipantType),

						nameof(
							AstRecord.GtdStatus),

						nameof(
							AstRecord.DiarrheaMedicationPast72h),

						nameof(
							AstRecord.MedicationsPast72h),

						nameof(
							AstRecord.IllnessFunctionalImpact)
					}),

				new(
					"03",
					"Laboratory Identification",
					new[]
					{
						nameof(
							AstRecord.BacterialIdentification),

						nameof(
							AstRecord.ViralIdentification),

						nameof(
							AstRecord.ParasiteIdentification),

						nameof(
							AstRecord.DiagnosticTest),

						nameof(
							AstRecord.MdrStatus),

						nameof(
							AstRecord.EsblStatus)
					}),

				new(
					"04",
					"Antimicrobial Susceptibility",
					GetAntimicrobialProperties())
			};
	}


	// =========================================================
	// ANTIBIOTIC FIELDS
	// =========================================================

	private static IEnumerable<string>
		GetAntimicrobialProperties()
	{
		return typeof(AstRecord)
			.GetProperties(
				BindingFlags.Public |
				BindingFlags.Instance)
			.Where(
				x =>
					x.PropertyType ==
						typeof(string)
					&&
					(
						x.Name.EndsWith(
							"Mic",
							StringComparison.OrdinalIgnoreCase)
						||
						x.Name.EndsWith(
							"Int",
							StringComparison.OrdinalIgnoreCase)
					))
			.Select(
				x =>
					x.Name)
			.OrderBy(
				x =>
					x,
				StringComparer.OrdinalIgnoreCase);
	}


	// =========================================================
	// SPECIMEN ID VALIDATION
	// =========================================================

	private static bool IsValidSpecimenId(
		string? value)
	{
		if (
			string.IsNullOrWhiteSpace(
				value)
		)
		{
			return false;
		}


		return
			System.Text.RegularExpressions.Regex
				.IsMatch(
					value,
					@"^EPS1549-[A-Z0-9]+-[0-9]{4}[AB]$",
					System.Text.RegularExpressions.RegexOptions.IgnoreCase);
	}


	// =========================================================
	// AST MONTH FORMATTING
	// =========================================================

	public string? FormatMonthForInput(
		string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}


		value =
			value.Trim();


		// -----------------------------------------------------
		// Canonical database value:
		//
		// yyyy-MM-dd
		//
		// Example:
		// 2026-09-01
		// becomes:
		// 2026-09
		// -----------------------------------------------------

		if (
			DateTime.TryParseExact(
				value,
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var date)
		)
		{
			return date.ToString(
				"yyyy-MM",
				CultureInfo.InvariantCulture);
		}


		// -----------------------------------------------------
		// Already in HTML month-input format:
		//
		// yyyy-MM
		// -----------------------------------------------------

		if (
			DateTime.TryParseExact(
				value,
				"yyyy-MM",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var month)
		)
		{
			return month.ToString(
				"yyyy-MM",
				CultureInfo.InvariantCulture);
		}


		// -----------------------------------------------------
		// Unknown/historical value
		// -----------------------------------------------------

		return value;
	}


	// =========================================================
	// AST MONTH NORMALIZATION
	// =========================================================

	private static string? NormalizeMonthValue(
		string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}


		value =
			value.Trim();


		if (
			DateTime.TryParseExact(
				value,
				"yyyy-MM",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var month)
		)
		{
			return new DateTime(
					month.Year,
					month.Month,
					1)
				.ToString(
					"yyyy-MM-dd",
					CultureInfo.InvariantCulture);
		}


		if (
			DateTime.TryParseExact(
				value,
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var date)
		)
		{
			return new DateTime(
					date.Year,
					date.Month,
					1)
				.ToString(
					"yyyy-MM-dd",
					CultureInfo.InvariantCulture);
		}


		return value;
	}


	private static bool IsValidCanonicalMonth(
		string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return false;
		}


		if (
			!DateTime.TryParseExact(
				value,
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var date)
		)
		{
			return false;
		}


		return date.Day == 1;
	}


	// =========================================================
	// NORMALIZATION
	// =========================================================

	private static string?
		NormalizeValue(
			string? value)
	{
		return
			string.IsNullOrWhiteSpace(
				value)
				?
				null
				:
				value.Trim();
	}


	private static string?
		NormalizeSpecimenId(
			string? value)
	{
		return
			string.IsNullOrWhiteSpace(
				value)
				?
				null
				:
				value
					.Trim()
					.ToUpperInvariant();
	}


	private static string?
		NormalizeSiteCode(
			string? value)
	{
		return
			string.IsNullOrWhiteSpace(
				value)
				?
				null
				:
				value
					.Trim()
					.ToUpperInvariant();
	}


	private static string?
		NormalizeLan(
			string? value)
	{
		return
			string.IsNullOrWhiteSpace(
				value)
				?
				null
				:
				value
					.Trim()
					.ToUpperInvariant();
	}


	// =========================================================
	// DISPLAY LABEL
	// =========================================================

	private static string Humanize(
		string propertyName)
	{
		if (
			string.IsNullOrWhiteSpace(
				propertyName)
		)
		{
			return string.Empty;
		}


		var chars =
			new List<char>();


		for (
			var i = 0;
			i < propertyName.Length;
			i++)
		{
			var character =
				propertyName[i];


			if (
				i > 0
				&&
				char.IsUpper(
					character)
				&&
				!char.IsUpper(
					propertyName[i - 1])
			)
			{
				chars.Add(' ');
			}


			chars.Add(
				character);
		}


		return new string(
			chars.ToArray())
			.Replace(
				"Lan",
				"LAN")
			.Replace(
				"Mic",
				"MIC")
			.Replace(
				"Int",
				"Interpretation")
			.Replace(
				"Mdr",
				"MDR")
			.Replace(
				"Esbl",
				"ESBL")
			.Replace(
				"Gtd",
				"GTD");
	}


	// =========================================================
	// VIEW MODELS
	// =========================================================

	public sealed class EditSection
	{
		public EditSection(
			string number,
			string title,
			IEnumerable<string> fieldNames)
		{
			Number =
				number;

			Title =
				title;

			Fields =
				fieldNames
					.Select(
						x =>
							new EditField
							{
								Name =
									x,

								Label =
									Humanize(x)
							})
					.ToList();
		}


		public string Number { get; }

		public string Title { get; }

		public List<EditField> Fields { get; }
	}


	public sealed class EditField
	{
		public string Name { get; set; } =
			string.Empty;

		public string Label { get; set; } =
			string.Empty;
	}
}