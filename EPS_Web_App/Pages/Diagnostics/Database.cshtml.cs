using EPS_Web_App.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Pages.Diagnostics;

public class DatabaseModel : PageModel
{
	private readonly ApplicationDbContext _context;

	public DatabaseModel(ApplicationDbContext context)
	{
		_context = context;
	}

	public string StatusMessage { get; private set; } = string.Empty;

	public int QuestionnaireCount { get; private set; }

	public int AstCount { get; private set; }

	public async Task OnGetAsync()
	{
		try
		{
			var canConnect =
				await _context.Database.CanConnectAsync();

			if (!canConnect)
			{
				StatusMessage =
					"SQL Server connection could not be established.";

				return;
			}

			QuestionnaireCount =
				await _context.QuestionnaireEntries.CountAsync();

			AstCount =
				await _context.AstRecords.CountAsync();

			StatusMessage =
				"SQL Server connection successful.";
		}
		catch (Exception ex)
		{
			StatusMessage =
				$"Database connection failed: {ex.Message}";
		}
	}
}