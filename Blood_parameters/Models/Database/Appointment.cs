using System;
using System.Collections.Generic;

namespace Blood_parameters.Models.Database;

public partial class Appointment
{
    public int Id { get; set; }

    public DateOnly TreatmentDate { get; set; }

    public TimeOnly TreatmentTime { get; set; }

    public string RecordNumber { get; set; } = null!;

    public int PatientId { get; set; }

    public int DoctorId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Diagnosis { get; set; }

    public string? Treatment { get; set; }

    public string? TreatmentAndWorkRecommendations { get; set; }

    public string? Recommended { get; set; }

    public virtual Doctor? Doctor { get; set; } = null!;

    public virtual Patient? Patient { get; set; } = null!;
}
