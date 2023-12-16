using System;
using System.Collections.Generic;

namespace Blood_parameters.Models.Database;

public partial class Norm
{
    public int Id { get; set; }

    public int ParameterId { get; set; }

    public string Gender { get; set; } = null!;

    public double MinValue { get; set; }

    public double MaxValue { get; set; }

    public virtual Parameter? Parameter { get; set; } = null!;
}
