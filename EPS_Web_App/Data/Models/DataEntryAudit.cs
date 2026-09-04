using System;
using System.Collections.Generic;

namespace EPS_Web_App.Data.Models;

public partial class DataEntryAudit
{
    public long AuditId { get; set; }

    public string RecordType { get; set; } = null!;

    public string RecordKey { get; set; } = null!;

    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string ChangedBy { get; set; } = null!;

    public DateTime ChangedAt { get; set; }

    public string? Reason { get; set; }
}
