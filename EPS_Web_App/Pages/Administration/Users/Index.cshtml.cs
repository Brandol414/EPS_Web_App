using EPS_Web_App.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Administration.Users;

public class IndexModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public IndexModel(
		ApplicationDbContext context)
	{
		_context = context;
	}


	// =========================================================
	// USERS
	// =========================================================

	public List<UserSummary> Users { get; private set; } =
		[];


	// =========================================================
	// PAGE LOAD
	// =========================================================

	public async Task OnGetAsync()
	{
		Users =
			await _context.AdminUsers
				.AsNoTracking()
				.OrderBy(
					x =>
						x.DisplayName
				)
				.Select(
					x =>
						new UserSummary
						{
							AdminUserId =
								x.AdminUserId,

							Username =
								x.Username,

							DisplayName =
								x.DisplayName,

							IsAdministrator =
								x.IsAdministrator,

							IsActive =
								x.IsActive,

							CreatedAt =
								x.CreatedAt,

							LastLoginAt =
								x.LastLoginAt
						}
				)
				.ToListAsync();
	}


	// =========================================================
	// VIEW MODEL
	// =========================================================

	public sealed class UserSummary
	{
		public long AdminUserId { get; set; }

		public string Username { get; set; } =
			string.Empty;

		public string DisplayName { get; set; } =
			string.Empty;

		public bool IsAdministrator { get; set; }

		public bool IsActive { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime? LastLoginAt { get; set; }
	}
}