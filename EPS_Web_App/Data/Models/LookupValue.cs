using System;
using System.Collections.Generic;

namespace EPS_Web_App.Data.Models;

public partial class LookupValue
{
    public long LookupId { get; set; }

    public string LookupGroup { get; set; } = null!;

    public string LookupCode { get; set; } = null!;

    public string LookupLabel { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}
