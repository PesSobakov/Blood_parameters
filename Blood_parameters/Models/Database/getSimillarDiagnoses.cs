using System;
using System.Collections.Generic;

namespace Blood_parameters.Models.Database;

public partial class getSimillarDiagnoses
{
    public int appointment_id { get; set; }

    public double match_count { get; set; }
}
