namespace EPS_Web_App.Services;

public sealed class AstEditResult
{
	public long AstRecordId { get; init; }

	public bool Changed { get; init; }

	public List<string> ChangedFields { get; init; } = [];

	public ReconciliationRunResult? Reconciliation { get; init; }
}