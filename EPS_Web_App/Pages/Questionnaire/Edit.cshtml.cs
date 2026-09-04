using System.Globalization;
using System.Reflection;
using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using EPS_Web_App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Questionnaire;

[Authorize(Policy = "AdministratorOnly")]
public class EditModel : PageModel
{
	private readonly ApplicationDbContext _context;

	private readonly ReconciliationService
		_reconciliationService;


	// =========================================================
	// DATE / TIME FIELD DEFINITIONS
	// =========================================================

	private static readonly HashSet<string> DateFields =
		new(
			StringComparer.OrdinalIgnoreCase)
	{
		nameof(QuestionnaireEntry.IsolationDate),
		nameof(QuestionnaireEntry.DateOfBirth),
		nameof(QuestionnaireEntry.RotavirusDose1Date),
		nameof(QuestionnaireEntry.RotavirusDose2Date),
		nameof(QuestionnaireEntry.OtherRotavirusVaccineDate),
		nameof(QuestionnaireEntry.SymptomOnsetDate),
		nameof(QuestionnaireEntry.AdmissionDate),
		nameof(QuestionnaireEntry.DispositionDate)
	};


	private static readonly HashSet<string> TimeFields =
		new(
			StringComparer.OrdinalIgnoreCase)
	{
		nameof(QuestionnaireEntry.SymptomOnsetTime),
		nameof(QuestionnaireEntry.AdmissionTime),
		nameof(QuestionnaireEntry.DispositionTime)
	};


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
	// RECORD INFORMATION
	// =========================================================

	public string SpecimenId { get; private set; } =
		string.Empty;


	public string SiteCode { get; private set; } =
		string.Empty;


	public string Lan { get; private set; } =
		string.Empty;


	// =========================================================
	// EDIT VALUES
	// =========================================================

	[BindProperty]
	public Dictionary<string, string?> Values { get; set; } =
		new(
			StringComparer.OrdinalIgnoreCase
		);


	// =========================================================
	// REASON
	// =========================================================

	[BindProperty]
	public string Reason { get; set; } =
		string.Empty;


	// =========================================================
	// EDIT SECTIONS
	// =========================================================

	public List<EditSection> Sections { get; private set; } =
		[];


	// =========================================================
	// GET
	// =========================================================

	public async Task<IActionResult> OnGetAsync()
	{
		var record =
			await _context.QuestionnaireEntries
				.AsNoTracking()
				.FirstOrDefaultAsync(
					x =>
						x.QuestionnaireId == Id
				);


		if (record == null)
		{
			return NotFound();
		}


		LoadRecord(record);

		BuildSections();

		return Page();
	}


	// =========================================================
	// POST
	// =========================================================

	public async Task<IActionResult> OnPostAsync()
	{
		var record =
			await _context.QuestionnaireEntries
				.FirstOrDefaultAsync(
					x =>
						x.QuestionnaireId == Id
				);


		if (record == null)
		{
			return NotFound();
		}


		// -----------------------------------------------------
		// CAPTURE THE ORIGINAL ID BEFORE ANY CHANGE.
		// -----------------------------------------------------

		var oldSpecimenId =
			NormalizeSpecimenId(
				record.SpecimenId
			);


		// -----------------------------------------------------
		// REQUIRED AUDIT REASON
		// -----------------------------------------------------

		if (
			string.IsNullOrWhiteSpace(
				Reason
			)
		)
		{
			ModelState.AddModelError(
				nameof(Reason),
				"A reason for the correction is required."
			);
		}


		// -----------------------------------------------------
		// SPECIMEN ID VALIDATION
		// -----------------------------------------------------

		Values.TryGetValue(
			nameof(
				QuestionnaireEntry.SpecimenId
			),
			out var submittedSpecimenId
		);


		submittedSpecimenId =
			NormalizeSpecimenId(
				submittedSpecimenId
			);


		if (
			string.IsNullOrWhiteSpace(
				submittedSpecimenId
			)
		)
		{
			ModelState.AddModelError(
				"Values[SpecimenId]",
				"Specimen ID is required."
			);
		}
		else if (
			!IsValidSpecimenId(
				submittedSpecimenId
			)
		)
		{
			ModelState.AddModelError(
				"Values[SpecimenId]",
				"Specimen ID must follow the EPS1549-SITE-####A/B format."
			);
		}
		else
		{
			var duplicate =
				await _context.QuestionnaireEntries
					.AsNoTracking()
					.AnyAsync(
						x =>
							x.QuestionnaireId != Id
							&&
							x.SpecimenId ==
							submittedSpecimenId
					);


			if (duplicate)
			{
				ModelState.AddModelError(
					"Values[SpecimenId]",
					"That Specimen ID already exists in the questionnaire database."
				);
			}
		}


		// -----------------------------------------------------
		// SITE CODE CONSISTENCY
		// -----------------------------------------------------

		Values.TryGetValue(
			nameof(
				QuestionnaireEntry.SiteCode
			),
			out var submittedSiteCode
		);


		submittedSiteCode =
			NormalizeSiteCode(
				submittedSiteCode
			);


		if (
			!string.IsNullOrWhiteSpace(
				submittedSpecimenId
			)
			&&
			IsValidSpecimenId(
				submittedSpecimenId
			)
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
						submittedSiteCode
					)
					&&
					!string.Equals(
						submittedSiteCode,
						derivedSite,
						StringComparison.OrdinalIgnoreCase
					)
				)
				{
					ModelState.AddModelError(
						"Values[SiteCode]",
						$"Site code must match the site embedded in the Specimen ID ({derivedSite})."
					);
				}
			}
		}


		// -----------------------------------------------------
		// STOP IF VALIDATION FAILED
		// -----------------------------------------------------

		if (
			!ModelState.IsValid
		)
		{
			LoadRecord(record);

			ApplySubmittedValuesToDisplay();

			BuildSections();

			return Page();
		}


		// -----------------------------------------------------
		// AUDIT USER
		// -----------------------------------------------------

		var username =
			User.Identity?.Name
			??
			"Unknown Administrator";


		var changedAt =
			DateTime.UtcNow;


		var auditEntries =
			new List<DataEntryAudit>();


		// -----------------------------------------------------
		// EDITABLE STRING PROPERTIES
		// -----------------------------------------------------

		var editableProperties =
			typeof(QuestionnaireEntry)
				.GetProperties(
					BindingFlags.Public |
					BindingFlags.Instance
				)
				.Where(
					x =>
						x.PropertyType ==
						typeof(string)
				)
				.Where(
					x =>
						!ProtectedFields.Contains(
							x.Name
						)
				)
				.ToList();


		foreach (
			var property
			in editableProperties)
		{
			var propertyName =
				property.Name;


			var oldValue =
				property.GetValue(
					record
				) as string;


			Values.TryGetValue(
				propertyName,
				out var newValue
			);


			newValue =
				NormalizeValue(
					newValue
				);


			// -------------------------------------------------
			// DATE NORMALIZATION
			// -------------------------------------------------

			if (
				DateFields.Contains(
					propertyName
				)
			)
			{
				newValue =
					NormalizeDateValue(
						newValue
					);
			}
			else if (
				TimeFields.Contains(
					propertyName
				)
			)
			{
				newValue =
					NormalizeTimeValue(
						newValue
					);
			}


			// -------------------------------------------------
			// IDENTIFIER NORMALIZATION
			// -------------------------------------------------

			if (
				propertyName ==
				nameof(
					QuestionnaireEntry.SpecimenId
				)
			)
			{
				newValue =
					NormalizeSpecimenId(
						newValue
					);
			}


			if (
				propertyName ==
				nameof(
					QuestionnaireEntry.SiteCode
				)
			)
			{
				newValue =
					NormalizeSiteCode(
						newValue
					);
			}


			if (
				propertyName ==
				nameof(
					QuestionnaireEntry.Lan
				)
			)
			{
				newValue =
					NormalizeLan(
						newValue
					);
			}


			// -------------------------------------------------
			// NO CHANGE
			// -------------------------------------------------

			if (
				string.Equals(
					oldValue,
					newValue,
					StringComparison.Ordinal
				)
			)
			{
				continue;
			}


			property.SetValue(
				record,
				newValue
			);


			auditEntries.Add(
				new DataEntryAudit
				{
					RecordType =
						"Questionnaire",

					RecordKey =
						record.QuestionnaireId
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
						Reason.Trim()
				}
			);
		}


		// -----------------------------------------------------
		// UPDATE TIMESTAMP
		// -----------------------------------------------------

		record.UpdatedAt =
			changedAt;


		// -----------------------------------------------------
		// DETERMINE WHETHER SPECIMEN ID CHANGED
		// -----------------------------------------------------

		var newSpecimenId =
			NormalizeSpecimenId(
				record.SpecimenId
			);


		var specimenIdChanged =
			!string.Equals(
				oldSpecimenId,
				newSpecimenId,
				StringComparison.OrdinalIgnoreCase
			);


		// -----------------------------------------------------
		// TRANSACTION
		//
		// Questionnaire correction, audit entries and
		// reconciliation consequences should succeed/fail
		// together.
		// -----------------------------------------------------

		await using var transaction =
			await _context.Database
				.BeginTransactionAsync();


		try
		{
			// -------------------------------------------------
			// SAVE QUESTIONNAIRE + AUDIT
			// -------------------------------------------------

			if (
				auditEntries.Count > 0
			)
			{
				_context.DataEntryAudits.AddRange(
					auditEntries
				);
			}


			await _context.SaveChangesAsync();


			// -------------------------------------------------
			// RECONCILIATION CONSEQUENCE
			//
			// Only run the targeted identity reconciliation
			// when Specimen ID actually changed.
			// -------------------------------------------------

			if (
				specimenIdChanged
				&&
				!string.IsNullOrWhiteSpace(
					oldSpecimenId
				)
				&&
				!string.IsNullOrWhiteSpace(
					newSpecimenId
				)
			)
			{
				await _reconciliationService
					.ReconcileSpecimenIdChangeAsync(
						oldSpecimenId,
						newSpecimenId,
						username
					);
			}


			await transaction.CommitAsync();
		}
		catch
		{
			await transaction.RollbackAsync();

			throw;
		}


		// -----------------------------------------------------
		// RETURN TO RECORD
		// -----------------------------------------------------

		return RedirectToPage(
			"./View",
			new
			{
				id =
					record.QuestionnaireId
			}
		);
	}


	// =========================================================
	// LOAD RECORD
	// =========================================================

	private void LoadRecord(
		QuestionnaireEntry record)
	{
		SpecimenId =
			record.SpecimenId
			??
			string.Empty;


		SiteCode =
			record.SiteCode
			??
			string.Empty;


		Lan =
			record.Lan
			??
			string.Empty;


		Values =
			new Dictionary<string, string?>(
				StringComparer.OrdinalIgnoreCase
			);


		var properties =
			typeof(QuestionnaireEntry)
				.GetProperties(
					BindingFlags.Public |
					BindingFlags.Instance
				)
				.Where(
					x =>
						x.PropertyType ==
						typeof(string)
				);


		foreach (
			var property
			in properties)
		{
			Values[property.Name] =
				property.GetValue(
					record
				) as string;
		}
	}


	// =========================================================
	// PRESERVE SUBMITTED VALUES
	// =========================================================

	private void ApplySubmittedValuesToDisplay()
	{
		if (
			Values.TryGetValue(
				nameof(
					QuestionnaireEntry.SpecimenId
				),
				out var specimenId
			)
		)
		{
			SpecimenId =
				specimenId
				??
				string.Empty;
		}


		if (
			Values.TryGetValue(
				nameof(
					QuestionnaireEntry.SiteCode
				),
				out var siteCode
			)
		)
		{
			SiteCode =
				siteCode
				??
				string.Empty;
		}


		if (
			Values.TryGetValue(
				nameof(
					QuestionnaireEntry.Lan
				),
				out var lan
			)
		)
		{
			Lan =
				lan
				??
				string.Empty;
		}
	}


	// =========================================================
	// DATE / TIME INPUT FORMATTING
	// =========================================================

	public string GetInputType(
		string fieldName)
	{
		if (
			DateFields.Contains(
				fieldName
			)
		)
		{
			return "date";
		}


		if (
			TimeFields.Contains(
				fieldName
			)
		)
		{
			return "time";
		}


		return "text";
	}


	public string? FormatValueForInput(
		string fieldName,
		string? value)
	{
		if (
			string.IsNullOrWhiteSpace(
				value
			)
		)
		{
			return null;
		}


		value =
			value.Trim();


		if (
			DateFields.Contains(
				fieldName
			)
		)
		{
			return NormalizeDateValue(
				value
			);
		}


		if (
			TimeFields.Contains(
				fieldName
			)
		)
		{
			return NormalizeTimeValue(
				value
			);
		}


		return value;
	}


	private static string? NormalizeDateValue(
		string? value)
	{
		if (
			string.IsNullOrWhiteSpace(
				value
			)
		)
		{
			return null;
		}


		value =
			value.Trim();


		// -----------------------------------------------------
		// Canonical ISO format
		// -----------------------------------------------------

		if (
			DateTime.TryParseExact(
				value,
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var isoDate
			)
		)
		{
			return isoDate.ToString(
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
			"yyyy/MM/dd"
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
			return parsedDate.ToString(
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture
			);
		}


		return value;
	}


	private static string? NormalizeTimeValue(
		string? value)
	{
		if (
			string.IsNullOrWhiteSpace(
				value
			)
		)
		{
			return null;
		}


		value =
			value.Trim();


		if (
			TimeSpan.TryParseExact(
				value,
				new[]
				{
					@"hh\:mm",
					@"h\:mm"
				},
				CultureInfo.InvariantCulture,
				out var time
			)
		)
		{
			return time.ToString(
				@"hh\:mm",
				CultureInfo.InvariantCulture
			);
		}


		return value;
	}


	// =========================================================
	// BUILD FORM SECTIONS
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
						"SiteCode",
						"SpecimenId",
						"Lan",
						"IsolationDate"
					}
				),

				new(
					"02",
					"Participant Information",
					new[]
					{
						"Age",
						"DateOfBirth",
						"Gender",
						"Residence",
						"Occupation",
						"Rank",
						"MainJob"
					}
				),

				new(
					"03",
					"Rotavirus Vaccination & HIV",
					new[]
					{
						"RotavirusDose1Status",
						"RotavirusDose1Date",
						"RotavirusDose2Status",
						"RotavirusDose2Date",
						"OtherRotavirusVaccineStatus",
						"OtherRotavirusVaccineDate",
						"HivStatus"
					}
				),

				new(
					"04",
					"Symptom Onset & Diarrhoeal Illness",
					new[]
					{
						"FirstSymptom",
						"SymptomOnsetDate",
						"SymptomOnsetTime",
						"DiarrheaDuration",
						"MaxLooseStools24h",
						"LooseStools8h",
						"LooseStools24h",
						"MucousStool",
						"BloodyStool",
						"BloodyStoolDuration",
						"RiceWaterStool"
					}
				),

				new(
					"05",
					"Associated Symptoms",
					new[]
					{
						"AbdominalCramps",
						"AbdominalCrampsDuration",
						"AbdominalCrampsSeverity",
						"ExcessiveGas",
						"ExcessiveGasDuration",
						"ExcessiveGasSeverity",
						"Nausea",
						"NauseaDuration",
						"NauseaSeverity",
						"Fever",
						"FeverDuration",
						"FeverSeverity",
						"PainfulStrainingStool",
						"PainfulStrainingDuration",
						"PainfulStrainingSeverity",
						"MalaiseFatigue",
						"MalaiseFatigueDuration",
						"MalaiseFatigueSeverity",
						"Vomiting",
						"VomitingDuration",
						"VomitingCount",
						"VomitingSeverity",
						"Headache",
						"HeadacheDuration",
						"HeadacheSeverity",
						"LossOfAppetite",
						"LossOfAppetiteDuration",
						"Lightheadedness",
						"LightheadednessDuration",
						"LightheadednessSeverity",
						"StoolUrgency",
						"StoolUrgencyDuration",
						"StoolUrgencySeverity",
						"MuscleAches",
						"MuscleAchesDuration",
						"MuscleAchesSeverity",
						"JointAches",
						"JointAchesDuration",
						"JointAchesSeverity",
						"FecalIncontinence",
						"FecalIncontinenceDuration",
						"FecalIncontinenceSeverity",
						"OtherSymptoms",
						"OtherSymptomsSpecified",
						"OtherSymptomsSeverity",
						"AdditionalSymptoms",
						"AdditionalSymptomsSeverity"
					}
				),

				new(
					"06",
					"Clinical Assessment",
					new[]
					{
						"GeneralCondition",
						"BodyTemperature",
						"BodyWeight",
						"Height",
						"BloodPressure",
						"RespiratoryRate",
						"MalnutritionStatus",
						"ChildConsciousnessResponse",
						"ChildRestlessness",
						"ChildAssessment",
						"SkinPinchBack",
						"CapillaryRefill",
						"ChildDrinkingBreastfeeding",
						"Muac",
						"AdultConsciousnessResponse",
						"PatientClinicalState",
						"IllnessFunctionalImpact"
					}
				),

				new(
					"07",
					"Treatment & Management",
					new[]
					{
						"OutpatientTreatment",
						"TreatmentGiven",
						"Admitted",
						"AdmissionDate",
						"AdmissionTime",
						"OralRehydration",
						"IvRehydration",
						"OtherTreatment"
					}
				),

				new(
					"08",
					"Disposition & Outcome",
					new[]
					{
						"Disposition",
						"DispositionDate",
						"DispositionTime",
						"DischargeDeathDiagnosis"
					}
				),

				new(
					"09",
					"Laboratory Identification",
					new[]
					{
						"BacterialIdentification",
						"ViralIdentification",
						"ParasiteIdentification"
					}
				),

				new(
					"10",
					"Water Source & Treatment",
					new[]
					{
						"WaterSourceType",
						"WaterTreatment"
					}
				)
			};
	}


	// =========================================================
	// SYSTEM FIELDS THAT MUST NOT BE CHANGED
	// =========================================================

	private static readonly HashSet<string>
		ProtectedFields =
			new(
				StringComparer.OrdinalIgnoreCase)
			{
				"QuestionnaireId",
				"CreatedAt",
				"UpdatedAt"
			};


	// =========================================================
	// SPECIMEN ID
	// =========================================================

	private static bool IsValidSpecimenId(
		string? value)
	{
		if (
			string.IsNullOrWhiteSpace(
				value
			)
		)
		{
			return false;
		}


		return System.Text.RegularExpressions.Regex
			.IsMatch(
				value,
				@"^EPS1549-[A-Z0-9]+-[0-9]{4}[AB]$",
				System.Text.RegularExpressions.RegexOptions.IgnoreCase
			);
	}


	private static string? NormalizeSpecimenId(
		string? value)
	{
		return
			string.IsNullOrWhiteSpace(value)
				? null
				: value
					.Trim()
					.ToUpperInvariant();
	}


	// =========================================================
	// SITE CODE
	// =========================================================

	private static string? NormalizeSiteCode(
		string? value)
	{
		return
			string.IsNullOrWhiteSpace(value)
				? null
				: value
					.Trim()
					.ToUpperInvariant();
	}


	// =========================================================
	// LAN
	// =========================================================

	private static string? NormalizeLan(
		string? value)
	{
		return
			string.IsNullOrWhiteSpace(value)
				? null
				: value
					.Trim()
					.ToUpperInvariant();
	}


	// =========================================================
	// GENERAL VALUE NORMALIZATION
	// =========================================================

	private static string? NormalizeValue(
		string? value)
	{
		return
			string.IsNullOrWhiteSpace(value)
				? null
				: value.Trim();
	}


	// =========================================================
	// DISPLAY LABEL
	// =========================================================

	private static string Humanize(
		string propertyName)
	{
		if (
			string.IsNullOrWhiteSpace(
				propertyName
			)
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
				char.IsUpper(character)
				&&
				!char.IsUpper(
					propertyName[i - 1]
				)
			)
			{
				chars.Add(' ');
			}


			chars.Add(character);
		}


		return new string(
			chars.ToArray()
		)
		.Replace(
			"Iv ",
			"IV "
		)
		.Replace(
			"Hiv ",
			"HIV "
		)
		.Replace(
			"Muac",
			"MUAC"
		)
		.Replace(
			"Lan",
			"LAN"
		);
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
								Name = x,
								Label = Humanize(x)
							}
					)
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


		public string? Value { get; set; }
	}
}