using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Reconciliation;

[Authorize(Policy = "AdministratorOnly")]
public class ViewModel : PageModel
{
	private readonly ApplicationDbContext _context;


	public ViewModel(
		ApplicationDbContext context)
	{
		_context = context;
	}


	// =========================================================
	// ROUTE / QUERY PARAMETERS
	// =========================================================

	[BindProperty(SupportsGet = true)]
	public long? Id { get; set; }


	[BindProperty(SupportsGet = true)]
	public long? AstId { get; set; }


	// =========================================================
	// ALERT
	// =========================================================

	public ReconciliationAlert? Alert
	{
		get;
		private set;
	}


	// =========================================================
	// AST EVIDENCE
	// =========================================================

	public List<AstRecord> AstRecords
	{
		get;
		private set;
	} = [];


	// =========================================================
	// QUESTIONNAIRE EVIDENCE
	// =========================================================

	public QuestionnaireEntry? QuestionnaireRecord
	{
		get;
		private set;
	}


	// =========================================================
	// FIELD DIFFERENCES
	// =========================================================

	public List<FieldDifference> Differences
	{
		get;
		private set;
	} = [];


	// =========================================================
	// RECONCILIATION AUDIT HISTORY
	// =========================================================

	public List<DataEntryAudit> AuditHistory
	{
		get;
		private set;
	} = [];


	// =========================================================
	// RESOLUTION FORM
	// =========================================================

	[BindProperty]
	public string? ResolutionAction { get; set; }


	[BindProperty]
	public string? ResolutionNote { get; set; }


	// =========================================================
	// STATUS
	// =========================================================

	public bool IsResolved =>
		string.Equals(
			Alert?.Status,
			"Resolved",
			StringComparison.OrdinalIgnoreCase
		);


	public bool IsNotAnError =>
		string.Equals(
			Alert?.Status,
			"Not an Error",
			StringComparison.OrdinalIgnoreCase
		);


	public bool IsClosed =>
		IsResolved ||
		IsNotAnError;


	public bool HasDifferences =>
		Differences.Count > 0;


	// =========================================================
	// GET
	// =========================================================

	public async Task<IActionResult> OnGetAsync(
		CancellationToken cancellationToken = default)
	{
		if (Id.HasValue)
		{
			return await LoadAlertAsync(
				Id.Value,
				cancellationToken
			);
		}


		if (AstId.HasValue)
		{
			return await LoadAstCandidateAsync(
				AstId.Value,
				cancellationToken
			);
		}


		return NotFound();
	}


	// =========================================================
	// LOAD ALERT
	// =========================================================

	private async Task<IActionResult> LoadAlertAsync(
		long alertId,
		CancellationToken cancellationToken)
	{
		Alert =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.FirstOrDefaultAsync(
					x =>
						x.AlertId ==
						alertId,
					cancellationToken
				);


		if (Alert == null)
		{
			return NotFound();
		}


		if (!string.IsNullOrWhiteSpace(
				Alert.SpecimenId))
		{
			await LoadEvidenceAsync(
				Alert.SpecimenId,
				cancellationToken
			);
		}


		await LoadAuditHistoryAsync(
			Alert.AlertId,
			cancellationToken
		);


		BuildDifferences();


		return Page();
	}


	// =========================================================
	// LOAD AST CANDIDATE
	// =========================================================

	private async Task<IActionResult> LoadAstCandidateAsync(
		long astId,
		CancellationToken cancellationToken)
	{
		var ast =
			await _context.AstRecords
				.AsNoTracking()
				.FirstOrDefaultAsync(
					x =>
						x.AstRecordId ==
						astId,
					cancellationToken
				);


		if (ast == null)
		{
			return NotFound();
		}


		AstRecords =
			[ast];


		if (!string.IsNullOrWhiteSpace(
				ast.SpecimenId))
		{
			await LoadEvidenceAsync(
				ast.SpecimenId,
				cancellationToken
			);


			Alert =
				await _context.ReconciliationAlerts
					.AsNoTracking()
					.Where(
						x =>
							x.SpecimenId ==
							ast.SpecimenId
					)
					.OrderByDescending(
						x =>
							x.CreatedAt
					)
					.FirstOrDefaultAsync(
						cancellationToken
					);


			if (Alert != null)
			{
				await LoadAuditHistoryAsync(
					Alert.AlertId,
					cancellationToken
				);
			}
		}


		BuildDifferences();


		return Page();
	}


	// =========================================================
	// LOAD QUESTIONNAIRE + AST EVIDENCE
	// =========================================================

	private async Task LoadEvidenceAsync(
		string specimenId,
		CancellationToken cancellationToken)
	{
		QuestionnaireRecord =
			await _context.QuestionnaireEntries
				.AsNoTracking()
				.FirstOrDefaultAsync(
					x =>
						x.SpecimenId ==
						specimenId,
					cancellationToken
				);


		AstRecords =
			await _context.AstRecords
				.AsNoTracking()
				.Where(
					x =>
						x.SpecimenId ==
						specimenId
				)
				.OrderByDescending(
					x =>
						x.AstRecordId
				)
				.ToListAsync(
					cancellationToken
				);
	}


	// =========================================================
	// LOAD AUDIT HISTORY
	// =========================================================

	private async Task LoadAuditHistoryAsync(
		long alertId,
		CancellationToken cancellationToken)
	{
		AuditHistory =
			await _context.DataEntryAudits
				.AsNoTracking()
				.Where(
					x =>
						x.RecordType ==
						"Reconciliation"
						&&
						x.RecordKey ==
						alertId.ToString()
				)
				.OrderByDescending(
					x =>
						x.ChangedAt
				)
				.ThenByDescending(
					x =>
						x.AuditId
				)
				.ToListAsync(
					cancellationToken
				);
	}


	// =========================================================
	// BUILD SIDE-BY-SIDE DIFFERENCES
	// =========================================================

	private void BuildDifferences()
	{
		Differences =
			[];


		if (QuestionnaireRecord == null)
		{
			return;
		}


		var ast =
			AstRecords.FirstOrDefault();


		if (ast == null)
		{
			return;
		}


		AddDifference(
			"LAN",
			QuestionnaireRecord.Lan,
			ast.Lan
		);


		AddDifference(
			"Site Code",
			QuestionnaireRecord.SiteCode,
			ast.SiteCode
		);


		AddDifference(
			"Bacterial Identification",
			QuestionnaireRecord.BacterialIdentification,
			ast.BacterialIdentification
		);


		AddDifference(
			"Viral Identification",
			QuestionnaireRecord.ViralIdentification,
			ast.ViralIdentification
		);


		AddDifference(
			"Parasite Identification",
			QuestionnaireRecord.ParasiteIdentification,
			ast.ParasiteIdentification
		);


		AddDifference(
			"Illness Functional Impact",
			QuestionnaireRecord.IllnessFunctionalImpact,
			ast.IllnessFunctionalImpact
		);
	}


	// =========================================================
	// ADD ONE DIFFERENCE
	// =========================================================

	private void AddDifference(
		string fieldName,
		string? questionnaireValue,
		string? astValue)
	{
		var questionnaireNormalized =
			Normalize(
				questionnaireValue
			);


		var astNormalized =
			Normalize(
				astValue
			);


		if (
			string.Equals(
				questionnaireNormalized,
				astNormalized,
				StringComparison.OrdinalIgnoreCase)
		)
		{
			return;
		}


		Differences.Add(
			new FieldDifference
			{
				FieldName =
					fieldName,

				QuestionnaireValue =
					DisplayValue(
						questionnaireValue
					),

				AstValue =
					DisplayValue(
						astValue
					)
			}
		);
	}


	// =========================================================
	// ADMINISTRATIVE RESOLUTION
	// =========================================================

	public async Task<IActionResult> OnPostResolutionAsync(
		CancellationToken cancellationToken = default)
	{
		if (!Id.HasValue)
		{
			return NotFound();
		}


		var alert =
			await _context.ReconciliationAlerts
				.FirstOrDefaultAsync(
					x =>
						x.AlertId ==
						Id.Value,
					cancellationToken
				);


		if (alert == null)
		{
			return NotFound();
		}


		// -----------------------------------------------------
		// VALIDATE ACTION
		// -----------------------------------------------------

		var action =
			NormalizeResolutionAction(
				ResolutionAction
			);


		if (action == null)
		{
			ModelState.AddModelError(
				nameof(ResolutionAction),
				"Select a valid administrative action."
			);
		}


		// -----------------------------------------------------
		// REQUIRED NOTE
		// -----------------------------------------------------

		if (
			string.IsNullOrWhiteSpace(
				ResolutionNote)
		)
		{
			ModelState.AddModelError(
				nameof(ResolutionNote),
				"A resolution or review note is required."
			);
		}


		// -----------------------------------------------------
		// RETURN PAGE WHEN VALIDATION FAILS
		// -----------------------------------------------------

		if (!ModelState.IsValid)
		{
			Alert =
				alert;


			if (!string.IsNullOrWhiteSpace(
					alert.SpecimenId))
			{
				await LoadEvidenceAsync(
					alert.SpecimenId,
					cancellationToken
				);
			}


			await LoadAuditHistoryAsync(
				alert.AlertId,
				cancellationToken
			);


			BuildDifferences();


			return Page();
		}


		// -----------------------------------------------------
		// ADMINISTRATOR IDENTITY
		// -----------------------------------------------------

		var changedBy =
			User.Identity?.Name
			??
			"Unknown Administrator";


		var changedAt =
			DateTime.UtcNow;


		var oldStatus =
			alert.Status;


		var newStatus =
			action!;


		var reason =
			ResolutionNote!
				.Trim();


		// -----------------------------------------------------
		// NOTHING CHANGED
		// -----------------------------------------------------

		if (
			string.Equals(
				oldStatus,
				newStatus,
				StringComparison.OrdinalIgnoreCase)
		)
		{
			ModelState.AddModelError(
				nameof(ResolutionAction),
				$"The alert is already '{oldStatus}'."
			);


			Alert =
				alert;


			if (!string.IsNullOrWhiteSpace(
					alert.SpecimenId))
			{
				await LoadEvidenceAsync(
					alert.SpecimenId,
					cancellationToken
				);
			}


			await LoadAuditHistoryAsync(
				alert.AlertId,
				cancellationToken
			);


			BuildDifferences();


			return Page();
		}


		// =====================================================
		// TRANSACTION
		// =====================================================

		await using var transaction =
			await _context.Database
				.BeginTransactionAsync(
					cancellationToken
				);


		try
		{
			// -------------------------------------------------
			// UPDATE ALERT ONLY
			//
			// This action does NOT silently modify Questionnaire
			// or AST source data.
			// -------------------------------------------------

			alert.Status =
				newStatus;


			alert.ResolutionNote =
				reason;


			// -------------------------------------------------
			// FINAL STATUS METADATA
			// -------------------------------------------------

			if (
				string.Equals(
					newStatus,
					"Resolved",
					StringComparison.OrdinalIgnoreCase)
				||
				string.Equals(
					newStatus,
					"Not an Error",
					StringComparison.OrdinalIgnoreCase)
				||
				string.Equals(
					newStatus,
					"Deferred",
					StringComparison.OrdinalIgnoreCase)
			)
			{
				alert.ResolvedBy =
					changedBy;

				alert.ResolvedAt =
					changedAt;
			}
			else
			{
				alert.ResolvedBy =
					null;

				alert.ResolvedAt =
					null;
			}


			// -------------------------------------------------
			// AUDIT STATUS CHANGE
			// -------------------------------------------------

			var audit =
				new DataEntryAudit
				{
					RecordType =
						"Reconciliation",

					RecordKey =
						alert.AlertId.ToString(),

					FieldName =
						"Status",

					OldValue =
						oldStatus,

					NewValue =
						newStatus,

					ChangedBy =
						changedBy,

					ChangedAt =
						changedAt,

					Reason =
						reason
				};


			_context.DataEntryAudits.Add(
				audit
			);


			await _context.SaveChangesAsync(
				cancellationToken
			);


			await transaction.CommitAsync(
				cancellationToken
			);
		}
		catch
		{
			await transaction.RollbackAsync(
				cancellationToken
			);

			throw;
		}


		return RedirectToPage(
			"./View",
			new
			{
				id =
					alert.AlertId
			}
		);
	}


	// =========================================================
	// RESOLUTION ACTION NORMALIZATION
	// =========================================================

	private static string? NormalizeResolutionAction(
		string? action)
	{
		if (
			string.IsNullOrWhiteSpace(
				action)
		)
		{
			return null;
		}


		return action
			.Trim()
			.ToUpperInvariant() switch
		{
			"UNDER REVIEW" =>
				"Under Review",

			"RESOLVED" =>
				"Resolved",

			"DEFERRED" =>
				"Deferred",

			"NOT AN ERROR" =>
				"Not an Error",

			_ =>
				null
		};
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


	private static string DisplayValue(
		string? value)
	{
		return
			string.IsNullOrWhiteSpace(value)
				? "(blank)"
				: value.Trim();
	}


	// =========================================================
	// FIELD DIFFERENCE MODEL
	// =========================================================

	public sealed class FieldDifference
	{
		public string FieldName { get; set; } =
			string.Empty;


		public string QuestionnaireValue { get; set; } =
			string.Empty;


		public string AstValue { get; set; } =
			string.Empty;
	}
}