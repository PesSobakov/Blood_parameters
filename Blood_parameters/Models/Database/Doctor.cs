using System;
using System.Collections.Generic;

namespace Blood_parameters.Models.Database;

public partial class Doctor
{
    public int Id { get; set; }

    public int? YearsOfExperience { get; set; }

    public string? RecordNumber { get; set; }

    public string? Surname { get; set; }

    public string? Name { get; set; }

    public string? Patronymic { get; set; }

    public string? Qualifications { get; set; }

    public string? CurrentPosition { get; set; }

    public string? Education { get; set; }

    public string? ContactPhone { get; set; }

    public byte[]? Photo { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<MedicalCondition> MedicalConditions { get; set; } = new List<MedicalCondition>();

    public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
