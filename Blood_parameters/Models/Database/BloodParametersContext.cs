using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Blood_parameters.Models.Database;

public partial class BloodParametersContext : DbContext
{
    public BloodParametersContext()
    {
    }

    public BloodParametersContext(DbContextOptions<BloodParametersContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<MedicalCondition> MedicalConditions { get; set; }

    public virtual DbSet<Medication> Medications { get; set; }

    public virtual DbSet<Norm> Norms { get; set; }

    public virtual DbSet<Parameter> Parameters { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<PatientsParameter> PatientsParameters { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }
    
    public virtual DbSet<getSimillarDiagnoses> getSimillarDiagnoses { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Blood_parameters;Trusted_Connection=True;", builder =>
        //optionsBuilder.UseSqlServer($"Server=(localdb)\\mssqllocaldb;AttachDbFilename={System.Environment.CurrentDirectory}\\Blood_parameters.mdf;Trusted_Connection=True;User Instance=True;", builder =>
        //optionsBuilder.UseSqlServer($"Server=(localdb)\\mssqllocaldb;AttachDbFilename=|DataDirectory|\\folder\\qwertyuiop1.mdf;Trusted_Connection=True;", builder =>
        //optionsBuilder.UseSqlServer($"Server=(localdb)\\mssqllocaldb;AttachDbFilename=D:\\Blood_parameters.mdf;Trusted_Connection=True;Initial Catalog=D:;", builder =>/*works*/
        optionsBuilder.UseSqlServer($"Data Source=(localdb)\\mssqllocaldb;AttachDbFilename=|DataDirectory|\\Blood_parameters.mdf;Trusted_Connection=True;Database=Blood_parameters;", builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            builder.UseDateOnlyTimeOnly();
        });
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<getSimillarDiagnoses>(entity =>
        {
            entity.HasNoKey();
            entity.Property(e => e.appointment_id).HasColumnName("appointment_id");
            entity.Property(e => e.match_count).HasColumnName("match_count");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3213E83F304A24B0");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Diagnosis)
                .HasMaxLength(1000)
                .HasColumnName("diagnosis");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Recommended)
                .HasMaxLength(500)
                .HasColumnName("recommended");
            entity.Property(e => e.RecordNumber)
                .HasMaxLength(20)
                .HasColumnName("record_number");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Treatment)
                .HasMaxLength(1000)
                .HasColumnName("treatment");
            entity.Property(e => e.TreatmentAndWorkRecommendations)
                .HasMaxLength(500)
                .HasColumnName("treatment_and_work_recommendations");
            entity.Property(e => e.TreatmentDate).HasColumnName("treatment_date");
            entity.Property(e => e.TreatmentTime).HasColumnName("treatment_time");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_doctors");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_patients");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Doctors__3213E83F1324E97E");

            entity.ToTable(tb => tb.HasTrigger("doctor_phone"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(20)
                .HasColumnName("contact_phone");
            entity.Property(e => e.CurrentPosition)
                .HasMaxLength(30)
                .HasColumnName("current_position");
            entity.Property(e => e.Education)
                .HasMaxLength(30)
                .HasColumnName("education");
            entity.Property(e => e.Name)
                .HasMaxLength(25)
                .HasColumnName("name");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(25)
                .HasColumnName("patronymic");
            entity.Property(e => e.Photo).HasColumnName("photo");
            entity.Property(e => e.Qualifications)
                .HasMaxLength(50)
                .HasColumnName("qualifications");
            entity.Property(e => e.RecordNumber)
                .HasMaxLength(10)
                .HasColumnName("record_number");
            entity.Property(e => e.Surname)
                .HasMaxLength(25)
                .HasColumnName("surname");
            entity.Property(e => e.YearsOfExperience).HasColumnName("years_of_experience");
        });

        modelBuilder.Entity<MedicalCondition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MedicalC__3213E83F7CF58086");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClinicalHistory)
                .HasMaxLength(1000)
                .HasColumnName("clinical_history");
            entity.Property(e => e.Complaints)
                .HasMaxLength(1000)
                .HasColumnName("complaints");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.MedicalHistory)
                .HasMaxLength(1000)
                .HasColumnName("medical_history");
            entity.Property(e => e.ObjectiveCondition)
                .HasMaxLength(1000)
                .HasColumnName("objective_condition");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");

            entity.HasOne(d => d.Doctor).WithMany(p => p.MedicalConditions)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalConditions_doctors");

            entity.HasOne(d => d.Patient).WithMany(p => p.MedicalConditions)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MedicalConditions_patients");
        });

        modelBuilder.Entity<Medication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medicati__3213E83F1F7D3FA6");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.MedicationDosage)
                .HasMaxLength(20)
                .HasColumnName("medication_dosage");
            entity.Property(e => e.MedicationFrequency)
                .HasMaxLength(50)
                .HasColumnName("medication_frequency");
            entity.Property(e => e.MedicationName)
                .HasMaxLength(20)
                .HasColumnName("medication_name");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.ReceptionDuration)
                .HasMaxLength(50)
                .HasColumnName("reception_duration");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Medications)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Medications_doctors");

            entity.HasOne(d => d.Patient).WithMany(p => p.Medications)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_Medications_patients");
        });

        modelBuilder.Entity<Norm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Norms__3213E83F233EA5B6");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .HasColumnName("gender");
            entity.Property(e => e.MaxValue).HasColumnName("max_value");
            entity.Property(e => e.MinValue).HasColumnName("min_value");
            entity.Property(e => e.ParameterId).HasColumnName("parameter_id");

            entity.HasOne(d => d.Parameter).WithMany(p => p.Norms)
                .HasForeignKey(d => d.ParameterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Norms_parameters");
        });

        modelBuilder.Entity<Parameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Paramete__3213E83FBEE34A82");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Patients__3213E83F6029AEC9");

            entity.ToTable(tb => tb.HasTrigger("DeletePatient"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(50)
                .HasColumnName("address");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.EmergencyContactName)
                .HasMaxLength(25)
                .HasColumnName("emergency_contact_name");
            entity.Property(e => e.EmergencyContactPatronymic)
                .HasMaxLength(25)
                .HasColumnName("emergency_contact_patronymic");
            entity.Property(e => e.EmergencyContactPhone)
                .HasMaxLength(20)
                .HasColumnName("emergency_contact_phone");
            entity.Property(e => e.EmergencyContactSurname)
                .HasMaxLength(25)
                .HasColumnName("emergency_contact_surname");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .HasColumnName("gender");
            entity.Property(e => e.Name)
                .HasMaxLength(25)
                .HasColumnName("name");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(25)
                .HasColumnName("patronymic");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Surname)
                .HasMaxLength(25)
                .HasColumnName("surname");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Patients)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK_Patients_doctors");
        });

        modelBuilder.Entity<PatientsParameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Patients__3213E83F6ABDCB34");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateOfCheck).HasColumnName("date_of_check");
            entity.Property(e => e.ParameterId).HasColumnName("parameter_id");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Parameter).WithMany(p => p.PatientsParameters)
                .HasForeignKey(d => d.ParameterId)
                .HasConstraintName("FK_PatientsParameters_Parameters");

            entity.HasOne(d => d.Patient).WithMany(p => p.PatientsParameters)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_PatientsParameters_Patients");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3213E83FEFBFCA0B");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3213E83F15D05497");

            entity.HasIndex(e => e.Email, "unique1").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DoctorId).HasColumnName("doctor_id");
            entity.Property(e => e.Email)
                .HasMaxLength(30)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(30)
                .HasColumnName("password");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Users)
                .HasForeignKey(d => d.DoctorId)
                .HasConstraintName("FK_doctor");

            entity.HasOne(d => d.Patient).WithMany(p => p.Users)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("FK_patient");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
