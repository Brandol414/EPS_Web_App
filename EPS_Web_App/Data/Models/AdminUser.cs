using System;

namespace EPS_Web_App.Data.Models;

public partial class AdminUser
{
	public long AdminUserId { get; set; }

	public string Username { get; set; } =
		string.Empty;

	public string DisplayName { get; set; } =
		string.Empty;

	public string PasswordHash { get; set; } =
		string.Empty;

	public bool IsAdministrator { get; set; }

	public bool IsActive { get; set; }

	public DateTime CreatedAt { get; set; }

	public DateTime? LastLoginAt { get; set; }
}