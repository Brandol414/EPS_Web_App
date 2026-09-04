using EPS_Web_App.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages;

public class IndexModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public IndexModel(ApplicationDbContext context)
	{
		_context = context;
	}

	// =========================================================
	// CORE COUNTS
	// =========================================================

	public int QuestionnaireCount { get; private set; }

	public int AstCount { get; private set; }

	public int LinkedAstCount { get; private set; }

	public int UnlinkedAstCount { get; private set; }

	public int OpenAlertCount { get; private set; }

	public int HighPriorityAlertCount { get; private set; }

	// =========================================================
	// RECENT ALERTS
	// =========================================================

	public List<AlertSummary> RecentAlerts { get; private set; } = [];

	// =========================================================
	// PAGE LOAD
	// =========================================================

	public async Task OnGetAsync()
	{
		QuestionnaireCount =
			await _context.QuestionnaireEntries
				.AsNoTracking()
				.CountAsync();

		AstCount =
			await _context.AstRecords
				.AsNoTracking()
				.CountAsync();

		LinkedAstCount =
			await _context.AstRecords
				.AsNoTracking()
				.CountAsync(
					x => x.LinkageStatus == "Linked"
				);

		UnlinkedAstCount =
			await _context.AstRecords
				.AsNoTracking()
				.CountAsync(
					x => x.LinkageStatus == "Unlinked"
				);

		OpenAlertCount =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.CountAsync(
					x => x.Status == "Open"
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

		RecentAlerts =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.OrderByDescending(
					x => x.CreatedAt
				)
				.Take(6)
				.Select(
					x => new AlertSummary
					{
						AlertId = x.AlertId,
						SpecimenId = x.SpecimenId,
						AlertType = x.AlertType,
						Priority = x.Priority,
						Status = x.Status,
						Description = x.Description,
						CreatedAt = x.CreatedAt
					}
				)
				.ToListAsync();
	}


	// =========================================================
	// ALERT VIEW MODEL
	// =========================================================

	public sealed class AlertSummary
	{
		public long AlertId { get; set; }

		public string? SpecimenId { get; set; }

		public string AlertType { get; set; } = string.Empty;

		public string Priority { get; set; } = string.Empty;

		public string Status { get; set; } = string.Empty;

		public string? Description { get; set; }

		public DateTime CreatedAt { get; set; }
	}
}