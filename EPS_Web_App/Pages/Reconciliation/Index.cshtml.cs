using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using EPS_Web_App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Reconciliation;

[Authorize(Policy = "AdministratorOnly")]
public class IndexModel : PageModel
{
	private readonly ApplicationDbContext _context;

	private readonly ReconciliationService
		_reconciliationService;


	public IndexModel(
		ApplicationDbContext context,
		ReconciliationService reconciliationService)
	{
		_context =
			context;

		_reconciliationService =
			reconciliationService;
	}


	// =========================================================
	// PAGINATION
	// =========================================================

	public const int PageSize = 20;

	public int PageNumber { get; private set; } = 1;

	public int TotalPages =>
		TotalItems == 0
			? 0
			: (int)Math.Ceiling(
				TotalItems /
				(double)PageSize
			);


	// =========================================================
	// FILTERS
	// =========================================================

	public string? Search { get; private set; }

	public string? Status { get; private set; }

	public string? Priority { get; private set; }


	// =========================================================
	// ALERT COUNTS
	// =========================================================

	public int OpenAlertCount { get; private set; }

	public int UnderReviewAlertCount { get; private set; }

	public int DeferredAlertCount { get; private set; }

	public int NotAnErrorAlertCount { get; private set; }

	public int HighPriorityAlertCount { get; private set; }

	public int ResolvedAlertCount { get; private set; }

	public int TotalAlertCount { get; private set; }


	// =========================================================
	// AST RECONCILIATION COUNTS
	// =========================================================

	public int TotalAstCount { get; private set; }

	public int LinkedAstCount { get; private set; }

	public int UnlinkedAstCount { get; private set; }

	public int UnlinkedWithQuestionnaireCount
	{
		get;
		private set;
	}


	// =========================================================
	// RESULTS
	// =========================================================

	public List<ReconciliationListItem> Alerts
	{
		get;
		private set;
	} = [];


	public List<UnlinkedAstItem> UnlinkedAstRecords
	{
		get;
		private set;
	} = [];


	public int TotalItems
	{
		get;
		private set;
	}


	// =========================================================
	// RUN MESSAGE
	// =========================================================

	public string? RunMessage
	{
		get;
		private set;
	}


	// =========================================================
	// PAGE LOAD
	// =========================================================

	public async Task<IActionResult> OnGetAsync(
		string? search,
		string? status,
		string? priority,
		int pageNumber = 1)
	{
		Search =
			string.IsNullOrWhiteSpace(search)
				? null
				: search.Trim();


		Status =
			string.IsNullOrWhiteSpace(status)
				? null
				: status.Trim();


		Priority =
			string.IsNullOrWhiteSpace(priority)
				? null
				: priority.Trim();


		PageNumber =
			pageNumber < 1
				? 1
				: pageNumber;


		// -----------------------------------------------------
		// TEMPORARY RUN MESSAGE
		// -----------------------------------------------------

		RunMessage =
			TempData["ReconciliationMessage"]
				?.ToString();


		// =====================================================
		// ALERT COUNTS
		// =====================================================

		TotalAlertCount =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.CountAsync();


		OpenAlertCount =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.CountAsync(
					x =>
						x.Status == "Open"
				);


		UnderReviewAlertCount =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.CountAsync(
					x =>
						x.Status == "Under Review"
				);


		DeferredAlertCount =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.CountAsync(
					x =>
						x.Status == "Deferred"
				);


		NotAnErrorAlertCount =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.CountAsync(
					x =>
						x.Status == "Not an Error"
				);


		HighPriorityAlertCount =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.CountAsync(
					x =>
						x.Status == "Open"
						&&
						(
							x.Priority == "High"
							||
							x.Priority == "Critical"
						)
				);


		ResolvedAlertCount =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.CountAsync(
					x =>
						x.Status == "Resolved"
				);


		// =====================================================
		// AST LINKAGE COUNTS
		// =====================================================

		TotalAstCount =
			await _context.AstRecords
				.AsNoTracking()
				.CountAsync();


		LinkedAstCount =
			await _context.AstRecords
				.AsNoTracking()
				.CountAsync(
					x =>
						x.LinkageStatus == "Linked"
				);


		UnlinkedAstCount =
			await _context.AstRecords
				.AsNoTracking()
				.CountAsync(
					x =>
						x.LinkageStatus != "Linked"
				);


		UnlinkedWithQuestionnaireCount =
			await (
				from ast
					in _context.AstRecords.AsNoTracking()

				join questionnaire
					in _context.QuestionnaireEntries
						.AsNoTracking()

					on ast.SpecimenId
					equals questionnaire.SpecimenId

				where
					ast.LinkageStatus != "Linked"
					&&
					ast.SpecimenId != null

				select ast.AstRecordId
			)
			.CountAsync();


		// =====================================================
		// ALERT QUERY
		// =====================================================

		IQueryable<ReconciliationAlert> query =
			_context.ReconciliationAlerts
				.AsNoTracking();


		// -----------------------------------------------------
		// SEARCH
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(
				Search))
		{
			var searchValue =
				Search.Trim();


			query =
				query.Where(
					x =>
						(
							x.SpecimenId != null
							&&
							x.SpecimenId.Contains(
								searchValue
							)
						)
						||
						(
							x.AlertType != null
							&&
							x.AlertType.Contains(
								searchValue
							)
						)
						||
						(
							x.Description != null
							&&
							x.Description.Contains(
								searchValue
							)
						)
						||
						(
							x.SourceRecord != null
							&&
							x.SourceRecord.Contains(
								searchValue
							)
						)
				);
		}


		// -----------------------------------------------------
		// STATUS
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(
				Status))
		{
			query =
				query.Where(
					x =>
						x.Status == Status
				);
		}


		// -----------------------------------------------------
		// PRIORITY
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(
				Priority))
		{
			query =
				query.Where(
					x =>
						x.Priority == Priority
				);
		}


		// =====================================================
		// TOTAL FILTERED ALERTS
		// =====================================================

		TotalItems =
			await query.CountAsync();


		// =====================================================
		// PAGE SAFETY
		// =====================================================

		if (
			TotalPages > 0
			&&
			PageNumber > TotalPages
		)
		{
			PageNumber =
				TotalPages;
		}


		// =====================================================
		// ALERT RESULTS
		// =====================================================

		Alerts =
			await query
				.OrderBy(
					x =>
						x.Status == "Open"
							? 0
							:
							x.Status == "Under Review"
								? 1
								:
								x.Status == "Deferred"
									? 2
									: 3
				)
				.ThenBy(
					x =>
						x.Priority == "Critical"
							? 0
							:
							x.Priority == "High"
								? 1
								:
								x.Priority == "Medium"
									? 2
									: 3
				)
				.ThenByDescending(
					x =>
						x.CreatedAt
				)
				.Skip(
					(PageNumber - 1)
					*
					PageSize
				)
				.Take(
					PageSize
				)
				.Select(
					x =>
						new ReconciliationListItem
						{
							AlertId =
								x.AlertId,

							AlertType =
								x.AlertType,

							SpecimenId =
								x.SpecimenId,

							SourceRecord =
								x.SourceRecord,

							Priority =
								x.Priority,

							Status =
								x.Status,

							Description =
								x.Description,

							CreatedAt =
								x.CreatedAt,

							ResolutionNote =
								x.ResolutionNote,

							ResolvedAt =
								x.ResolvedAt,

							ResolvedBy =
								x.ResolvedBy
						}
				)
				.ToListAsync();


		// =====================================================
		// UNLINKED AST CANDIDATES
		// =====================================================

		UnlinkedAstRecords =
			await (
				from ast
					in _context.AstRecords.AsNoTracking()

				join questionnaire
					in _context.QuestionnaireEntries
						.AsNoTracking()

					on ast.SpecimenId
					equals questionnaire.SpecimenId

					into questionnaireGroup

				from questionnaire
					in questionnaireGroup
						.DefaultIfEmpty()

				where
					ast.LinkageStatus != "Linked"

				orderby
					questionnaire != null
						? 0
						: 1,

					ast.AstRecordId descending

				select new UnlinkedAstItem
				{
					AstRecordId =
						ast.AstRecordId,

					SpecimenId =
						ast.SpecimenId,

					Lan =
						ast.Lan,

					SiteCode =
						ast.SiteCode,

					BacterialIdentification =
						ast.BacterialIdentification,

					ViralIdentification =
						ast.ViralIdentification,

					ParasiteIdentification =
						ast.ParasiteIdentification,

					LinkageStatus =
						ast.LinkageStatus,

					HasQuestionnaireMatch =
						questionnaire != null
				}
			)
			.Take(10)
			.ToListAsync();


		return Page();
	}


	// =========================================================
	// RUN RECONCILIATION
	// =========================================================

	public async Task<IActionResult> OnPostRunAsync()
	{
		var result =
			await _reconciliationService
				.RunAsync();


		TempData["ReconciliationMessage"] =
			$"Reconciliation completed. " +
			$"{result.AstRecordsChecked:N0} AST records checked; " +
			$"{result.AlertsCreated:N0} new alert(s) created; " +
			$"{result.DuplicateAlertsSkipped:N0} duplicate alert(s) skipped.";


		return RedirectToPage(
			"./Index"
		);
	}


	// =========================================================
	// ALERT LIST ITEM
	// =========================================================

	public sealed class ReconciliationListItem
	{
		public long AlertId { get; set; }

		public string AlertType { get; set; } =
			string.Empty;

		public string? SpecimenId { get; set; }

		public string? SourceRecord { get; set; }

		public string Priority { get; set; } =
			string.Empty;

		public string Status { get; set; } =
			string.Empty;

		public string? Description { get; set; }

		public DateTime CreatedAt { get; set; }

		public string? ResolutionNote { get; set; }

		public DateTime? ResolvedAt { get; set; }

		public string? ResolvedBy { get; set; }
	}


	// =========================================================
	// UNLINKED AST ITEM
	// =========================================================

	public sealed class UnlinkedAstItem
	{
		public long AstRecordId { get; set; }

		public string? SpecimenId { get; set; }

		public string? Lan { get; set; }

		public string? SiteCode { get; set; }

		public string? BacterialIdentification { get; set; }

		public string? ViralIdentification { get; set; }

		public string? ParasiteIdentification { get; set; }

		public string LinkageStatus { get; set; } =
			string.Empty;

		public bool HasQuestionnaireMatch { get; set; }
	}
}