using System;
using System.Collections.Generic;

namespace Blood_parameters.Models.Database;

public partial class Medication
{
    public int Id { get; set; }

    public string? ReceptionDuration { get; set; }

    public int? PatientId { get; set; }

    public string? MedicationName { get; set; }

    public string? MedicationDosage { get; set; }

    public string? MedicationFrequency { get; set; }

    public int DoctorId { get; set; }

    public virtual Doctor? Doctor { get; set; } = null!;

    public virtual Patient? Patient { get; set; }
}
