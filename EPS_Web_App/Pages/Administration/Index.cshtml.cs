using EPS_Web_App.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Administration;

public class IndexModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public IndexModel(ApplicationDbContext context)
	{
		_context = context;
	}


	// =========================================================
	// SYSTEM COUNTS
	// =========================================================

	public int QuestionnaireCount
	{
		get;
		private set;
	}

	public int AstCount
	{
		get;
		private set;
	}

	public int OpenAlertCount
	{
		get;
		private set;
	}

	public int LookupValueCount
	{
		get;
		private set;
	}

	public int ActiveLookupValueCount
	{
		get;
		private set;
	}

	public int FormVersionCount
	{
		get;
		private set;
	}

	public int CurrentFormVersionCount
	{
		get;
		private set;
	}

	public int AuditEntryCount
	{
		get;
		private set;
	}


	// =========================================================
	// LOOKUP GROUPS
	// =========================================================

	public List<LookupGroupSummary> LookupGroups
	{
		get;
		private set;
	} = [];


	// =========================================================
	// FORM VERSIONS
	// =========================================================

	public List<FormVersionSummary> CurrentFormVersions
	{
		get;
		private set;
	} = [];


	// =========================================================
	// RECENT AUDIT
	// =========================================================

	public List<AuditSummary> RecentAudits
	{
		get;
		private set;
	} = [];


	// =========================================================
	// PAGE LOAD
	// =========================================================

	public async Task OnGetAsync()
	{
		// -----------------------------------------------------
		// CORE DATA COUNTS
		// -----------------------------------------------------

		QuestionnaireCount =
			await _context.QuestionnaireEntries
				.AsNoTracking()
				.CountAsync();


		AstCount =
			await _context.AstRecords
				.AsNoTracking()
				.CountAsync();


		OpenAlertCount =
			await _context.ReconciliationAlerts
				.AsNoTracking()
				.CountAsync(
					x =>
						x.Status == "Open"
				);


		// -----------------------------------------------------
		// LOOKUPS
		// -----------------------------------------------------

		LookupValueCount =
			await _context.LookupValues
				.AsNoTracking()
				.CountAsync();


		ActiveLookupValueCount =
			await _context.LookupValues
				.AsNoTracking()
				.CountAsync(
					x =>
						x.IsActive
				);


		LookupGroups =
			await _context.LookupValues
				.AsNoTracking()
				.GroupBy(
					x =>
						x.LookupGroup
				)
				.Select(
					x =>
						new LookupGroupSummary
						{
							LookupGroup =
								x.Key,

							TotalValues =
								x.Count(),

							ActiveValues =
								x.Count(
									y =>
										y.IsActive
								)
						}
				)
				.OrderBy(
					x =>
						x.LookupGroup
				)
				.ToListAsync();


		// -----------------------------------------------------
		// FORM VERSIONS
		// -----------------------------------------------------

		FormVersionCount =
			await _context.FormVersions
				.AsNoTracking()
				.CountAsync();


		CurrentFormVersionCount =
			await _context.FormVersions
				.AsNoTracking()
				.CountAsync(
					x =>
						x.IsCurrent
				);


		CurrentFormVersions =
			await _context.FormVersions
				.AsNoTracking()
				.Where(
					x =>
						x.IsCurrent
				)
				.OrderBy(
					x =>
						x.FormName
				)
				.Select(
					x =>
						new FormVersionSummary
						{
							FormVersionId =
								x.FormVersionId,

							FormName =
								x.FormName,

							VersionNumber =
								x.VersionNumber,

							EffectiveFrom =
								x.EffectiveFrom,

							EffectiveTo =
								x.EffectiveTo,

							Notes =
								x.Notes,

							IsCurrent =
								x.IsCurrent
						}
				)
				.ToListAsync();


		// -----------------------------------------------------
		// AUDIT
		// -----------------------------------------------------

		AuditEntryCount =
			await _context.DataEntryAudits
				.AsNoTracking()
				.CountAsync();


		RecentAudits =
			await _context.DataEntryAudits
				.AsNoTracking()
				.OrderByDescending(
					x =>
						x.ChangedAt
				)
				.Take(10)
				.Select(
					x =>
						new AuditSummary
						{
							AuditId =
								x.AuditId,

							RecordType =
								x.RecordType,

							RecordKey =
								x.RecordKey,

							FieldName =
								x.FieldName,

							OldValue =
								x.OldValue,

							NewValue =
								x.NewValue,

							Reason =
								x.Reason,

							ChangedBy =
								x.ChangedBy,

							ChangedAt =
								x.ChangedAt
						}
				)
				.ToListAsync();
	}


	// =========================================================
	// LOOKUP GROUP SUMMARY
	// =========================================================

	public sealed class LookupGroupSummary
	{
		public string LookupGroup { get; set; } =
			string.Empty;

		public int TotalValues { get; set; }

		public int ActiveValues { get; set; }
	}


	// =========================================================
	// FORM VERSION SUMMARY
	// =========================================================

	public sealed class FormVersionSummary
	{
		public long FormVersionId { get; set; }

		public string FormName { get; set; } =
			string.Empty;

		public string VersionNumber { get; set; } =
			string.Empty;

		public DateTime EffectiveFrom { get; set; }

		public DateTime? EffectiveTo { get; set; }

		public bool IsCurrent { get; set; }

		public string? Notes { get; set; }
	}


	// =========================================================
	// AUDIT SUMMARY
	// =========================================================

	public sealed class AuditSummary
	{
		public long AuditId { get; set; }

		public string RecordType { get; set; } =
			string.Empty;

		public string RecordKey { get; set; } =
			string.Empty;

		public string FieldName { get; set; } =
			string.Empty;

		public string? OldValue { get; set; }

		public string? NewValue { get; set; }

		public string? Reason { get; set; }

		public string? ChangedBy { get; set; }

		public DateTime ChangedAt { get; set; }
	}
}