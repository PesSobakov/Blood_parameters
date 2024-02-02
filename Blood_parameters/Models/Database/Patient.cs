using System;
using System.Collections.Generic;

namespace Blood_parameters.Models.Database;

public partial class Patient
{
    public int Id { get; set; }

    public string? Surname { get; set; }

    public string? Name { get; set; }

    public string? Patronymic { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Gender { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? EmergencyContactSurname { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactPatronymic { get; set; }

    public string? EmergencyContactPhone { get; set; }

    public int? DoctorId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Doctor? Doctor { get; set; }

    public virtual ICollection<MedicalCondition> MedicalConditions { get; set; } = new List<MedicalCondition>();

    public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();

    public virtual ICollection<PatientsParameter> PatientsParameters { get; set; } = new List<PatientsParameter>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
