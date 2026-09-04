using System.Reflection;
using EPS_Web_App.Data;
using EPS_Web_App.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPS_Web_App.Services;

public sealed class AstEditService
{
	private readonly ApplicationDbContext _context;

	private readonly ReconciliationService
		_reconciliationService;


	public AstEditService(
		ApplicationDbContext context,
		ReconciliationService reconciliationService)
	{
		_context = context;

		_reconciliationService =
			reconciliationService;
	}


	// =========================================================
	// UPDATE AST RECORD
	// =========================================================

	public async Task<AstEditResult> UpdateAsync(
		long astRecordId,
		IDictionary<string, string?> submittedValues,
		string changedBy,
		string reason,
		CancellationToken cancellationToken = default)
	{
		// -----------------------------------------------------
		// REQUIRED USER
		// -----------------------------------------------------

		if (
			string.IsNullOrWhiteSpace(
				changedBy)
		)
		{
			throw new ArgumentException(
				"Authenticated user identity is required.",
				nameof(changedBy));
		}


		// -----------------------------------------------------
		// REQUIRED REASON
		// -----------------------------------------------------

		if (
			string.IsNullOrWhiteSpace(
				reason)
		)
		{
			throw new ArgumentException(
				"A reason for the correction is required.",
				nameof(reason));
		}


		// -----------------------------------------------------
		// LOAD AST RECORD
		// -----------------------------------------------------

		var record =
			await _context.AstRecords
				.FirstOrDefaultAsync(
					x =>
						x.AstRecordId ==
						astRecordId,
					cancellationToken);


		if (record == null)
		{
			throw new KeyNotFoundException(
				$"AST record #{astRecordId} was not found.");
		}


		// -----------------------------------------------------
		// ORIGINAL SPECIMEN ID
		// -----------------------------------------------------

		var oldSpecimenId =
			NormalizeSpecimenId(
				record.SpecimenId);


		var changedAt =
			DateTime.UtcNow;


		var auditEntries =
			new List<DataEntryAudit>();


		// =====================================================
		// APPLY EDITABLE STRING PROPERTIES
		// =====================================================

		foreach (
			var property
			in GetEditableProperties())
		{
			// -------------------------------------------------
			// Only process fields submitted by the form.
			// -------------------------------------------------

			if (
				!submittedValues.TryGetValue(
					property.Name,
					out var submittedValue)
			)
			{
				continue;
			}


			// -------------------------------------------------
			// ORIGINAL VALUE
			// -------------------------------------------------

			var oldValue =
				property.GetValue(
					record)
				as string;


			// -------------------------------------------------
			// NORMALIZE NEW VALUE
			// -------------------------------------------------

			var newValue =
				NormalizeSubmittedValue(
					submittedValue);


			if (
				string.Equals(
					property.Name,
					nameof(AstRecord.SpecimenId),
					StringComparison.OrdinalIgnoreCase)
			)
			{
				newValue =
					NormalizeSpecimenId(
						newValue);
			}


			if (
				string.Equals(
					property.Name,
					nameof(AstRecord.SiteCode),
					StringComparison.OrdinalIgnoreCase)
			)
			{
				newValue =
					NormalizeSiteCode(
						newValue);
			}


			if (
				string.Equals(
					property.Name,
					nameof(AstRecord.Lan),
					StringComparison.OrdinalIgnoreCase)
			)
			{
				newValue =
					NormalizeLan(
						newValue);
			}


			// -------------------------------------------------
			// NO ACTUAL CHANGE
			// -------------------------------------------------

			if (
				ValuesEqual(
					oldValue,
					newValue)
			)
			{
				continue;
			}


			// -------------------------------------------------
			// APPLY NEW VALUE
			// -------------------------------------------------

			property.SetValue(
				record,
				ConvertValue(
					newValue,
					property.PropertyType));


			// -------------------------------------------------
			// FIELD-LEVEL AUDIT
			// -------------------------------------------------

			auditEntries.Add(
				new DataEntryAudit
				{
					RecordType =
						"AST",

					RecordKey =
						astRecordId.ToString(),

					FieldName =
						property.Name,

					OldValue =
						oldValue,

					NewValue =
						newValue,

					ChangedBy =
						changedBy,

					ChangedAt =
						changedAt,

					Reason =
						reason.Trim()
				});
		}


		// =====================================================
		// NOTHING CHANGED
		// =====================================================

		if (
			auditEntries.Count == 0
		)
		{
			return new AstEditResult
			{
				AstRecordId =
					astRecordId,

				Changed =
					false,

				ChangedFields =
					[]
			};
		}


		// =====================================================
		// NEW SPECIMEN ID
		// =====================================================

		var newSpecimenId =
			NormalizeSpecimenId(
				record.SpecimenId);


		var specimenIdChanged =
			!string.Equals(
				oldSpecimenId,
				newSpecimenId,
				StringComparison.OrdinalIgnoreCase);


		// =====================================================
		// UPDATED TIMESTAMP
		// =====================================================

		record.UpdatedAt =
			changedAt;


		// =====================================================
		// TRANSACTION
		//
		// AST update
		// +
		// audit entries
		// +
		// reconciliation consequences
		//
		// succeed/fail together.
		// =====================================================

		await using var transaction =
			await _context.Database
				.BeginTransactionAsync(
					cancellationToken);


		try
		{
			// -------------------------------------------------
			// SAVE AUDIT ENTRIES
			// -------------------------------------------------

			_context.DataEntryAudits.AddRange(
				auditEntries);


			// -------------------------------------------------
			// SAVE AST + AUDIT
			// -------------------------------------------------

			await _context.SaveChangesAsync(
				cancellationToken);


			// -------------------------------------------------
			// SPECIMEN ID CHANGED
			// -------------------------------------------------

			if (
				specimenIdChanged
				&&
				!string.IsNullOrWhiteSpace(
					oldSpecimenId)
				&&
				!string.IsNullOrWhiteSpace(
					newSpecimenId)
			)
			{
				await _reconciliationService
					.ReconcileSpecimenIdChangeAsync(
						oldSpecimenId,
						newSpecimenId,
						changedBy,
						reason,
						cancellationToken);
			}
			else
			{
				// -------------------------------------------------
				// Normal AST correction.
				//
				// Only reconcile the specimen affected by the edit.
				// -------------------------------------------------

				await _reconciliationService
					.RunForSpecimenAsync(
						newSpecimenId
						??
						string.Empty,
						cancellationToken);
			}


			// -------------------------------------------------
			// COMMIT
			// -------------------------------------------------

			await transaction.CommitAsync(
				cancellationToken);
		}
		catch
		{
			await transaction.RollbackAsync(
				cancellationToken);

			throw;
		}


		// =====================================================
		// RESULT
		// =====================================================

		return new AstEditResult
		{
			AstRecordId =
				astRecordId,

			Changed =
				true,

			ChangedFields =
				auditEntries
					.Select(
						x =>
							x.FieldName!)
					.ToList(),

			Reconciliation =
				null
		};
	}


	// =========================================================
	// EDITABLE AST PROPERTIES
	//
	// Only string fields are exposed.
	//
	// Application-managed identity/status/timestamps remain
	// protected from direct editing.
	// =========================================================

	private static IEnumerable<PropertyInfo>
		GetEditableProperties()
	{
		var protectedFields =
			new HashSet<string>(
				StringComparer.OrdinalIgnoreCase)
			{
				nameof(
					AstRecord.AstRecordId),

				nameof(
					AstRecord.CreatedAt),

				nameof(
					AstRecord.UpdatedAt),

				nameof(
					AstRecord.LinkageStatus)
			};


		var properties =
			typeof(AstRecord)
				.GetProperties(
					BindingFlags.Public |
					BindingFlags.Instance);


		foreach (
			var property
			in properties)
		{
			if (
				!property.CanRead
				||
				!property.CanWrite
			)
			{
				continue;
			}


			if (
				property.PropertyType !=
				typeof(string)
			)
			{
				continue;
			}


			if (
				protectedFields.Contains(
					property.Name)
			)
			{
				continue;
			}


			yield return property;
		}
	}


	// =========================================================
	// SPECIMEN ID NORMALIZATION
	// =========================================================

	private static string?
		NormalizeSpecimenId(
			string? value)
	{
		return
			string.IsNullOrWhiteSpace(
				value)
				?
				null
				:
				value
					.Trim()
					.ToUpperInvariant();
	}


	// =========================================================
	// SITE CODE NORMALIZATION
	// =========================================================

	private static string?
		NormalizeSiteCode(
			string? value)
	{
		return
			string.IsNullOrWhiteSpace(
				value)
				?
				null
				:
				value
					.Trim()
					.ToUpperInvariant();
	}


	// =========================================================
	// LAN NORMALIZATION
	// =========================================================

	private static string?
		NormalizeLan(
			string? value)
	{
		return
			string.IsNullOrWhiteSpace(
				value)
				?
				null
				:
				value
					.Trim()
					.ToUpperInvariant();
	}


	// =========================================================
	// GENERAL INPUT NORMALIZATION
	// =========================================================

	private static string?
		NormalizeSubmittedValue(
			string? value)
	{
		return
			string.IsNullOrWhiteSpace(
				value)
				?
				null
				:
				value.Trim();
	}


	// =========================================================
	// VALUE COMPARISON
	// =========================================================

	private static bool ValuesEqual(
		string? oldValue,
		string? newValue)
	{
		return string.Equals(
			oldValue?.Trim(),
			newValue?.Trim(),
			StringComparison.Ordinal);
	}


	// =========================================================
	// VALUE CONVERSION
	// =========================================================

	private static object?
		ConvertValue(
			string? value,
			Type targetType)
	{
		if (
			targetType ==
			typeof(string)
		)
		{
			return value;
		}


		var underlyingType =
			Nullable.GetUnderlyingType(
				targetType);


		if (
			underlyingType != null
		)
		{
			if (
				string.IsNullOrWhiteSpace(
					value)
			)
			{
				return null;
			}


			targetType =
				underlyingType;
		}


		if (
			targetType ==
			typeof(DateTime)
		)
		{
			return string.IsNullOrWhiteSpace(
				value)
				?
				null
				:
				DateTime.Parse(
					value);
		}


		if (
			targetType ==
			typeof(int)
		)
		{
			return string.IsNullOrWhiteSpace(
				value)
				?
				0
				:
				int.Parse(
					value);
		}


		if (
			targetType ==
			typeof(long)
		)
		{
			return string.IsNullOrWhiteSpace(
				value)
				?
				0L
				:
				long.Parse(
					value);
		}


		if (
			targetType ==
			typeof(decimal)
		)
		{
			return string.IsNullOrWhiteSpace(
				value)
				?
				0m
				:
				decimal.Parse(
					value);
		}


		if (
			targetType ==
			typeof(double)
		)
		{
			return string.IsNullOrWhiteSpace(
				value)
				?
				0d
				:
				double.Parse(
					value);
		}


		if (
			targetType ==
			typeof(bool)
		)
		{
			return bool.TryParse(
				value,
				out var boolean)
				&& boolean;
		}


		if (
			targetType.IsEnum
		)
		{
			return Enum.Parse(
				targetType,
				value!,
				true);
		}


		return Convert.ChangeType(
			value,
			targetType);
	}
}