using EPS_Web_App.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Data;

public partial class ApplicationDbContext
{
	// =========================================================
	// ADMIN USERS
	// =========================================================

	public virtual DbSet<AdminUser> AdminUsers { get; set; }


	// =========================================================
	// MODEL CONFIGURATION
	//
	// ApplicationDbContext already calls this partial method
	// at the end of its existing OnModelCreating method.
	// =========================================================

	partial void OnModelCreatingPartial(
		ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<AdminUser>(
			entity =>
			{
				entity.ToTable(
					"admin_user"
				);


				entity.HasKey(
					e =>
						e.AdminUserId
				);


				entity.HasIndex(
					e =>
						e.Username
				)
				.IsUnique()
				.HasDatabaseName(
					"UQ_admin_user_username"
				);


				entity.Property(
					e =>
						e.AdminUserId
				)
				.HasColumnName(
					"admin_user_id"
				);


				entity.Property(
					e =>
						e.Username
				)
				.HasMaxLength(100)
				.IsUnicode(false)
				.HasColumnName(
					"username"
				);


				entity.Property(
					e =>
						e.DisplayName
				)
				.HasMaxLength(150)
				.HasColumnName(
					"display_name"
				);


				entity.Property(
					e =>
						e.PasswordHash
				)
				.HasMaxLength(500)
				.HasColumnName(
					"password_hash"
				);


				entity.Property(
					e =>
						e.IsAdministrator
				)
				.HasColumnName(
					"is_administrator"
				);


				entity.Property(
					e =>
						e.IsActive
				)
				.HasDefaultValue(
					true
				)
				.HasColumnName(
					"is_active"
				);


				entity.Property(
					e =>
						e.CreatedAt
				)
				.HasPrecision(0)
				.HasDefaultValueSql(
					"(sysutcdatetime())"
				)
				.HasColumnName(
					"created_at"
				);


				entity.Property(
					e =>
						e.LastLoginAt
				)
				.HasPrecision(0)
				.HasColumnName(
					"last_login_at"
				);
			}
		);
	}
}