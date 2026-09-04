using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Services;

public sealed class ReconciliationService
{
	private readonly ApplicationDbContext _context;

	public ReconciliationService(
		ApplicationDbContext context)
	{
		_context = context;
	}

	// =========================================================
	// FULL RECONCILIATION
	// =========================================================

	public async Task<ReconciliationRunResult> RunAsync(
		CancellationToken cancellationToken = default)
	{
		return await RunInternalAsync(
			null,
			cancellationToken);
	}


	// =========================================================
	// TARGETED RECONCILIATION — ONE SPECIMEN
	// =========================================================

	public async Task<ReconciliationRunResult>
		RunForSpecimenAsync(
			string specimenId,
			CancellationToken cancellationToken = default)
	{
		var normalizedSpecimenId =
			Normalize(specimenId);

		if (
			string.IsNullOrWhiteSpace(
				normalizedSpecimenId)
		)
		{
			return new ReconciliationRunResult();
		}

		var affectedSpecimens =
			new HashSet<string>(
				StringComparer.OrdinalIgnoreCase)
			{
				normalizedSpecimenId
			};

		return await RunInternalAsync(
			affectedSpecimens,
			cancellationToken);
	}


	// =========================================================
	// RECONCILE AFTER SPECIMEN ID CHANGE
	// =========================================================

	public async Task<ReconciliationRunResult>
		ReconcileSpecimenIdChangeAsync(
			string oldSpecimenId,
			string newSpecimenId,
			string changedBy,
			string? reason = null,
			CancellationToken cancellationToken = default)
	{
		var oldId =
			Normalize(oldSpecimenId);

		var newId =
			Normalize(newSpecimenId);


		// -----------------------------------------------------
		// Nothing meaningful changed.
		// -----------------------------------------------------

		if (
			string.IsNullOrWhiteSpace(oldId)
			||
			string.IsNullOrWhiteSpace(newId)
			||
			string.Equals(
				oldId,
				newId,
				StringComparison.OrdinalIgnoreCase)
		)
		{
			return await RunForSpecimenAsync(
				newId,
				cancellationToken);
		}


		// -----------------------------------------------------
		// Preserve old alerts as history.
		// -----------------------------------------------------

		var oldAlerts =
			await _context.ReconciliationAlerts
				.Where(
					x =>
						x.Status != "Resolved"
						&&
						x.SpecimenId != null
						&&
						x.SpecimenId != ""
						&&
						x.SpecimenId == oldId
				)
				.ToListAsync(
					cancellationToken);


		var resolvedAt =
			DateTime.UtcNow;


		foreach (
			var alert
			in oldAlerts)
		{
			alert.Status =
				"Resolved";

			alert.ResolvedBy =
				changedBy;

			alert.ResolvedAt =
				resolvedAt;

			alert.ResolutionNote =
				string.IsNullOrWhiteSpace(reason)
					?
					$"Automatically superseded after administrator corrected the questionnaire Specimen ID from '{oldId}' to '{newId}'."
					:
					$"Automatically superseded after administrator corrected the Specimen ID from '{oldId}' to '{newId}'. Reason: {reason.Trim()}";
		}


		if (
			oldAlerts.Count > 0
		)
		{
			await _context.SaveChangesAsync(
				cancellationToken);
		}


		// -----------------------------------------------------
		// Reconcile ONLY old and new identities.
		// -----------------------------------------------------

		var affectedSpecimens =
			new HashSet<string>(
				StringComparer.OrdinalIgnoreCase)
			{
				oldId,
				newId
			};


		return await RunInternalAsync(
			affectedSpecimens,
			cancellationToken);
	}


	// =========================================================
	// INTERNAL RECONCILIATION ENGINE
	// =========================================================

	private async Task<ReconciliationRunResult>
		RunInternalAsync(
			HashSet<string>? affectedSpecimens,
			CancellationToken cancellationToken)
	{
		// -----------------------------------------------------
		// QUESTIONNAIRE RECORDS
		// -----------------------------------------------------

		var questionnaireQuery =
			_context.QuestionnaireEntries
				.AsNoTracking()
				.Where(
					x =>
						x.SpecimenId != null
						&&
						x.SpecimenId != ""
				);


		var questionnaireRecords =
			await questionnaireQuery
				.ToListAsync(
					cancellationToken);


		if (
			affectedSpecimens != null
		)
		{
			questionnaireRecords =
				questionnaireRecords
					.Where(
						x =>
							affectedSpecimens.Contains(
								Normalize(
									x.SpecimenId)
							)
					)
					.ToList();
		}


		var questionnaireBySpecimen =
			questionnaireRecords
				.GroupBy(
					x =>
						Normalize(
							x.SpecimenId)
				)
				.Where(
					x =>
						!string.IsNullOrWhiteSpace(
							x.Key)
				)
				.ToDictionary(
					x => x.Key,
					x => x.First(),
					StringComparer.OrdinalIgnoreCase
				);


		// -----------------------------------------------------
		// AST RECORDS
		// -----------------------------------------------------

		var astRecords =
			await _context.AstRecords
				.AsNoTracking()
				.Where(
					x =>
						x.SpecimenId != null
						&&
						x.SpecimenId != ""
				)
				.OrderBy(
					x =>
						x.AstRecordId
				)
				.ToListAsync(
					cancellationToken);


		if (
			affectedSpecimens != null
		)
		{
			astRecords =
				astRecords
					.Where(
						x =>
							affectedSpecimens.Contains(
								Normalize(
									x.SpecimenId)
							)
					)
					.ToList();
		}


		// -----------------------------------------------------
		// EXISTING UNRESOLVED ALERTS
		// -----------------------------------------------------

		var existingAlerts =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.Where(
					x =>
						x.Status != "Resolved"
						&&
						x.SpecimenId != null
						&&
						x.SpecimenId != ""
				)
				.ToListAsync(
					cancellationToken);


		if (
			affectedSpecimens != null
		)
		{
			existingAlerts =
				existingAlerts
					.Where(
						x =>
							affectedSpecimens.Contains(
								Normalize(
									x.SpecimenId)
							)
					)
					.ToList();
		}


		var existingAlertKeys =
			new HashSet<string>(
				existingAlerts.Select(
					x =>
						BuildAlertKey(
							x.SpecimenId!,
							x.AlertType)
				),
				StringComparer.OrdinalIgnoreCase
			);


		// -----------------------------------------------------
		// RESULT COLLECTION
		// -----------------------------------------------------

		var newAlerts =
			new List<ReconciliationAlert>();


		var duplicateAlertsSkipped =
			0;


		var fieldDiscrepanciesDetected =
			0;


		var specimensWithFieldDiscrepancies =
			0;


		// =====================================================
		// INSPECT AST RECORDS
		// =====================================================

		foreach (
			var ast
			in astRecords)
		{
			cancellationToken.ThrowIfCancellationRequested();


			if (
				string.IsNullOrWhiteSpace(
					ast.SpecimenId)
			)
			{
				continue;
			}


			var specimenId =
				Normalize(
					ast.SpecimenId);


			// -------------------------------------------------
			// NO QUESTIONNAIRE MATCH
			// -------------------------------------------------

			if (
				!questionnaireBySpecimen.TryGetValue(
					specimenId,
					out var questionnaire)
			)
			{
				const string alertType =
					"AST_NO_QUESTIONNAIRE_MATCH";


				var key =
					BuildAlertKey(
						specimenId,
						alertType);


				if (
					existingAlertKeys.Contains(
						key)
				)
				{
					duplicateAlertsSkipped++;

					continue;
				}


				newAlerts.Add(
					new ReconciliationAlert
					{
						SpecimenId =
							specimenId,

						AlertType =
							alertType,

						Priority =
							"Medium",

						Status =
							"Open",

						SourceRecord =
							$"AST:{ast.AstRecordId}",

						Description =
							$"AST record #{ast.AstRecordId} exists for specimen {specimenId}, but no questionnaire record with the same Specimen ID was found.",

						CreatedAt =
							DateTime.UtcNow
					}
				);


				existingAlertKeys.Add(
					key);

				continue;
			}


			// -------------------------------------------------
			// AST LINKAGE STATUS
			// -------------------------------------------------

			if (
				!string.Equals(
					ast.LinkageStatus,
					"Linked",
					StringComparison.OrdinalIgnoreCase)
			)
			{
				const string alertType =
					"AST_LINKAGE_MISMATCH";


				var key =
					BuildAlertKey(
						specimenId,
						alertType);


				if (
					existingAlertKeys.Contains(
						key)
				)
				{
					duplicateAlertsSkipped++;
				}
				else
				{
					newAlerts.Add(
						new ReconciliationAlert
						{
							SpecimenId =
								specimenId,

							AlertType =
								alertType,

							Priority =
								"High",

							Status =
								"Open",

							SourceRecord =
								$"AST:{ast.AstRecordId}",

							Description =
								$"AST record #{ast.AstRecordId} matches questionnaire specimen {specimenId}, but its linkage status is '{ast.LinkageStatus ?? "Unlinked"}'.",

							CreatedAt =
								DateTime.UtcNow
						}
					);


					existingAlertKeys.Add(
						key);
				}
			}


			// -------------------------------------------------
			// FIELD-LEVEL COMPARISON
			// -------------------------------------------------

			var discrepancies =
				CompareFields(
					questionnaire,
					ast);


			if (
				discrepancies.Count == 0
			)
			{
				continue;
			}


			fieldDiscrepanciesDetected +=
				discrepancies.Count;


			specimensWithFieldDiscrepancies++;


			const string fieldAlertType =
				"FIELD_LEVEL_MISMATCH";


			var fieldAlertKey =
				BuildAlertKey(
					specimenId,
					fieldAlertType);


			if (
				existingAlertKeys.Contains(
					fieldAlertKey)
			)
			{
				duplicateAlertsSkipped++;

				continue;
			}


			var description =
				BuildFieldDiscrepancyDescription(
					specimenId,
					ast.AstRecordId,
					discrepancies);


			var priority =
				DetermineFieldMismatchPriority(
					discrepancies);


			newAlerts.Add(
				new ReconciliationAlert
				{
					SpecimenId =
						specimenId,

					AlertType =
						fieldAlertType,

					Priority =
						priority,

					Status =
						"Open",

					SourceRecord =
						$"AST:{ast.AstRecordId}",

					Description =
						description,

					CreatedAt =
						DateTime.UtcNow
				}
			);


			existingAlertKeys.Add(
				fieldAlertKey);
		}


		// =====================================================
		// SAVE NEW ALERTS
		// =====================================================

		if (
			newAlerts.Count > 0
		)
		{
			_context.ReconciliationAlerts.AddRange(
				newAlerts);

			await _context.SaveChangesAsync(
				cancellationToken);
		}


		// =====================================================
		// RESULT
		// =====================================================

		return new ReconciliationRunResult
		{
			AstRecordsChecked =
				astRecords.Count,

			QuestionnaireSpecimensChecked =
				questionnaireBySpecimen.Count,

			AlertsCreated =
				newAlerts.Count,

			DuplicateAlertsSkipped =
				duplicateAlertsSkipped,

			FieldDiscrepanciesDetected =
				fieldDiscrepanciesDetected,

			SpecimensWithFieldDiscrepancies =
				specimensWithFieldDiscrepancies
		};
	}


	// =========================================================
	// FIELD COMPARISON
	// =========================================================

	private static List<FieldDiscrepancy> CompareFields(
		QuestionnaireEntry questionnaire,
		AstRecord ast)
	{
		var discrepancies =
			new List<FieldDiscrepancy>();


		Compare(
			discrepancies,
			"LAN",
			questionnaire.Lan,
			ast.Lan);


		Compare(
			discrepancies,
			"Site Code",
			questionnaire.SiteCode,
			ast.SiteCode);


		Compare(
			discrepancies,
			"Bacterial Identification",
			questionnaire.BacterialIdentification,
			ast.BacterialIdentification);


		Compare(
			discrepancies,
			"Viral Identification",
			questionnaire.ViralIdentification,
			ast.ViralIdentification);


		Compare(
			discrepancies,
			"Parasite Identification",
			questionnaire.ParasiteIdentification,
			ast.ParasiteIdentification);


		Compare(
			discrepancies,
			"Illness Functional Impact",
			questionnaire.IllnessFunctionalImpact,
			ast.IllnessFunctionalImpact);


		return discrepancies;
	}


	// =========================================================
	// COMPARE ONE FIELD
	// =========================================================

	private static void Compare(
		List<FieldDiscrepancy> discrepancies,
		string fieldName,
		string? questionnaireValue,
		string? astValue)
	{
		var questionnaireNormalized =
			NormalizeComparisonValue(
				questionnaireValue);


		var astNormalized =
			NormalizeComparisonValue(
				astValue);


		if (
			string.IsNullOrWhiteSpace(
				questionnaireNormalized)
			&&
			string.IsNullOrWhiteSpace(
				astNormalized)
		)
		{
			return;
		}


		if (
			string.Equals(
				questionnaireNormalized,
				astNormalized,
				StringComparison.OrdinalIgnoreCase)
		)
		{
			return;
		}


		discrepancies.Add(
			new FieldDiscrepancy
			{
				FieldName =
					fieldName,

				QuestionnaireValue =
					DisplayValue(
						questionnaireValue),

				AstValue =
					DisplayValue(
						astValue)
			});
	}


	// =========================================================
	// FIELD ALERT DESCRIPTION
	// =========================================================

	private static string BuildFieldDiscrepancyDescription(
		string specimenId,
		long astRecordId,
		List<FieldDiscrepancy> discrepancies)
	{
		var lines =
			discrepancies.Select(
				x =>
					$"{x.FieldName}: Questionnaire='{x.QuestionnaireValue}', AST='{x.AstValue}'"
			);


		return
			$"Field-level discrepancies detected for specimen {specimenId} " +
			$"between Questionnaire and AST record #{astRecordId}. " +
			string.Join(
				" | ",
				lines);
	}


	// =========================================================
	// PRIORITY
	// =========================================================

	private static string DetermineFieldMismatchPriority(
		List<FieldDiscrepancy> discrepancies)
	{
		var highImpactFields =
			new HashSet<string>(
				StringComparer.OrdinalIgnoreCase)
			{
				"LAN",
				"Site Code",
				"Bacterial Identification",
				"Viral Identification",
				"Parasite Identification"
			};


		var containsHighImpactField =
			discrepancies.Any(
				x =>
					highImpactFields.Contains(
						x.FieldName));


		return containsHighImpactField
			? "High"
			: "Medium";
	}


	// =========================================================
	// NORMALIZATION
	// =========================================================

	private static string Normalize(
		string? value)
	{
		return
			string.IsNullOrWhiteSpace(value)
				? string.Empty
				: value
					.Trim()
					.ToUpperInvariant();
	}


	private static string NormalizeComparisonValue(
		string? value)
	{
		if (
			string.IsNullOrWhiteSpace(
				value)
		)
		{
			return string.Empty;
		}


		return value
			.Trim()
			.ToUpperInvariant();
	}


	private static string DisplayValue(
		string? value)
	{
		return
			string.IsNullOrWhiteSpace(value)
				? "(blank)"
				: value.Trim();
	}


	// =========================================================
	// ALERT KEY
	// =========================================================

	private static string BuildAlertKey(
		string specimenId,
		string? alertType)
	{
		return
			$"{Normalize(specimenId)}|{alertType ?? string.Empty}";
	}
}


// =============================================================
// FIELD DISCREPANCY
// =============================================================

public sealed class FieldDiscrepancy
{
	public string FieldName { get; init; } =
		string.Empty;


	public string QuestionnaireValue { get; init; } =
		string.Empty;


	public string AstValue { get; init; } =
		string.Empty;
}


// =============================================================
// RECONCILIATION RUN RESULT
// =============================================================

public sealed class ReconciliationRunResult
{
	public int AstRecordsChecked { get; init; }

	public int QuestionnaireSpecimensChecked { get; init; }

	public int AlertsCreated { get; init; }

	public int DuplicateAlertsSkipped { get; init; }

	public int FieldDiscrepanciesDetected { get; init; }

	public int SpecimensWithFieldDiscrepancies { get; init; }
}