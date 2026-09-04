using EPS_Web_App.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Questionnaire;

public class IndexModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public IndexModel(ApplicationDbContext context)
	{
		_context = context;
	}


	// =========================================================
	// SEARCH FILTERS
	// =========================================================

	[BindProperty(SupportsGet = true)]
	public string? Search { get; set; }

	[BindProperty(SupportsGet = true)]
	public string? SiteCode { get; set; }

	[BindProperty(SupportsGet = true)]
	public string? Lan { get; set; }

	[BindProperty(SupportsGet = true)]
	public string? Pathogen { get; set; }


	// =========================================================
	// PAGINATION
	// =========================================================

	[BindProperty(SupportsGet = true)]
	public int PageNumber { get; set; } = 1;

	public const int PageSize = 20;

	public int TotalRecords { get; private set; }

	public int TotalPages { get; private set; }


	// =========================================================
	// RECORDS
	// =========================================================

	public List<QuestionnaireRow> Records { get; private set; } = [];


	// =========================================================
	// SITE LIST
	// =========================================================

	public List<string> AvailableSites { get; private set; } = [];


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
		// Site list
		// -----------------------------------------------------

		AvailableSites =
			await _context.QuestionnaireEntries
				.AsNoTracking()
				.Where(x => x.SiteCode != null)
				.Select(x => x.SiteCode!)
				.Distinct()
				.OrderBy(x => x)
				.ToListAsync();


		// -----------------------------------------------------
		// Base query
		// -----------------------------------------------------

		IQueryable<EPS_Web_App.Data.Models.QuestionnaireEntry> query =
			_context.QuestionnaireEntries
				.AsNoTracking();


		// -----------------------------------------------------
		// General search
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(Search))
		{
			var term = Search.Trim();

			query =
				query.Where(
					x =>
						x.SpecimenId.Contains(term)
						||
						(x.Lan != null &&
						 x.Lan.Contains(term))
						||
						(x.SiteCode != null &&
						 x.SiteCode.Contains(term))
						||
						(x.BacterialIdentification != null &&
						 x.BacterialIdentification.Contains(term))
						||
						(x.ViralIdentification != null &&
						 x.ViralIdentification.Contains(term))
						||
						(x.ParasiteIdentification != null &&
						 x.ParasiteIdentification.Contains(term))
				);
		}


		// -----------------------------------------------------
		// Site filter
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(SiteCode))
		{
			var selectedSite = SiteCode.Trim();

			query =
				query.Where(
					x =>
						x.SiteCode == selectedSite
				);
		}


		// -----------------------------------------------------
		// LAN filter
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(Lan))
		{
			var selectedLan = Lan.Trim();

			query =
				query.Where(
					x =>
						x.Lan != null &&
						x.Lan.Contains(selectedLan)
				);
		}


		// -----------------------------------------------------
		// Pathogen filter
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(Pathogen))
		{
			var selectedPathogen = Pathogen.Trim();

			query =
				query.Where(
					x =>
						(x.BacterialIdentification != null &&
						 x.BacterialIdentification.Contains(selectedPathogen))
						||
						(x.ViralIdentification != null &&
						 x.ViralIdentification.Contains(selectedPathogen))
						||
						(x.ParasiteIdentification != null &&
						 x.ParasiteIdentification.Contains(selectedPathogen))
				);
		}


		// -----------------------------------------------------
		// Total count
		// -----------------------------------------------------

		TotalRecords =
			await query.CountAsync();

		TotalPages =
			(int)Math.Ceiling(
				TotalRecords /
				(double)PageSize
			);


		if (TotalPages > 0 &&
			PageNumber > TotalPages)
		{
			PageNumber = TotalPages;
		}


		// -----------------------------------------------------
		// Paginated records
		// -----------------------------------------------------

		Records =
			await query

				.OrderByDescending(
					x => x.QuestionnaireId
				)

				.Skip(
					(PageNumber - 1) *
					PageSize
				)

				.Take(
					PageSize
				)

				.Select(
					x => new QuestionnaireRow
					{
						QuestionnaireId =
							x.QuestionnaireId,

						SpecimenId =
							x.SpecimenId,

						Lan =
							x.Lan,

						SiteCode =
							x.SiteCode,

						IsolationDate =
							x.IsolationDate,

						Age =
							x.Age,

						Gender =
							x.Gender,

						BacterialIdentification =
							x.BacterialIdentification,

						ViralIdentification =
							x.ViralIdentification,

						ParasiteIdentification =
							x.ParasiteIdentification
					}
				)

				.ToListAsync();
	}


	// =========================================================
	// CLEAR SEARCH
	// =========================================================

	public IActionResult OnGetClear()
	{
		return RedirectToPage();
	}


	// =========================================================
	// ROW VIEW MODEL
	// =========================================================

	public sealed class QuestionnaireRow
	{
		public long QuestionnaireId { get; set; }

		public string SpecimenId { get; set; } = string.Empty;

		public string? Lan { get; set; }

		public string? SiteCode { get; set; }

		public string? IsolationDate { get; set; }

		public string? Age { get; set; }

		public string? Gender { get; set; }

		public string? BacterialIdentification { get; set; }

		public string? ViralIdentification { get; set; }

		public string? ParasiteIdentification { get; set; }
	}
}