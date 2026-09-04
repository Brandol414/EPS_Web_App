using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Administration.Lookups;

public class IndexModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public IndexModel(ApplicationDbContext context)
	{
		_context = context;
	}


	// =========================================================
	// PAGINATION
	// =========================================================

	public const int PageSize = 25;

	public int PageNumber { get; private set; } = 1;

	public int TotalItems { get; private set; }

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

	public string? LookupGroup { get; private set; }

	public string? Status { get; private set; }


	// =========================================================
	// DATA
	// =========================================================

	public List<LookupItem> Records
	{
		get;
		private set;
	} = [];


	public List<string> AvailableGroups
	{
		get;
		private set;
	} = [];


	// =========================================================
	// PAGE LOAD
	// =========================================================

	public async Task OnGetAsync(
		string? search,
		string? lookupGroup,
		string? status,
		int pageNumber = 1)
	{
		Search =
			string.IsNullOrWhiteSpace(search)
				? null
				: search.Trim();

		LookupGroup =
			string.IsNullOrWhiteSpace(lookupGroup)
				? null
				: lookupGroup.Trim();

		Status =
			string.IsNullOrWhiteSpace(status)
				? null
				: status.Trim();

		PageNumber =
			pageNumber < 1
				? 1
				: pageNumber;


		// =====================================================
		// AVAILABLE GROUPS
		// =====================================================

		AvailableGroups =
			await _context.LookupValues
				.AsNoTracking()
				.Select(
					x =>
						x.LookupGroup
				)
				.Distinct()
				.OrderBy(
					x =>
						x
				)
				.ToListAsync();


		// =====================================================
		// BASE QUERY
		// =====================================================

		IQueryable<LookupValue> query =
			_context.LookupValues
				.AsNoTracking();


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
						x.LookupGroup.Contains(
							searchValue
						)
						||
						x.LookupCode.Contains(
							searchValue
						)
						||
						x.LookupLabel.Contains(
							searchValue
						)
				);
		}


		// =====================================================
		// GROUP
		// =====================================================

		if (!string.IsNullOrWhiteSpace(
				LookupGroup))
		{
			query =
				query.Where(
					x =>
						x.LookupGroup ==
						LookupGroup
				);
		}


		// =====================================================
		// STATUS
		// =====================================================

		if (
			string.Equals(
				Status,
				"Active",
				StringComparison.OrdinalIgnoreCase)
		)
		{
			query =
				query.Where(
					x =>
						x.IsActive
				);
		}
		else if (
			string.Equals(
				Status,
				"Inactive",
				StringComparison.OrdinalIgnoreCase)
		)
		{
			query =
				query.Where(
					x =>
						!x.IsActive
				);
		}


		// =====================================================
		// COUNT
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
				.OrderBy(
					x =>
						x.LookupGroup
				)
				.ThenBy(
					x =>
						x.DisplayOrder ??
						int.MaxValue
				)
				.ThenBy(
					x =>
						x.LookupLabel
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
						new LookupItem
						{
							LookupId =
								x.LookupId,

							LookupGroup =
								x.LookupGroup,

							LookupCode =
								x.LookupCode,

							LookupLabel =
								x.LookupLabel,

							DisplayOrder =
								x.DisplayOrder,

							IsActive =
								x.IsActive
						}
				)
				.ToListAsync();
	}


	// =========================================================
	// ACTIVATE
	// =========================================================

	public async Task<IActionResult> OnPostActivateAsync(
		long id)
	{
		var lookup =
			await _context.LookupValues
				.FirstOrDefaultAsync(
					x =>
						x.LookupId ==
						id
				);


		if (lookup == null)
		{
			return NotFound();
		}


		lookup.IsActive =
			true;


		await _context.SaveChangesAsync();


		return RedirectToPage(
			"./Index",
			new
			{
				search = Search,
				lookupGroup = LookupGroup,
				status = Status,
				pageNumber = PageNumber
			}
		);
	}


	// =========================================================
	// DEACTIVATE
	// =========================================================

	public async Task<IActionResult> OnPostDeactivateAsync(
		long id)
	{
		var lookup =
			await _context.LookupValues
				.FirstOrDefaultAsync(
					x =>
						x.LookupId ==
						id
				);


		if (lookup == null)
		{
			return NotFound();
		}


		lookup.IsActive =
			false;


		await _context.SaveChangesAsync();


		return RedirectToPage(
			"./Index",
			new
			{
				search = Search,
				lookupGroup = LookupGroup,
				status = Status,
				pageNumber = PageNumber
			}
		);
	}


	// =========================================================
	// LIST ITEM
	// =========================================================

	public sealed class LookupItem
	{
		public long LookupId { get; set; }

		public string LookupGroup { get; set; } =
			string.Empty;

		public string LookupCode { get; set; } =
			string.Empty;

		public string LookupLabel { get; set; } =
			string.Empty;

		public int? DisplayOrder { get; set; }

		public bool IsActive { get; set; }
	}
}