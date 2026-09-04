using System;
using System.Collections.Generic;

namespace EPS_Web_App.Data.Models;

public partial class ReconciliationAlert
{
    public long AlertId { get; set; }

    public string? SpecimenId { get; set; }

    public string AlertType { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? SourceRecord { get; set; }

    public string? Description { get; set; }

    public string? ResolutionNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public string? ResolvedBy { get; set; }
}
