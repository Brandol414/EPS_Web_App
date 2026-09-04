using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.AST;

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

	public const int PageSize = 20;

	[BindProperty(SupportsGet = true)]
	public int PageNumber { get; set; } = 1;


	// =========================================================
	// SEARCH / FILTERS
	// =========================================================

	[BindProperty(SupportsGet = true)]
	public string? Search { get; set; }

	[BindProperty(SupportsGet = true)]
	public string? Lan { get; set; }

	[BindProperty(SupportsGet = true)]
	public string? SiteCode { get; set; }

	[BindProperty(SupportsGet = true)]
	public string? LinkageStatus { get; set; }


	// =========================================================
	// RESULTS
	// =========================================================

	public List<AstListItem> Records { get; private set; } = [];

	public List<string> AvailableSites { get; private set; } = [];

	public int TotalRecords { get; private set; }

	public int TotalPages =>
		TotalRecords == 0
			? 0
			: (int)Math.Ceiling(
				TotalRecords /
				(double)PageSize
			);


	// =========================================================
	// SUMMARY COUNTS
	// =========================================================

	public int LinkedCount { get; private set; }

	public int UnlinkedCount { get; private set; }


	// =========================================================
	// PAGE LOAD
	// =========================================================

	public async Task OnGetAsync()
	{
		if (PageNumber < 1)
		{
			PageNumber = 1;
		}

		// -----------------------------------------------------
		// AVAILABLE SITES
		// -----------------------------------------------------

		AvailableSites =
			await _context.AstRecords
				.AsNoTracking()
				.Where(
					x =>
						x.SiteCode != null &&
						x.SiteCode != ""
				)
				.Select(
					x => x.SiteCode!
				)
				.Distinct()
				.OrderBy(
					x => x
				)
				.ToListAsync();


		// -----------------------------------------------------
		// SUMMARY COUNTS
		// -----------------------------------------------------

		LinkedCount =
			await _context.AstRecords
				.AsNoTracking()
				.CountAsync(
					x =>
						x.LinkageStatus == "Linked"
				);

		UnlinkedCount =
			await _context.AstRecords
				.AsNoTracking()
				.CountAsync(
					x =>
						x.LinkageStatus == "Unlinked"
				);


		// -----------------------------------------------------
		// BASE QUERY
		// -----------------------------------------------------

		IQueryable<AstRecord> query =
			_context.AstRecords
				.AsNoTracking();


		// -----------------------------------------------------
		// GENERAL SEARCH
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(Search))
		{
			var search =
				Search.Trim();

			query =
				query.Where(
					x =>
						(x.SpecimenId != null &&
						 x.SpecimenId.Contains(search))

						||

						(x.Lan != null &&
						 x.Lan.Contains(search))

						||

						(x.SiteCode != null &&
						 x.SiteCode.Contains(search))

						||

						(x.BacterialIdentification != null &&
						 x.BacterialIdentification.Contains(search))

						||

						(x.ViralIdentification != null &&
						 x.ViralIdentification.Contains(search))

						||

						(x.ParasiteIdentification != null &&
						 x.ParasiteIdentification.Contains(search))
				);
		}


		// -----------------------------------------------------
		// LAN FILTER
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(Lan))
		{
			var lan =
				Lan.Trim();

			query =
				query.Where(
					x =>
						x.Lan != null &&
						x.Lan.Contains(lan)
				);
		}


		// -----------------------------------------------------
		// SITE FILTER
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(SiteCode))
		{
			var site =
				SiteCode.Trim();

			query =
				query.Where(
					x =>
						x.SiteCode == site
				);
		}


		// -----------------------------------------------------
		// LINKAGE FILTER
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(
				LinkageStatus))
		{
			var linkage =
				LinkageStatus.Trim();

			query =
				query.Where(
					x =>
						x.LinkageStatus == linkage
				);
		}


		// -----------------------------------------------------
		// TOTAL RECORD COUNT
		// -----------------------------------------------------

		TotalRecords =
			await query.CountAsync();


		// -----------------------------------------------------
		// PAGE SAFETY
		// -----------------------------------------------------

		if (
			TotalPages > 0 &&
			PageNumber > TotalPages
		)
		{
			PageNumber =
				TotalPages;
		}


		// -----------------------------------------------------
		// PAGED RESULTS
		// -----------------------------------------------------

		Records =
			await query
				.OrderByDescending(
					x => x.AstRecordId
				)
				.Skip(
					(PageNumber - 1) *
					PageSize
				)
				.Take(
					PageSize
				)
				.Select(
					x =>
						new AstListItem
						{
							AstRecordId =
								x.AstRecordId,

							SpecimenId =
								x.SpecimenId,

							Lan =
								x.Lan,

							SiteCode =
								x.SiteCode,

							MonthCollected =
								x.MonthCollected,

							ParticipantType =
								x.ParticipantType,

							BacterialIdentification =
								x.BacterialIdentification,

							ViralIdentification =
								x.ViralIdentification,

							ParasiteIdentification =
								x.ParasiteIdentification,

							MdrStatus =
								x.MdrStatus,

							EsblStatus =
								x.EsblStatus,

							LinkageStatus =
								x.LinkageStatus,

							CreatedAt =
								x.CreatedAt
						}
				)
				.ToListAsync();
	}


	// =========================================================
	// AST LIST VIEW MODEL
	// =========================================================

	public sealed class AstListItem
	{
		public long AstRecordId { get; set; }

		public string? SpecimenId { get; set; }

		public string? Lan { get; set; }

		public string? SiteCode { get; set; }

		public string? MonthCollected { get; set; }

		public string? ParticipantType { get; set; }

		public string? BacterialIdentification { get; set; }

		public string? ViralIdentification { get; set; }

		public string? ParasiteIdentification { get; set; }

		public string? MdrStatus { get; set; }

		public string? EsblStatus { get; set; }

		public string LinkageStatus { get; set; } = "Unlinked";

		public DateTime CreatedAt { get; set; }
	}
}