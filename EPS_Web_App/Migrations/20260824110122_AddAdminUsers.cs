using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPS_Web_App.Migrations
{
	/// <inheritdoc />
	public partial class AddAdminUsers : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "admin_user",
				columns: table => new
				{
					admin_user_id = table.Column<long>(
						type: "bigint",
						nullable: false
					)
					.Annotation(
						"SqlServer:Identity",
						"1, 1"
					),

					username = table.Column<string>(
						type: "varchar(100)",
						unicode: false,
						maxLength: 100,
						nullable: false
					),

					display_name = table.Column<string>(
						type: "nvarchar(150)",
						maxLength: 150,
						nullable: false
					),

					password_hash = table.Column<string>(
						type: "nvarchar(500)",
						maxLength: 500,
						nullable: false
					),

					is_administrator = table.Column<bool>(
						type: "bit",
						nullable: false
					),

					is_active = table.Column<bool>(
						type: "bit",
						nullable: false,
						defaultValue: true
					),

					created_at = table.Column<DateTime>(
						type: "datetime2(0)",
						precision: 0,
						nullable: false,
						defaultValueSql:
							"(sysutcdatetime())"
					),

					last_login_at = table.Column<DateTime>(
						type: "datetime2(0)",
						precision: 0,
						nullable: true
					)
				},

				constraints: table =>
				{
					table.PrimaryKey(
						"PK_admin_user",
						x => x.admin_user_id
					);
				}
			);


			migrationBuilder.CreateIndex(
				name: "UQ_admin_user_username",
				table: "admin_user",
				column: "username",
				unique: true
			);
		}


		/// <inheritdoc />
		protected override void Down(
			MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "admin_user"
			);
		}
	}
}