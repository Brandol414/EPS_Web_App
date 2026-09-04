using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Questionnaire;

public class ViewModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public ViewModel(ApplicationDbContext context)
	{
		_context = context;
	}


	// =========================================================
	// ROUTE
	// =========================================================

	[BindProperty(SupportsGet = true)]
	public long Id { get; set; }


	// =========================================================
	// QUESTIONNAIRE RECORD
	// =========================================================

	public QuestionnaireEntry? Record { get; private set; }


	// =========================================================
	// LINKED AST
	// =========================================================

	public List<AstRecord> AstRecords { get; private set; } = [];


	// =========================================================
	// RECONCILIATION ALERTS
	// =========================================================

	public List<ReconciliationAlert> Alerts { get; private set; } = [];


	// =========================================================
	// SUMMARY
	// =========================================================

	public int AstCount =>
		AstRecords.Count;


	public int OpenAlertCount =>
		Alerts.Count(
			x => x.Status == "Open"
		);


	// =========================================================
	// PAGE LOAD
	// =========================================================

	public async Task<IActionResult> OnGetAsync()
	{
		Record =
			await _context.QuestionnaireEntries
				.AsNoTracking()
				.FirstOrDefaultAsync(
					x =>
						x.QuestionnaireId == Id
				);


		if (Record == null)
		{
			return NotFound();
		}


		// -----------------------------------------------------
		// LINKED AST RECORDS
		// -----------------------------------------------------

		AstRecords =
			await _context.AstRecords
				.AsNoTracking()
				.Where(
					x =>
						x.SpecimenId ==
						Record.SpecimenId
				)
				.OrderByDescending(
					x => x.AstRecordId
				)
				.ToListAsync();


		// -----------------------------------------------------
		// RECONCILIATION ALERTS
		// -----------------------------------------------------

		Alerts =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.Where(
					x =>
						x.SpecimenId ==
						Record.SpecimenId
				)
				.OrderByDescending(
					x => x.CreatedAt
				)
				.ToListAsync();


		return Page();
	}
}