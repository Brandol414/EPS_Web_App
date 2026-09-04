using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Reconciliation;

public class HistoryModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public HistoryModel(ApplicationDbContext context)
	{
		_context = context;
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


	public int TotalItems
	{
		get;
		private set;
	}


	// =========================================================
	// FILTERS
	// =========================================================

	public string? Search
	{
		get;
		private set;
	}

	public string? Priority
	{
		get;
		private set;
	}

	public string? AlertType
	{
		get;
		private set;
	}


	// =========================================================
	// HISTORY COUNTS
	// =========================================================

	public int ResolvedAlertCount
	{
		get;
		private set;
	}

	public int HighPriorityResolvedCount
	{
		get;
		private set;
	}


	// =========================================================
	// RESULTS
	// =========================================================

	public List<ResolvedAlertItem> Records
	{
		get;
		private set;
	} = [];


	// =========================================================
	// PAGE LOAD
	// =========================================================

	public async Task OnGetAsync(
		string? search,
		string? priority,
		string? alertType,
		int pageNumber = 1)
	{
		Search =
			string.IsNullOrWhiteSpace(search)
				? null
				: search.Trim();

		Priority =
			string.IsNullOrWhiteSpace(priority)
				? null
				: priority.Trim();

		AlertType =
			string.IsNullOrWhiteSpace(alertType)
				? null
				: alertType.Trim();

		PageNumber =
			pageNumber < 1
				? 1
				: pageNumber;


		// =====================================================
		// HISTORY COUNTS
		// =====================================================

		ResolvedAlertCount =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.CountAsync(
					x =>
						x.Status == "Resolved"
				);


		HighPriorityResolvedCount =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.CountAsync(
					x =>
						x.Status == "Resolved"
						&&
						(
							x.Priority == "High"
							||
							x.Priority == "Critical"
						)
				);


		// =====================================================
		// BASE QUERY
		// =====================================================

		IQueryable<ReconciliationAlert> query =
			_context.ReconciliationAlerts
				.AsNoTracking()
				.Where(
					x =>
						x.Status == "Resolved"
				);


		// =====================================================
		// SEARCH
		// =====================================================

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
							x.ResolutionNote != null
							&&
							x.ResolutionNote.Contains(
								searchValue
							)
						)
						||
						(
							x.ResolvedBy != null
							&&
							x.ResolvedBy.Contains(
								searchValue
							)
						)
				);
		}


		// =====================================================
		// PRIORITY
		// =====================================================

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
		// ALERT TYPE
		// =====================================================

		if (!string.IsNullOrWhiteSpace(
				AlertType))
		{
			query =
				query.Where(
					x =>
						x.AlertType == AlertType
				);
		}


		// =====================================================
		// FILTERED COUNT
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
		// RESULTS
		// =====================================================

		Records =
			await query
				.OrderByDescending(
					x =>
						x.ResolvedAt
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
						new ResolvedAlertItem
						{
							AlertId =
								x.AlertId,

							SpecimenId =
								x.SpecimenId,

							AlertType =
								x.AlertType,

							Priority =
								x.Priority,

							Description =
								x.Description,

							SourceRecord =
								x.SourceRecord,

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
	}


	// =========================================================
	// HISTORY RECORD
	// =========================================================

	public sealed class ResolvedAlertItem
	{
		public long AlertId { get; set; }

		public string? SpecimenId { get; set; }

		public string AlertType { get; set; } =
			string.Empty;

		public string Priority { get; set; } =
			string.Empty;

		public string? Description { get; set; }

		public string? SourceRecord { get; set; }

		public DateTime CreatedAt { get; set; }

		public string? ResolutionNote { get; set; }

		public DateTime? ResolvedAt { get; set; }

		public string? ResolvedBy { get; set; }
	}
}