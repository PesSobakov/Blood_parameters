using System;
using System.Collections.Generic;

namespace Blood_parameters.Models.Database;

public partial class PatientsParameter
{
    public int Id { get; set; }

    public int? PatientId { get; set; }

    public int? ParameterId { get; set; }

    public double? Value { get; set; }

    public DateOnly? DateOfCheck { get; set; }

    public virtual Parameter? Parameter { get; set; }

    public virtual Patient? Patient { get; set; }
}
