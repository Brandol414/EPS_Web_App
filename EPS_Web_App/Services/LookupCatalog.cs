using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Services;

public sealed class LookupCatalog
{
	private readonly ApplicationDbContext _context;

	public LookupCatalog(ApplicationDbContext context)
	{
		_context = context;
	}


	// =========================================================
	// FORM PROPERTY → LOOKUP GROUP
	// =========================================================

	private static readonly Dictionary<string, string>
		PropertyToGroup =
			new(
				StringComparer.OrdinalIgnoreCase)
			{
				// =================================================
				// QUESTIONNAIRE
				// =================================================

				["Gender"] =
					"gender",

				["RotavirusDose1Status"] =
					"yes_no",

				["RotavirusDose2Status"] =
					"yes_no",

				["OtherRotavirusVaccineStatus"] =
					"yes_no",

				["HivStatus"] =
					"hiv_status",

				["FirstSymptom"] =
					"first_symptom",

				["MucousStool"] =
					"yes_no",

				["BloodyStool"] =
					"yes_no",

				["RiceWaterStool"] =
					"yes_no",

				["AbdominalCramps"] =
					"yes_no",

				["AbdominalCrampsSeverity"] =
					"severity",

				["ExcessiveGas"] =
					"yes_no",

				["ExcessiveGasSeverity"] =
					"severity",

				["Nausea"] =
					"yes_no",

				["NauseaSeverity"] =
					"severity",

				["Fever"] =
					"yes_no",

				["FeverSeverity"] =
					"severity",

				["PainfulStrainingStool"] =
					"yes_no",

				["PainfulStrainingSeverity"] =
					"severity",

				["MalaiseFatigue"] =
					"yes_no",

				["MalaiseFatigueSeverity"] =
					"severity",

				["Vomiting"] =
					"yes_no",

				["VomitingSeverity"] =
					"severity",

				["Headache"] =
					"yes_no",

				["HeadacheSeverity"] =
					"severity",

				["LossOfAppetite"] =
					"yes_no",

				["Lightheadedness"] =
					"yes_no",

				["LightheadednessSeverity"] =
					"severity",

				["StoolUrgency"] =
					"yes_no",

				["StoolUrgencySeverity"] =
					"severity",

				["MuscleAches"] =
					"yes_no",

				["MuscleAchesSeverity"] =
					"severity",

				["JointAches"] =
					"yes_no",

				["JointAchesSeverity"] =
					"severity",

				["FecalIncontinence"] =
					"yes_no",

				["FecalIncontinenceSeverity"] =
					"severity",

				["OtherSymptoms"] =
					"yes_no",

				["OtherSymptomsSeverity"] =
					"severity",

				["AdditionalSymptoms"] =
					"yes_no",

				["AdditionalSymptomsSeverity"] =
					"severity",

				["MalnutritionStatus"] =
					"yes_no",

				["ChildRestlessness"] =
					"yes_no",

				["Admitted"] =
					"admission_status",

				["Disposition"] =
					"disposition",


				// =================================================
				// AST
				// =================================================

				["ParticipantType"] =
					"participant_type",

				["GtdStatus"] =
					"yes_no",

				["DiarrheaMedicationPast72h"] =
					"yes_no",

				["MdrStatus"] =
					"yes_no",

				["EsblStatus"] =
					"yes_no"
			};


	// =========================================================
	// DEFAULT LOOKUP GROUP DEFINITIONS
	//
	// Existing groups are NEVER overwritten.
	// Existing values are NEVER silently reactivated.
	// =========================================================

	private static readonly Dictionary<string, LookupDefinition>
		Defaults =
			new(
				StringComparer.OrdinalIgnoreCase)
			{
				// -------------------------------------------------
				// GENDER
				// -------------------------------------------------

				["gender"] =
					new LookupDefinition(
						"gender",
						new[]
						{
							("Male", "Male", 1),
							("Female", "Female", 2)
						}
					),


				// -------------------------------------------------
				// YES / NO
				// -------------------------------------------------

				["yes_no"] =
					new LookupDefinition(
						"yes_no",
						new[]
						{
							("Yes", "Yes", 1),
							("No", "No", 2)
						}
					),


				// -------------------------------------------------
				// SEVERITY
				// -------------------------------------------------

				["severity"] =
					new LookupDefinition(
						"severity",
						new[]
						{
							("Mild", "Mild", 1),
							("Moderate", "Moderate", 2),
							("Severe", "Severe", 3)
						}
					),


				// -------------------------------------------------
				// HIV STATUS
				// -------------------------------------------------

				["hiv_status"] =
					new LookupDefinition(
						"hiv_status",
						new[]
						{
							(
								"HIV negative",
								"HIV negative",
								1
							),

							(
								"HIV positive",
								"HIV positive",
								2
							),

							(
								"Unknown",
								"Unknown",
								3
							)
						}
					),


				// -------------------------------------------------
				// FIRST SYMPTOM
				// -------------------------------------------------

				["first_symptom"] =
					new LookupDefinition(
						"first_symptom",
						new[]
						{
							(
								"Diarrhea",
								"Diarrhea",
								1
							),

							(
								"Fever",
								"Fever",
								2
							),

							(
								"Vomiting",
								"Vomiting",
								3
							),

							(
								"Abdominal cramps",
								"Abdominal cramps",
								4
							),

							(
								"Other",
								"Other",
								5
							)
						}
					),


				// -------------------------------------------------
				// ADMISSION STATUS
				// -------------------------------------------------

				["admission_status"] =
					new LookupDefinition(
						"admission_status",
						new[]
						{
							(
								"No",
								"No — Outpatient",
								1
							),

							(
								"Yes",
								"Yes — Admitted",
								2
							)
						}
					),


				// -------------------------------------------------
				// DISPOSITION
				// -------------------------------------------------

				["disposition"] =
					new LookupDefinition(
						"disposition",
						new[]
						{
							(
								"Discharged",
								"Discharged",
								1
							),

							(
								"Transferred",
								"Transferred",
								2
							),

							(
								"Died",
								"Died",
								3
							),

							(
								"Other",
								"Other",
								4
							)
						}
					),


				// -------------------------------------------------
				// AST PARTICIPANT TYPE
				// -------------------------------------------------

				["participant_type"] =
					new LookupDefinition(
						"participant_type",
						new[]
						{
							(
								"Civilian",
								"Civilian",
								1
							),

							(
								"Military",
								"Military",
								2
							)
						}
					)
			};


	// =========================================================
	// SEED ALL DEFAULT GROUPS
	// =========================================================

	public async Task EnsureDefaultsAsync()
	{
		foreach (
			var definition
			in Defaults.Values)
		{
			var groupExists =
				await _context.LookupValues
					.AsNoTracking()
					.AnyAsync(
						x =>
							x.LookupGroup ==
							definition.Group
					);


			if (groupExists)
			{
				continue;
			}


			var values =
				definition.Values
					.Select(
						x =>
							new LookupValue
							{
								LookupGroup =
									definition.Group,

								LookupCode =
									x.Code,

								LookupLabel =
									x.Label,

								DisplayOrder =
									x.DisplayOrder,

								IsActive =
									true
							}
					)
					.ToList();


			_context.LookupValues.AddRange(
				values
			);
		}


		await _context.SaveChangesAsync();
	}


	// =========================================================
	// GET GROUP FOR A FORM PROPERTY
	// =========================================================

	public static string? GetGroupForProperty(
		string propertyName)
	{
		return PropertyToGroup.TryGetValue(
			propertyName,
			out var group)
			? group
			: null;
	}


	// =========================================================
	// GET ACTIVE OPTIONS
	// =========================================================

	public async Task<List<LookupOption>>
		GetActiveOptionsAsync(
			string group)
	{
		return
			await _context.LookupValues
				.AsNoTracking()
				.Where(
					x =>
						x.LookupGroup ==
						group
						&&
						x.IsActive
				)
				.OrderBy(
					x =>
						x.DisplayOrder
						??
						int.MaxValue
				)
				.ThenBy(
					x =>
						x.LookupLabel
				)
				.Select(
					x =>
						new LookupOption
						{
							Code =
								x.LookupCode,

							Label =
								x.LookupLabel
						}
				)
				.ToListAsync();
	}


	// =========================================================
	// SUPPORT TYPES
	// =========================================================

	private sealed record LookupDefinition(
		string Group,
		IReadOnlyList<(
			string Code,
			string Label,
			int DisplayOrder)> Values
	);


	public sealed class LookupOption
	{
		public string Code { get; init; } =
			string.Empty;

		public string Label { get; init; } =
			string.Empty;
	}
}