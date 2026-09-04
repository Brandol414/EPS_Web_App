using System;
using System.Collections.Generic;

namespace EPS_Web_App.Data.Models;

public partial class FormVersion
{
    public long FormVersionId { get; set; }

    public string FormName { get; set; } = null!;

    public string VersionNumber { get; set; } = null!;

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsCurrent { get; set; }

    public string? Notes { get; set; }
}
