using System.Globalization;
using System.Text.RegularExpressions;
using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Questionnaire;

public class CreateModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public CreateModel(ApplicationDbContext context)
	{
		_context = context;
	}

	// =========================================================
	// FORM INPUT
	// QuestionnaireInput now lives in:
	// Data/Models/QuestionnaireInput.cs
	// =========================================================

	[BindProperty]
	public QuestionnaireInput Input { get; set; } = new();

	// =========================================================
	// POST
	// =========================================================

	public async Task<IActionResult> OnPostAsync()
	{
		NormalizeIdentity();

		NormalizeWaterSelections();

		NormalizeDateTimeFields();

		ValidateIdentity();

		ApplySkipLogic();

		if (!ModelState.IsValid)
		{
			return Page();
		}

		// -----------------------------------------------------
		// PREVENT DUPLICATE SPECIMEN ID
		// -----------------------------------------------------

		var specimenExists =
			await _context.QuestionnaireEntries
				.AsNoTracking()
				.AnyAsync(
					x =>
						x.SpecimenId ==
						Input.SpecimenId
				);

		if (specimenExists)
		{
			ModelState.AddModelError(
				"Input.SpecimenId",
				"This Specimen ID already exists in the questionnaire database."
			);

			return Page();
		}

		// =====================================================
		// CREATE QUESTIONNAIRE RECORD
		// =====================================================

		var record =
			new QuestionnaireEntry
			{
				// =================================================
				// 01 — RECORD IDENTITY
				// =================================================

				SiteCode =
					Input.SiteCode,

				SpecimenId =
					Input.SpecimenId!,

				Lan =
					Input.Lan,

				IsolationDate =
					Input.IsolationDate,

				// =================================================
				// 02 — PARTICIPANT
				// =================================================

				Age =
					Input.Age,

				DateOfBirth =
					Input.DateOfBirth,

				Gender =
					Input.Gender,

				Residence =
					Input.Residence,

				Occupation =
					Input.Occupation,

				Rank =
					Input.Rank,

				MainJob =
					Input.MainJob,

				// =================================================
				// 03 — ROTAVIRUS / HIV
				// =================================================

				RotavirusDose1Status =
					Input.RotavirusDose1Status,

				RotavirusDose1Date =
					Input.RotavirusDose1Date,

				RotavirusDose2Status =
					Input.RotavirusDose2Status,

				RotavirusDose2Date =
					Input.RotavirusDose2Date,

				OtherRotavirusVaccineStatus =
					Input.OtherRotavirusVaccineStatus,

				OtherRotavirusVaccineDate =
					Input.OtherRotavirusVaccineDate,

				HivStatus =
					Input.HivStatus,

				// =================================================
				// 04 — SYMPTOM ONSET / DIARRHOEA
				// =================================================

				FirstSymptom =
					Input.FirstSymptom,

				SymptomOnsetDate =
					Input.SymptomOnsetDate,

				SymptomOnsetTime =
					Input.SymptomOnsetTime,

				DiarrheaDuration =
					Input.DiarrheaDuration,

				MaxLooseStools24h =
					Input.MaxLooseStools24h,

				LooseStools8h =
					Input.LooseStools8h,

				LooseStools24h =
					Input.LooseStools24h,

				MucousStool =
					Input.MucousStool,

				BloodyStool =
					Input.BloodyStool,

				BloodyStoolDuration =
					Input.BloodyStoolDuration,

				RiceWaterStool =
					Input.RiceWaterStool,

				// =================================================
				// 05 — ASSOCIATED SYMPTOMS
				// =================================================

				AbdominalCramps =
					Input.AbdominalCramps,

				AbdominalCrampsDuration =
					Input.AbdominalCrampsDuration,

				AbdominalCrampsSeverity =
					Input.AbdominalCrampsSeverity,

				ExcessiveGas =
					Input.ExcessiveGas,

				ExcessiveGasDuration =
					Input.ExcessiveGasDuration,

				ExcessiveGasSeverity =
					Input.ExcessiveGasSeverity,

				Nausea =
					Input.Nausea,

				NauseaDuration =
					Input.NauseaDuration,

				NauseaSeverity =
					Input.NauseaSeverity,

				Fever =
					Input.Fever,

				FeverDuration =
					Input.FeverDuration,

				FeverSeverity =
					Input.FeverSeverity,

				PainfulStrainingStool =
					Input.PainfulStrainingStool,

				PainfulStrainingDuration =
					Input.PainfulStrainingDuration,

				PainfulStrainingSeverity =
					Input.PainfulStrainingSeverity,

				MalaiseFatigue =
					Input.MalaiseFatigue,

				MalaiseFatigueDuration =
					Input.MalaiseFatigueDuration,

				MalaiseFatigueSeverity =
					Input.MalaiseFatigueSeverity,

				Vomiting =
					Input.Vomiting,

				VomitingDuration =
					Input.VomitingDuration,

				VomitingCount =
					Input.VomitingCount,

				VomitingSeverity =
					Input.VomitingSeverity,

				Headache =
					Input.Headache,

				HeadacheDuration =
					Input.HeadacheDuration,

				HeadacheSeverity =
					Input.HeadacheSeverity,

				LossOfAppetite =
					Input.LossOfAppetite,

				LossOfAppetiteDuration =
					Input.LossOfAppetiteDuration,

				Lightheadedness =
					Input.Lightheadedness,

				LightheadednessDuration =
					Input.LightheadednessDuration,

				LightheadednessSeverity =
					Input.LightheadednessSeverity,

				StoolUrgency =
					Input.StoolUrgency,

				StoolUrgencyDuration =
					Input.StoolUrgencyDuration,

				StoolUrgencySeverity =
					Input.StoolUrgencySeverity,

				MuscleAches =
					Input.MuscleAches,

				MuscleAchesDuration =
					Input.MuscleAchesDuration,

				MuscleAchesSeverity =
					Input.MuscleAchesSeverity,

				JointAches =
					Input.JointAches,

				JointAchesDuration =
					Input.JointAchesDuration,

				JointAchesSeverity =
					Input.JointAchesSeverity,

				FecalIncontinence =
					Input.FecalIncontinence,

				FecalIncontinenceDuration =
					Input.FecalIncontinenceDuration,

				FecalIncontinenceSeverity =
					Input.FecalIncontinenceSeverity,

				OtherSymptoms =
					Input.OtherSymptoms,

				OtherSymptomsSpecified =
					Input.OtherSymptomsSpecified,

				OtherSymptomsSeverity =
					Input.OtherSymptomsSeverity,

				AdditionalSymptoms =
					Input.AdditionalSymptoms,

				AdditionalSymptomsSeverity =
					Input.AdditionalSymptomsSeverity,

				// =================================================
				// 06 — CLINICAL ASSESSMENT
				// =================================================

				GeneralCondition =
					Input.GeneralCondition,

				BodyTemperature =
					Input.BodyTemperature,

				BodyWeight =
					Input.BodyWeight,

				Height =
					Input.Height,

				BloodPressure =
					Input.BloodPressure,

				RespiratoryRate =
					Input.RespiratoryRate,

				MalnutritionStatus =
					Input.MalnutritionStatus,

				ChildConsciousnessResponse =
					Input.ChildConsciousnessResponse,

				ChildRestlessness =
					Input.ChildRestlessness,

				ChildAssessment =
					Input.ChildAssessment,

				SkinPinchBack =
					Input.SkinPinchBack,

				CapillaryRefill =
					Input.CapillaryRefill,

				ChildDrinkingBreastfeeding =
					Input.ChildDrinkingBreastfeeding,

				Muac =
					Input.Muac,

				AdultConsciousnessResponse =
					Input.AdultConsciousnessResponse,

				PatientClinicalState =
					Input.PatientClinicalState,

				IllnessFunctionalImpact =
					Input.IllnessFunctionalImpact,

				// =================================================
				// 07 — TREATMENT / MANAGEMENT
				// =================================================

				OutpatientTreatment =
					Input.OutpatientTreatment,

				TreatmentGiven =
					Input.TreatmentGiven,

				Admitted =
					Input.Admitted,

				AdmissionDate =
					Input.AdmissionDate,

				AdmissionTime =
					Input.AdmissionTime,

				OralRehydration =
					Input.OralRehydration,

				IvRehydration =
					Input.IvRehydration,

				OtherTreatment =
					Input.OtherTreatment,

				// =================================================
				// 08 — DISPOSITION / OUTCOME
				// =================================================

				Disposition =
					Input.Disposition,

				DispositionDate =
					Input.DispositionDate,

				DispositionTime =
					Input.DispositionTime,

				DischargeDeathDiagnosis =
					Input.DischargeDeathDiagnosis,

				// =================================================
				// 09 — LABORATORY IDENTIFICATION
				// =================================================

				BacterialIdentification =
					Input.BacterialIdentification,

				ViralIdentification =
					Input.ViralIdentification,

				ParasiteIdentification =
					Input.ParasiteIdentification,

				// =================================================
				// 10 — WATER SOURCE / TREATMENT
				// =================================================

				WaterSourceType =
					Input.WaterSourceType,

				WaterTreatment =
					Input.WaterTreatment
			};

		_context.QuestionnaireEntries.Add(
			record
		);

		await _context.SaveChangesAsync();

		return RedirectToPage(
			"./View",
			new
			{
				id = record.QuestionnaireId
			}
		);
	}

	// =========================================================
	// NORMALIZE IDENTITY
	// =========================================================

	private void NormalizeIdentity()
	{
		if (!string.IsNullOrWhiteSpace(
				Input.SpecimenId))
		{
			Input.SpecimenId =
				Input.SpecimenId
					.Trim()
					.ToUpperInvariant();
		}

		if (!string.IsNullOrWhiteSpace(
				Input.SiteCode))
		{
			Input.SiteCode =
				Input.SiteCode
					.Trim()
					.ToUpperInvariant();
		}

		if (!string.IsNullOrWhiteSpace(
				Input.Lan))
		{
			Input.Lan =
				Input.Lan
					.Trim()
					.ToUpperInvariant();
		}
	}

	// =========================================================
	// DATE / TIME NORMALIZATION
	// =========================================================

	private void NormalizeDateTimeFields()
	{
		Input.IsolationDate =
			NormalizeDateField(
				Input.IsolationDate,
				"Input.IsolationDate");

		Input.DateOfBirth =
			NormalizeDateField(
				Input.DateOfBirth,
				"Input.DateOfBirth");

		Input.RotavirusDose1Date =
			NormalizeDateField(
				Input.RotavirusDose1Date,
				"Input.RotavirusDose1Date");

		Input.RotavirusDose2Date =
			NormalizeDateField(
				Input.RotavirusDose2Date,
				"Input.RotavirusDose2Date");

		Input.OtherRotavirusVaccineDate =
			NormalizeDateField(
				Input.OtherRotavirusVaccineDate,
				"Input.OtherRotavirusVaccineDate");

		Input.SymptomOnsetDate =
			NormalizeDateField(
				Input.SymptomOnsetDate,
				"Input.SymptomOnsetDate");

		Input.SymptomOnsetTime =
			NormalizeTimeField(
				Input.SymptomOnsetTime,
				"Input.SymptomOnsetTime");

		Input.AdmissionDate =
			NormalizeDateField(
				Input.AdmissionDate,
				"Input.AdmissionDate");

		Input.AdmissionTime =
			NormalizeTimeField(
				Input.AdmissionTime,
				"Input.AdmissionTime");

		Input.DispositionDate =
			NormalizeDateField(
				Input.DispositionDate,
				"Input.DispositionDate");

		Input.DispositionTime =
			NormalizeTimeField(
				Input.DispositionTime,
				"Input.DispositionTime");
	}


	private string? NormalizeDateField(
		string? value,
		string modelStateKey)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		value = value.Trim();

		if (DateTime.TryParseExact(
				value,
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var parsedDate))
		{
			return parsedDate.ToString(
				"yyyy-MM-dd",
				CultureInfo.InvariantCulture);
		}

		ModelState.AddModelError(
			modelStateKey,
			"Please enter a valid date in YYYY-MM-DD format.");

		return value;
	}


	private string? NormalizeTimeField(
		string? value,
		string modelStateKey)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		value = value.Trim();

		if (TimeSpan.TryParseExact(
				value,
				new[]
				{
					@"hh\:mm",
					@"h\:mm"
				},
				CultureInfo.InvariantCulture,
				out var parsedTime))
		{
			return parsedTime.ToString(
				@"hh\:mm",
				CultureInfo.InvariantCulture);
		}

		ModelState.AddModelError(
			modelStateKey,
			"Please enter a valid time in HH:MM format.");

		return value;
	}


	// =========================================================
	// WATER NORMALIZATION
	// =========================================================

	private void NormalizeWaterSelections()
	{
		// -----------------------------------------------------
		// WATER SOURCE
		// -----------------------------------------------------

		var sourceSelections =
			Input.WaterSourceSelections
				.Where(
					x =>
						!string.IsNullOrWhiteSpace(x)
				)
				.Select(
					x => x.Trim()
				)
				.Distinct(
					StringComparer.OrdinalIgnoreCase
				)
				.ToList();

		if (
			sourceSelections.Any(
				x =>
					string.Equals(
						x,
						"Other",
						StringComparison.OrdinalIgnoreCase
					)
			)
		)
		{
			var otherSource =
				Input.OtherWaterSource?
					.Trim();

			sourceSelections.RemoveAll(
				x =>
					string.Equals(
						x,
						"Other",
						StringComparison.OrdinalIgnoreCase
					)
			);

			if (!string.IsNullOrWhiteSpace(
					otherSource))
			{
				sourceSelections.Add(
					$"Other: {otherSource}"
				);
			}
		}

		Input.WaterSourceType =
			sourceSelections.Count == 0
				? null
				: string.Join(
					"; ",
					sourceSelections
				);

		// -----------------------------------------------------
		// WATER TREATMENT
		// -----------------------------------------------------

		var treatmentSelections =
			Input.WaterTreatmentSelections
				.Where(
					x =>
						!string.IsNullOrWhiteSpace(x)
				)
				.Select(
					x => x.Trim()
				)
				.Distinct(
					StringComparer.OrdinalIgnoreCase
				)
				.ToList();

		if (
			treatmentSelections.Any(
				x =>
					string.Equals(
						x,
						"Other",
						StringComparison.OrdinalIgnoreCase
					)
			)
		)
		{
			var otherTreatment =
				Input.OtherWaterTreatment?
					.Trim();

			treatmentSelections.RemoveAll(
				x =>
					string.Equals(
						x,
						"Other",
						StringComparison.OrdinalIgnoreCase
					)
			);

			if (!string.IsNullOrWhiteSpace(
					otherTreatment))
			{
				treatmentSelections.Add(
					$"Other: {otherTreatment}"
				);
			}
		}

		Input.WaterTreatment =
			treatmentSelections.Count == 0
				? null
				: string.Join(
					"; ",
					treatmentSelections
				);
	}

	// =========================================================
	// IDENTITY VALIDATION
	// =========================================================

	private void ValidateIdentity()
	{
		if (string.IsNullOrWhiteSpace(
				Input.SpecimenId))
		{
			ModelState.AddModelError(
				"Input.SpecimenId",
				"Specimen ID is required."
			);

			return;
		}

		if (!Regex.IsMatch(
				Input.SpecimenId,
				@"^EPS1549-[A-Z0-9]+-[0-9]{4}[AB]$"))
		{
			ModelState.AddModelError(
				"Input.SpecimenId",
				"Specimen ID must follow the EPS1549-SITE-####A/B format."
			);

			return;
		}

		var parts =
			Input.SpecimenId.Split('-');

		if (parts.Length != 3)
		{
			ModelState.AddModelError(
				"Input.SpecimenId",
				"Specimen ID format is invalid."
			);

			return;
		}

		var derivedSite =
			parts[1];

		if (string.IsNullOrWhiteSpace(
				Input.SiteCode))
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

		ValidateLan();
	}

	// =========================================================
	// LAN VALIDATION
	// =========================================================

	private void ValidateLan()
	{
		if (string.IsNullOrWhiteSpace(
				Input.Lan))
		{
			return;
		}

		if (Regex.IsMatch(
				Input.Lan,
				@"^MHK[0-9]{5}$"))
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

		if (historicalExceptions.Contains(
				Input.Lan))
		{
			return;
		}

		ModelState.AddModelError(
			"Input.Lan",
			"LAN must follow MHK##### or be a recognized historical exception."
		);
	}

	// =========================================================
	// SKIP LOGIC
	// =========================================================

	private void ApplySkipLogic()
	{
		// -----------------------------------------------------
		// DIARRHEA
		// -----------------------------------------------------

		if (!string.Equals(
				Input.FirstSymptom,
				"Diarrhea",
				StringComparison.OrdinalIgnoreCase))
		{
			Input.DiarrheaDuration = null;
		}

		// -----------------------------------------------------
		// BLOODY STOOL
		// -----------------------------------------------------

		if (!IsYes(
				Input.BloodyStool))
		{
			Input.BloodyStoolDuration = null;
		}

		// -----------------------------------------------------
		// ROTAVIRUS
		// -----------------------------------------------------

		if (!IsYes(
				Input.RotavirusDose1Status))
		{
			Input.RotavirusDose1Date = null;
		}

		if (!IsYes(
				Input.RotavirusDose2Status))
		{
			Input.RotavirusDose2Date = null;
		}

		if (!IsYes(
				Input.OtherRotavirusVaccineStatus))
		{
			Input.OtherRotavirusVaccineDate = null;
		}

		// -----------------------------------------------------
		// ASSOCIATED SYMPTOMS
		// -----------------------------------------------------

		if (!IsYes(Input.AbdominalCramps))
		{
			Input.AbdominalCrampsDuration = null;
			Input.AbdominalCrampsSeverity = null;
		}

		if (!IsYes(Input.ExcessiveGas))
		{
			Input.ExcessiveGasDuration = null;
			Input.ExcessiveGasSeverity = null;
		}

		if (!IsYes(Input.Nausea))
		{
			Input.NauseaDuration = null;
			Input.NauseaSeverity = null;
		}

		if (!IsYes(Input.Fever))
		{
			Input.FeverDuration = null;
			Input.FeverSeverity = null;
		}

		if (!IsYes(Input.PainfulStrainingStool))
		{
			Input.PainfulStrainingDuration = null;
			Input.PainfulStrainingSeverity = null;
		}

		if (!IsYes(Input.MalaiseFatigue))
		{
			Input.MalaiseFatigueDuration = null;
			Input.MalaiseFatigueSeverity = null;
		}

		if (!IsYes(Input.Vomiting))
		{
			Input.VomitingDuration = null;
			Input.VomitingCount = null;
			Input.VomitingSeverity = null;
		}

		if (!IsYes(Input.Headache))
		{
			Input.HeadacheDuration = null;
			Input.HeadacheSeverity = null;
		}

		if (!IsYes(Input.LossOfAppetite))
		{
			Input.LossOfAppetiteDuration = null;
		}

		if (!IsYes(Input.Lightheadedness))
		{
			Input.LightheadednessDuration = null;
			Input.LightheadednessSeverity = null;
		}

		if (!IsYes(Input.StoolUrgency))
		{
			Input.StoolUrgencyDuration = null;
			Input.StoolUrgencySeverity = null;
		}

		if (!IsYes(Input.MuscleAches))
		{
			Input.MuscleAchesDuration = null;
			Input.MuscleAchesSeverity = null;
		}

		if (!IsYes(Input.JointAches))
		{
			Input.JointAchesDuration = null;
			Input.JointAchesSeverity = null;
		}

		if (!IsYes(Input.FecalIncontinence))
		{
			Input.FecalIncontinenceDuration = null;
			Input.FecalIncontinenceSeverity = null;
		}

		if (!IsYes(Input.OtherSymptoms))
		{
			Input.OtherSymptomsSpecified = null;
			Input.OtherSymptomsSeverity = null;
		}

		if (!IsYes(Input.AdditionalSymptoms))
		{
			Input.AdditionalSymptomsSeverity = null;
		}

		// -----------------------------------------------------
		// CHILD / ADULT ASSESSMENT
		// -----------------------------------------------------

		var assessmentGroup =
			GetAssessmentGroup(
				Input.Age
			);

		if (
			assessmentGroup ==
			AssessmentGroup.Child
		)
		{
			Input.AdultConsciousnessResponse = null;
			Input.PatientClinicalState = null;
			Input.IllnessFunctionalImpact = null;
		}

		if (
			assessmentGroup ==
			AssessmentGroup.Adult
		)
		{
			Input.ChildConsciousnessResponse = null;
			Input.ChildRestlessness = null;
			Input.ChildAssessment = null;
			Input.SkinPinchBack = null;
			Input.CapillaryRefill = null;
			Input.ChildDrinkingBreastfeeding = null;
			Input.Muac = null;
		}

		// -----------------------------------------------------
		// OUTPATIENT / ADMITTED
		// -----------------------------------------------------

		if (IsYes(
				Input.Admitted))
		{
			// Admitted patients do not use outpatient fields.
			Input.OutpatientTreatment = null;
			Input.TreatmentGiven = null;
		}
		else
		{
			// Outpatients do not use admitted-only fields.
			Input.AdmissionDate = null;
			Input.AdmissionTime = null;

			Input.OralRehydration = null;
			Input.IvRehydration = null;
			Input.OtherTreatment = null;

			Input.Disposition = null;
			Input.DispositionDate = null;
			Input.DispositionTime = null;
			Input.DischargeDeathDiagnosis = null;
		}

		// -----------------------------------------------------
		// DISPOSITION DETAILS
		// -----------------------------------------------------

		if (
			IsYes(Input.Admitted) &&
			string.IsNullOrWhiteSpace(
				Input.Disposition)
		)
		{
			Input.DispositionDate = null;
			Input.DispositionTime = null;
			Input.DischargeDeathDiagnosis = null;
		}

		// -----------------------------------------------------
		// OTHER WATER SOURCE
		// -----------------------------------------------------

		if (
			!Input.WaterSourceSelections.Any(
				x =>
					string.Equals(
						x,
						"Other",
						StringComparison.OrdinalIgnoreCase
					)
			)
		)
		{
			Input.OtherWaterSource = null;
		}

		// -----------------------------------------------------
		// OTHER WATER TREATMENT
		// -----------------------------------------------------

		if (
			!Input.WaterTreatmentSelections.Any(
				x =>
					string.Equals(
						x,
						"Other",
						StringComparison.OrdinalIgnoreCase
					)
			)
		)
		{
			Input.OtherWaterTreatment = null;
		}
	}

	// =========================================================
	// AGE GROUP
	// =========================================================

	private static AssessmentGroup GetAssessmentGroup(
		string? age)
	{
		if (string.IsNullOrWhiteSpace(age))
		{
			return AssessmentGroup.Unknown;
		}

		var value =
			age.Trim()
				.ToLowerInvariant();

		if (
			value.Contains("child") ||
			value.Contains("infant") ||
			value.Contains("baby")
		)
		{
			return AssessmentGroup.Child;
		}

		if (value.Contains("adult"))
		{
			return AssessmentGroup.Adult;
		}

		if (
			value.Contains("month") ||
			value.Contains("week") ||
			value.Contains("day")
		)
		{
			return AssessmentGroup.Child;
		}

		var match =
			Regex.Match(
				value,
				@"(\d+(\.\d+)?)"
			);

		if (!match.Success)
		{
			return AssessmentGroup.Unknown;
		}

		if (!double.TryParse(
				match.Groups[1].Value,
				out var numericAge))
		{
			return AssessmentGroup.Unknown;
		}

		if (
			value.Contains("year") ||
			value.Contains("yr") ||
			value.Contains("yrs")
		)
		{
			return numericAge < 18
				? AssessmentGroup.Child
				: AssessmentGroup.Adult;
		}

		return numericAge < 18
			? AssessmentGroup.Child
			: AssessmentGroup.Adult;
	}

	private enum AssessmentGroup
	{
		Unknown,
		Child,
		Adult
	}

	// =========================================================
	// YES HELPER
	// =========================================================

	private static bool IsYes(
		string? value)
	{
		return string.Equals(
			value,
			"Yes",
			StringComparison.OrdinalIgnoreCase
		);
	}
}