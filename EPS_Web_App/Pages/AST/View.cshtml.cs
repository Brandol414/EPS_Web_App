using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.AST;

public class ViewModel : PageModel
{
	private readonly ApplicationDbContext _context;


	public ViewModel(
		ApplicationDbContext context)
	{
		_context = context;
	}


	// =========================================================
	// ROUTE
	// =========================================================

	[BindProperty(SupportsGet = true)]
	public long Id { get; set; }


	// =========================================================
	// AST RECORD
	// =========================================================

	public AstRecord? Record { get; private set; }


	// =========================================================
	// LINKED QUESTIONNAIRE
	// =========================================================

	public QuestionnaireEntry? QuestionnaireRecord
	{
		get;
		private set;
	}


	// =========================================================
	// AUDIT HISTORY
	// =========================================================

	public List<DataEntryAudit> AuditHistory { get; private set; } =
		[];


	// =========================================================
	// SUMMARY
	// =========================================================

	public bool IsLinked =>
		string.Equals(
			Record?.LinkageStatus,
			"Linked",
			StringComparison.OrdinalIgnoreCase
		);


	// =========================================================
	// PAGE LOAD
	// =========================================================

	public async Task<IActionResult> OnGetAsync(
		CancellationToken cancellationToken = default)
	{
		// -----------------------------------------------------
		// LOAD AST RECORD
		// -----------------------------------------------------

		Record =
			await _context.AstRecords
				.AsNoTracking()
				.FirstOrDefaultAsync(
					x =>
						x.AstRecordId == Id,
					cancellationToken
				);


		if (Record == null)
		{
			return NotFound();
		}


		// -----------------------------------------------------
		// QUESTIONNAIRE LINK
		// -----------------------------------------------------

		if (!string.IsNullOrWhiteSpace(
				Record.SpecimenId))
		{
			QuestionnaireRecord =
				await _context.QuestionnaireEntries
					.AsNoTracking()
					.FirstOrDefaultAsync(
						x =>
							x.SpecimenId ==
							Record.SpecimenId,
						cancellationToken
					);
		}


		// -----------------------------------------------------
		// AUDIT HISTORY
		//
		// AST edit service stores the AST record ID in
		// DataEntryAudit.RecordKey.
		// -----------------------------------------------------

		AuditHistory =
			await _context.DataEntryAudits
				.AsNoTracking()
				.Where(
					x =>
						x.RecordType == "AST"
						&&
						x.RecordKey ==
							Id.ToString()
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


		return Page();
	}
}