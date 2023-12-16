using System;
using System.Collections.Generic;

namespace Blood_parameters.Models.Database;

public partial class MedicalCondition
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public string? Complaints { get; set; }

    public string? MedicalHistory { get; set; }

    public string? ClinicalHistory { get; set; }

    public string? ObjectiveCondition { get; set; }

    public virtual Doctor? Doctor { get; set; } = null!;

    public virtual Patient? Patient { get; set; } = null!;
}
