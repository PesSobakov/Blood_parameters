using Blood_parameters.Models;
using Blood_parameters.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using static Blood_parameters.Models.DoctorTable;

namespace Blood_parameters.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdministratorController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public AdministratorController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Patients(Blood_parameters.Models.PatientTable record)
        {
            if (record.add == null)
            {
                record.add = new Patient();
                record.add.Surname = "Іванеко";
                record.add.Name = "Іван";
                record.add.Patronymic = "Іванович";
                record.add.Dob = new DateOnly(1999, 1, 1);
                record.add.Gender = "ч";
                record.add.Phone = "+380961321103";
                record.add.Address = "м. Кривий Ріг вул. Поштова 3";
                record.add.EmergencyContactSurname = "Іванеко";
                record.add.EmergencyContactName = "Іван";
                record.add.EmergencyContactPatronymic = "Петрович";
                record.add.EmergencyContactPhone = "+380973908070";
            }

            ViewBag.Title = "Patients";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.Head = new List<string>() {
               "ID",
               "Прізвище",
               "Ім’я",
               "По батькові",
               "Дата народження",
               "Стать",
               "Телефон",
               "Адрес",
               "Прізвище контактної особи",
               "Ім’я контактної особи",
               "По батькові контактної особи",
               "Телефон контактної особи",
               "ID лікаря"};
                ViewBag.Table = db.Patients.ToList().Select(x => new List<string?>() {
               x.Id.ToString(),
               x.Surname,
               x.Name,
               x.Patronymic,
               x.Dob?.ToShortDateString(),
               x.Gender,
               x.Phone,
               x.Address,
               x.EmergencyContactSurname,
               x.EmergencyContactName,
               x.EmergencyContactPatronymic,
               x.EmergencyContactPhone,
               x.DoctorId.ToString()}).ToList();
            }
            return View("Patients", record);
        }

        [HttpPost]
        public IActionResult PatientsAdd(Blood_parameters.Models.PatientTable record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Patients.Add(record.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return Patients(record);
            }
        }

        [HttpPost]
        public IActionResult PatientsUpdate(Blood_parameters.Models.PatientTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Patient? patient = db.Patients.Where(x => x.Id == record.update.Id).FirstOrDefault();
                        if (patient == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Patients(record);
                        }
                        patient.Id = record.update.Id;
                        patient.Surname = record.update.Surname;
                        patient.Name = record.update.Name;
                        patient.Patronymic = record.update.Patronymic;
                        patient.Dob = record.update.Dob;
                        patient.Gender = record.update.Gender;
                        patient.Phone = record.update.Phone;
                        patient.Address = record.update.Address;
                        patient.EmergencyContactSurname = record.update.EmergencyContactSurname;
                        patient.EmergencyContactName = record.update.EmergencyContactName;
                        patient.EmergencyContactPatronymic = record.update.EmergencyContactPatronymic;
                        patient.EmergencyContactPhone = record.update.EmergencyContactPhone;
                        patient.DoctorId = record.update.DoctorId;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Patients(record);
        }

        [HttpPost]
        public IActionResult PatientsFillUpdate(Blood_parameters.Models.PatientTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Patient? patient = db.Patients.Where(x => x.Id == record.fillUpdate).FirstOrDefault();
                        if (patient == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Patients(record);
                        }
                        record.update ??= new();

                        record.update.Id = patient.Id;
                        record.update.Surname = patient.Surname;
                        record.update.Name = patient.Name;
                        record.update.Patronymic = patient.Patronymic;
                        record.update.Dob = patient.Dob;
                        record.update.Gender = patient.Gender;
                        record.update.Phone = patient.Phone;
                        record.update.Address = patient.Address;
                        record.update.EmergencyContactSurname = patient.EmergencyContactSurname;
                        record.update.EmergencyContactName = patient.EmergencyContactName;
                        record.update.EmergencyContactPatronymic = patient.EmergencyContactPatronymic;
                        record.update.EmergencyContactPhone = patient.EmergencyContactPhone;
                        record.update.DoctorId = patient.DoctorId;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Patients(record);
        }

        [HttpPost]
        public IActionResult PatientsDelete(Blood_parameters.Models.PatientTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.Patients.Where(x => x.Id == record.id);
                        if (!query.Any())
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Patients(record);
                        }
                        db.Patients.Remove(query.First());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Patients(record);
        }
        /*
        [HttpPost]
        public IActionResult PatientsDeleteFull(Blood_parameters.Models.PatientTable record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        int rows = db.Database.ExecuteSql($"DeletePatient {record.id}");
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }
            }
            return Patients(record);
        }
        */
        [HttpGet]
        public IActionResult Doctors(Blood_parameters.Models.DoctorTable record)
        {
            if (record.add == null)
            {
                record.add = new Doctor();
                record.add.YearsOfExperience = 10;
                record.add.RecordNumber = "REC-101";
                record.add.Surname = "Іванеко";
                record.add.Name = "Іван";
                record.add.Patronymic = "Іванович";
                record.add.Qualifications = "Сертифікований терапевт";
                record.add.CurrentPosition = "Терапевт";
                record.add.Education = "Медичний коледж";
                record.add.ContactPhone = "+380961321103";
            }
            ViewBag.Title = "Doctors";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.Head = new List<string>() {
               "ID",
               "Досвід",
               "Код лікаря",
               "Прізвище",
               "Ім’я",
               "По батькові",
               "Кваліфікація",
               "Посада",
               "Освіта",
               "Телефон"};
                ViewBag.Table = db.Doctors.ToList().Select(x => new List<string?>() {
               x.Id.ToString(),
               x.YearsOfExperience.ToString(),
               x.RecordNumber,
               x.Surname,
               x.Name,
               x.Patronymic,
               x.Qualifications,
               x.CurrentPosition,
               x.Education,
               x.ContactPhone}).ToList();
            }
            return View("Doctors", record);
        }

        [HttpPost]
        public IActionResult DoctorsAdd(Blood_parameters.Models.DoctorTable record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Doctors.Add(record.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return Doctors(record);
            }
        }

        [HttpPost]
        public IActionResult DoctorsUpdate(Blood_parameters.Models.DoctorTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Doctor? doctor = db.Doctors.Where(x => x.Id == record.update.Id).FirstOrDefault();
                        if (doctor == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Doctors(record);
                        }

                        doctor.YearsOfExperience = record.update.YearsOfExperience;
                        doctor.RecordNumber = record.update.RecordNumber;
                        doctor.Surname = record.update.Surname;
                        doctor.Name = record.update.Name;
                        doctor.Patronymic = record.update.Patronymic;
                        doctor.Qualifications = record.update.Qualifications;
                        doctor.CurrentPosition = record.update.CurrentPosition;
                        doctor.Education = record.update.Education;
                        doctor.ContactPhone = record.update.ContactPhone;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Doctors(record);
        }

        [HttpPost]
        public IActionResult DoctorsFillUpdate(Blood_parameters.Models.DoctorTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Doctor? doctor = db.Doctors.Where(x => x.Id == record.fillUpdate).FirstOrDefault();
                        if (doctor == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Doctors(record);
                        }
                        record.update ??= new();

                        record.update.Id = doctor.Id;
                        record.update.YearsOfExperience = doctor.YearsOfExperience;
                        record.update.RecordNumber = doctor.RecordNumber;
                        record.update.Surname = doctor.Surname;
                        record.update.Name = doctor.Name;
                        record.update.Patronymic = doctor.Patronymic;
                        record.update.Qualifications = doctor.Qualifications;
                        record.update.CurrentPosition = doctor.CurrentPosition;
                        record.update.Education = doctor.Education;
                        record.update.ContactPhone = doctor.ContactPhone;

                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Doctors(record);
        }

        [HttpPost]
        public IActionResult DoctorsDelete(Blood_parameters.Models.DoctorTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.Doctors.Where(x => x.Id == record.id);
                        if (!query.Any())
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Doctors(record);
                        }
                        db.Doctors.Remove(query.First());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Doctors(record);
        }

        [HttpPost]
        public IActionResult DoctorPictureUpdate(Blood_parameters.Models.DoctorTable record)
        {
            if (record?.picture == null)
            {
                ViewBag.Error = "Файл не вибраний";
                return Doctors(record);
            }
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {

                        Doctor doctor = db.Doctors.Where(x => x.Id == record.pictureId).ToList().FirstOrDefault();

                        if (doctor == null)
                        {
                            ViewBag.Error = "Лікаря з таким ID не знайдено";
                            return Doctors(record);
                        }

                        using var fileStream = record.picture.OpenReadStream();
                        byte[] bytes = new byte[(int)record.picture.Length];
                        fileStream.Read(bytes, 0, (int)record.picture.Length);

                        var png = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                        if (bytes.Length < png.Length)
                        {
                            ViewBag.Error = "Файл повинен бути в форматі png";
                            return Doctors(record);
                        }
                        for (int i = 0; i < png.Length; i++)
                        {
                            if (bytes[i] != png[i])
                            {
                                ViewBag.Error = "Файл повинен бути в форматі png";
                                return Doctors(record);
                            }
                        }

                        doctor.Photo = bytes;
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return Doctors(record);
            }
        }


        [HttpGet]
        public IActionResult Appointments(Blood_parameters.Models.AppointmentsTable record)
        {
            if (record.add == null)
            {
                record.add = new Appointment();
                record.add.TreatmentDate = DateOnly.FromDateTime(DateTime.Now);
                record.add.TreatmentTime = TimeOnly.FromDateTime(DateTime.Now);
                record.add.RecordNumber = "000101\\2023";
                record.add.PatientId = 1;
                record.add.DoctorId = 1;
                record.add.StartDate = DateOnly.FromDateTime(DateTime.Now);
                record.add.EndDate = DateOnly.FromDateTime(DateTime.Now);
                record.add.Diagnosis = "Текст діагнозу";
                record.add.Treatment = "Текст лікування";
                record.add.TreatmentAndWorkRecommendations = "Текст Медичних показаннь";
                record.add.Recommended = "Текст рекомендації";
            }
            ViewBag.Title = "Appointments";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.Head = new List<string>() {
               "ID",
               "Дата прийому",
               "Час прийому",
               "Код прийому",
               "ID пацієнта",
               "ID лікаря",
               "Дата початку лікування",
               "Дата завершення лікування",
               "Діагноз",
               "Медичні засоби",
               "Медичні показання",
               "Рекомендації"};
                ViewBag.Table = db.Appointments.ToList().Select(x => new List<string?>() {
               x.Id.ToString(),
               x.TreatmentDate.ToShortDateString(),
               x.TreatmentTime.ToString(),
               x.RecordNumber,
               x.PatientId.ToString(),
               x.DoctorId.ToString(),
               x.StartDate.ToShortDateString(),
               x.EndDate.ToShortDateString(),
               x.Diagnosis,
               x.Treatment,
               x.TreatmentAndWorkRecommendations,
               x.Recommended}).ToList();
            }
            return View("Appointments", record);
        }

        [HttpPost]
        public IActionResult AppointmentsAdd(Blood_parameters.Models.AppointmentsTable record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Appointments.Add(record.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return Appointments(record);
            }
        }

        [HttpPost]
        public IActionResult AppointmentsUpdate(Blood_parameters.Models.AppointmentsTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Appointment? appointment = db.Appointments.Where(x => x.Id == record.update.Id).FirstOrDefault();
                        if (appointment == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Appointments(record);
                        }

                        appointment.TreatmentDate = record.update.TreatmentDate;
                        appointment.TreatmentTime = record.update.TreatmentTime;
                        appointment.RecordNumber = record.update.RecordNumber;
                        appointment.PatientId = record.update.PatientId;
                        appointment.DoctorId = record.update.DoctorId;
                        appointment.StartDate = record.update.StartDate;
                        appointment.EndDate = record.update.EndDate;
                        appointment.Diagnosis = record.update.Diagnosis;
                        appointment.Treatment = record.update.Treatment;
                        appointment.TreatmentAndWorkRecommendations = record.update.TreatmentAndWorkRecommendations;
                        appointment.Recommended = record.update.Recommended;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Appointments(record);
        }

        [HttpPost]
        public IActionResult AppointmentsFillUpdate(Blood_parameters.Models.AppointmentsTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Appointment? appointment = db.Appointments.Where(x => x.Id == record.fillUpdate).FirstOrDefault();
                        if (appointment == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Appointments(record);
                        }
                        record.update ??= new();

                        record.update.Id = appointment.Id;
                        record.update.TreatmentDate = appointment.TreatmentDate;
                        record.update.TreatmentTime = appointment.TreatmentTime;
                        record.update.RecordNumber = appointment.RecordNumber;
                        record.update.PatientId = appointment.PatientId;
                        record.update.DoctorId = appointment.DoctorId;
                        record.update.StartDate = appointment.StartDate;
                        record.update.EndDate = appointment.EndDate;
                        record.update.Diagnosis = appointment.Diagnosis;
                        record.update.Treatment = appointment.Treatment;
                        record.update.TreatmentAndWorkRecommendations = appointment.TreatmentAndWorkRecommendations;
                        record.update.Recommended = appointment.Recommended;
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Appointments(record);
        }

        [HttpPost]
        public IActionResult AppointmentsDelete(Blood_parameters.Models.AppointmentsTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.Appointments.Where(x => x.Id == record.id);
                        if (!query.Any())
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Appointments(record);
                        }
                        db.Appointments.Remove(query.First());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Appointments(record);
        }


        [HttpGet]
        public IActionResult MedicalConditions(Blood_parameters.Models.MedicalConditionsTable record)
        {
            if (record.add == null)
            {
                record.add = new MedicalCondition();
                record.add.PatientId = 1;
                record.add.DoctorId = 1;
                record.add.Complaints = "Скарги пацієнта";
                record.add.MedicalHistory = "Медична історія";
                record.add.ClinicalHistory = "Клінічна історія";
                record.add.ObjectiveCondition = "Об’єктивний стан пацієнта";
            }

            ViewBag.Title = "MedicalConditions";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.Head = new List<string>() {
                    "ID",
                    "ID пацієнта",
                    "ID лікаря",
                    "Скарги пацієнта",
                    "Медична історія",
                    "Клінічна історія",
                    "Об’єктивний стан пацієнта"};
                ViewBag.Table = db.MedicalConditions.ToList().Select(x => new List<string?>() {
                    x.Id.ToString(),
                    x.PatientId.ToString(),
                    x.DoctorId.ToString(),
                    x.Complaints,
                    x.MedicalHistory,
                    x.ClinicalHistory,
                    x.ObjectiveCondition}).ToList();
            }
            return View("MedicalConditions", record);
        }

        [HttpPost]
        public IActionResult MedicalConditionsAdd(Blood_parameters.Models.MedicalConditionsTable record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.MedicalConditions.Add(record.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return MedicalConditions(record);
            }
        }

        [HttpPost]
        public IActionResult MedicalConditionsUpdate(Blood_parameters.Models.MedicalConditionsTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        MedicalCondition? medicalCondition = db.MedicalConditions.Where(x => x.Id == record.update.Id).FirstOrDefault();
                        if (medicalCondition == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return MedicalConditions(record);
                        }
                        medicalCondition.PatientId = record.update.PatientId;
                        medicalCondition.DoctorId = record.update.DoctorId;
                        medicalCondition.Complaints = record.update.Complaints;
                        medicalCondition.MedicalHistory = record.update.MedicalHistory;
                        medicalCondition.ClinicalHistory = record.update.ClinicalHistory;
                        medicalCondition.ObjectiveCondition = record.update.ObjectiveCondition;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return MedicalConditions(record);
        }

        [HttpPost]
        public IActionResult MedicalConditionsFillUpdate(Blood_parameters.Models.MedicalConditionsTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        MedicalCondition? medicalCondition = db.MedicalConditions.Where(x => x.Id == record.fillUpdate).FirstOrDefault();
                        if (medicalCondition == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return MedicalConditions(record);
                        }
                        record.update ??= new();

                        record.update.Id = medicalCondition.Id;
                        record.update.PatientId = medicalCondition.PatientId;
                        record.update.DoctorId = medicalCondition.DoctorId;
                        record.update.Complaints = medicalCondition.Complaints;
                        record.update.MedicalHistory = medicalCondition.MedicalHistory;
                        record.update.ClinicalHistory = medicalCondition.ClinicalHistory;
                        record.update.ObjectiveCondition = medicalCondition.ObjectiveCondition;
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return MedicalConditions(record);
        }

        [HttpPost]
        public IActionResult MedicalConditionsDelete(Blood_parameters.Models.MedicalConditionsTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.MedicalConditions.Where(x => x.Id == record.id);
                        if (!query.Any())
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return MedicalConditions(record);
                        }
                        db.MedicalConditions.Remove(query.First());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return MedicalConditions(record);
        }


        [HttpGet]
        public IActionResult Medications(Blood_parameters.Models.MedicationsTable record)
        {
            if (record.add == null)
            {
                record.add = new Medication();
                record.add.ReceptionDuration = "тривало";
                record.add.PatientId = 1;
                record.add.MedicationName = "Еналаприл";
                record.add.MedicationDosage = "10 мг";
                record.add.MedicationFrequency = "2 рази на добу";
                record.add.DoctorId = 1;
            }

            ViewBag.Title = "Medications";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.Head = new List<string>() {
               "ID",
               "Тривалість прийому",
               "ID пацієнта",
               "Назва лікарського засобу",
               "Дозування",
               "Частота прийому",
               "ID лікаря"};
                ViewBag.Table = db.Medications.ToList().Select(x => new List<string?>() {
               x.Id.ToString(),
               x.ReceptionDuration,
               x.PatientId.ToString(),
               x.MedicationName,
               x.MedicationDosage,
               x.MedicationFrequency,
               x.DoctorId.ToString()}).ToList();
            }
            return View("Medications", record);
        }

        [HttpPost]
        public IActionResult MedicationsAdd(Blood_parameters.Models.MedicationsTable record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Medications.Add(record.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return Medications(record);
            }
        }

        [HttpPost]
        public IActionResult MedicationsUpdate(Blood_parameters.Models.MedicationsTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Medication? medication = db.Medications.Where(x => x.Id == record.update.Id).FirstOrDefault();
                        if (medication == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Medications(record);
                        }
                        medication.ReceptionDuration = record.update.ReceptionDuration;
                        medication.PatientId = record.update.PatientId;
                        medication.MedicationName = record.update.MedicationName;
                        medication.MedicationDosage = record.update.MedicationDosage;
                        medication.MedicationFrequency = record.update.MedicationFrequency;
                        medication.DoctorId = record.update.DoctorId;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Medications(record);
        }

        [HttpPost]
        public IActionResult MedicationsFillUpdate(Blood_parameters.Models.MedicationsTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Medication? medication = db.Medications.Where(x => x.Id == record.fillUpdate).FirstOrDefault();
                        if (medication == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Medications(record);
                        }
                        record.update ??= new();

                        record.update.Id = medication.Id;
                        record.update.ReceptionDuration = medication.ReceptionDuration;
                        record.update.PatientId = medication.PatientId;
                        record.update.MedicationName = medication.MedicationName;
                        record.update.MedicationDosage = medication.MedicationDosage;
                        record.update.MedicationFrequency = medication.MedicationFrequency;
                        record.update.DoctorId = medication.DoctorId;
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Medications(record);
        }

        [HttpPost]
        public IActionResult MedicationsDelete(Blood_parameters.Models.MedicationsTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.Medications.Where(x => x.Id == record.id);
                        if (!query.Any())
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Medications(record);
                        }
                        db.Medications.Remove(query.First());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Medications(record);
        }


        [HttpGet]
        public IActionResult PatientsParameters(Blood_parameters.Models.PatientsParameterTable record)
        {
            if (record.add == null)
            {
                record.add = new PatientsParameter();
                record.add.PatientId = 1;
                record.add.ParameterId = 1;
                record.add.Value = 1;
                record.add.DateOfCheck = DateOnly.FromDateTime(DateTime.Now);
            }

            ViewBag.Title = "PatientsParameters";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.Head = new List<string>() {
               "ID",
               "ID пацієнта",
               "ID параметру",
               "Значення",
               "Дата проведення аналізу"};
                ViewBag.Table = db.PatientsParameters.ToList().Select(x => new List<string?>() {
               x.Id.ToString(),
               x.PatientId.ToString(),
               x.ParameterId.ToString(),
               x.Value.ToString(),
               x.DateOfCheck?.ToShortDateString()}).ToList();
            }
            return View("PatientsParameters", record);
        }

        [HttpPost]
        public IActionResult PatientsParametersAdd(Blood_parameters.Models.PatientsParameterTable record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.PatientsParameters.Add(record.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return PatientsParameters(record);
            }
        }

        [HttpPost]
        public IActionResult PatientsParametersUpdate(Blood_parameters.Models.PatientsParameterTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        PatientsParameter? patientsParameter = db.PatientsParameters.Where(x => x.Id == record.update.Id).FirstOrDefault();
                        if (patientsParameter == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return PatientsParameters(record);
                        }
                        patientsParameter.PatientId = record.update.PatientId;
                        patientsParameter.ParameterId = record.update.ParameterId;
                        patientsParameter.Value = record.update.Value;
                        patientsParameter.DateOfCheck = record.update.DateOfCheck;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return PatientsParameters(record);
        }

        [HttpPost]
        public IActionResult PatientsParametersFillUpdate(Blood_parameters.Models.PatientsParameterTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        PatientsParameter? patientsParameter = db.PatientsParameters.Where(x => x.Id == record.fillUpdate).FirstOrDefault();
                        if (patientsParameter == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return PatientsParameters(record);
                        }
                        record.update ??= new();

                        record.update.Id = patientsParameter.Id;
                        record.update.PatientId = patientsParameter.PatientId;
                        record.update.ParameterId = patientsParameter.ParameterId;
                        record.update.Value = patientsParameter.Value;
                        record.update.DateOfCheck = patientsParameter.DateOfCheck;
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return PatientsParameters(record);
        }

        [HttpPost]
        public IActionResult PatientsParametersDelete(Blood_parameters.Models.PatientsParameterTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.PatientsParameters.Where(x => x.Id == record.id);
                        if (!query.Any())
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return PatientsParameters(record);
                        }
                        db.PatientsParameters.Remove(query.First());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return PatientsParameters(record);
        }


        [HttpGet]
        public IActionResult Parameters(Blood_parameters.Models.ParameterTable record)
        {
            if (record.add == null)
            {
                record.add = new Parameter();
                record.add.Name = "Гемоглобін";
            }
            ViewBag.Title = "Parameters";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.Head = new List<string>() {
               "ID",
               "Назва"};
                ViewBag.Table = db.Parameters.ToList().Select(x => new List<string?>() {
               x.Id.ToString(),
               x.Name}).ToList();
            }
            return View("Parameters", record);
        }

        [HttpPost]
        public IActionResult ParametersAdd(Blood_parameters.Models.ParameterTable record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Parameters.Add(record.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return Parameters(record);
            }
        }

        [HttpPost]
        public IActionResult ParametersUpdate(Blood_parameters.Models.ParameterTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Parameter? parameter = db.Parameters.Where(x => x.Id == record.update.Id).FirstOrDefault();
                        if (parameter == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Parameters(record);
                        }
                        parameter.Name = record.update.Name;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Parameters(record);
        }

        [HttpPost]
        public IActionResult ParametersFillUpdate(Blood_parameters.Models.ParameterTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Parameter? parameter = db.Parameters.Where(x => x.Id == record.fillUpdate).FirstOrDefault();
                        if (parameter == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Parameters(record);
                        }
                        record.update ??= new();

                        record.update.Id = parameter.Id;
                        record.update.Name = parameter.Name;
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Parameters(record);
        }

        [HttpPost]
        public IActionResult ParametersDelete(Blood_parameters.Models.ParameterTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.Parameters.Where(x => x.Id == record.id);
                        if (!query.Any())
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Parameters(record);
                        }
                        db.Parameters.Remove(query.First());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Parameters(record);
        }


        [HttpGet]
        public IActionResult Norms(Blood_parameters.Models.NormTable record)
        {
            if (record.add == null)
            {
                record.add = new Norm();
                record.add.ParameterId = 1;
                record.add.Gender = "ч";
                record.add.MinValue = 0;
                record.add.MaxValue = 1;
            }

            ViewBag.Title = "Norms";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.Head = new List<string>() {
               "ID",
               "ID параметру",
               "Стать",
               "Мінімальне значення",
               "Максимальне значення"};
                ViewBag.Table = db.Norms.ToList().Select(x => new List<string?>() {
               x.Id.ToString(),
               x.ParameterId.ToString(),
               x.Gender.ToString(),
               x.MinValue.ToString(),
               x.MaxValue.ToString()}).ToList();
            }
            return View("Norms", record);
        }

        [HttpPost]
        public IActionResult NormsAdd(Blood_parameters.Models.NormTable record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Norms.Add(record.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return Norms(record);
            }
        }

        [HttpPost]
        public IActionResult NormsUpdate(Blood_parameters.Models.NormTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Norm? norm = db.Norms.Where(x => x.Id == record.update.Id).FirstOrDefault();
                        if (norm == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Norms(record);
                        }
                        norm.ParameterId = record.update.ParameterId;
                        norm.Gender = record.update.Gender;
                        norm.MinValue = record.update.MinValue;
                        norm.MaxValue = record.update.MaxValue;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Norms(record);
        }

        [HttpPost]
        public IActionResult NormsFillUpdate(Blood_parameters.Models.NormTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Norm? norm = db.Norms.Where(x => x.Id == record.fillUpdate).FirstOrDefault();
                        if (norm == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Norms(record);
                        }
                        record.update ??= new();

                        record.update.Id = norm.Id;
                        record.update.ParameterId = norm.ParameterId;
                        record.update.Gender = norm.Gender;
                        record.update.MinValue = norm.MinValue;
                        record.update.MaxValue = norm.MaxValue;
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Norms(record);
        }

        [HttpPost]
        public IActionResult NormsDelete(Blood_parameters.Models.NormTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.Norms.Where(x => x.Id == record.id);
                        if (!query.Any())
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Norms(record);
                        }
                        db.Norms.Remove(query.First());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Norms(record);
        }


        [HttpGet]
        public IActionResult Users(Blood_parameters.Models.UserTable record)
        {
            if (record.add == null)
            {
                record.add = new User();
                record.add.Email = "ivanov.ivan@gmail.com";
                record.add.Password = "MyPassword1";
                record.add.PatientId = 1;
                record.add.DoctorId = 1;
                record.add.RoleId = 1;
            }

            ViewBag.Title = "Users";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.Head = new List<string>() {
               "ID",
               "Email",
               "Пароль",
               "ID пацієнта",
               "ID лікаря",
               "ID ролі"};
                ViewBag.Table = db.Users.ToList().Select(x => new List<string?>() {
               x.Id.ToString(),
               x.Email,
               x.Password,
               x.PatientId.ToString(),
               x.DoctorId.ToString(),
               x.RoleId.ToString()}).ToList();
            }
            return View("Users", record);
        }

        [HttpPost]
        public IActionResult UsersAdd(Blood_parameters.Models.UserTable record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Users.Add(record.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return Users(record);
            }
        }

        [HttpPost]
        public IActionResult UsersUpdate(Blood_parameters.Models.UserTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        User? user = db.Users.Where(x => x.Id == record.update.Id).FirstOrDefault();
                        if (user == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Users(record);
                        }
                        user.Email = record.update.Email;
                        user.Password = record.update.Password;
                        user.PatientId = record.update.PatientId;
                        user.DoctorId = record.update.DoctorId;
                        user.RoleId = record.update.RoleId;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Users(record);
        }

        [HttpPost]
        public IActionResult UsersFillUpdate(Blood_parameters.Models.UserTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        User? user = db.Users.Where(x => x.Id == record.fillUpdate).FirstOrDefault();
                        if (user == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Users(record);
                        }
                        record.update ??= new();

                        record.update.Id = user.Id;
                        record.update.Email = user.Email;
                        record.update.Password = user.Password;
                        record.update.PatientId = user.PatientId;
                        record.update.DoctorId = user.DoctorId;
                        record.update.RoleId = user.RoleId;
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Users(record);
        }

        [HttpPost]
        public IActionResult UsersDelete(Blood_parameters.Models.UserTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.Users.Where(x => x.Id == record.id);
                        if (!query.Any())
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Users(record);
                        }
                        db.Users.Remove(query.First());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Users(record);
        }


        [HttpGet]
        public IActionResult Roles(Blood_parameters.Models.RoleTable record)
        {
            if (record.add == null)
            {
                record.add = new Role();
                record.add.Name = "супер адміністратор";
            }

            ViewBag.Title = "Roles";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.Head = new List<string>() {
               "ID",
               "Назва"};
                ViewBag.Table = db.Roles.ToList().Select(x => new List<string?>() {
               x.Id.ToString(),
               x.Name}).ToList();
            }
            return View("Roles", record);
        }

        [HttpPost]
        public IActionResult RolesAdd(Blood_parameters.Models.RoleTable record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Roles.Add(record.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return Roles(record);
            }
        }

        [HttpPost]
        public IActionResult RolesUpdate(Blood_parameters.Models.RoleTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Role? role = db.Roles.Where(x => x.Id == record.update.Id).FirstOrDefault();
                        if (role == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Roles(record);
                        }
                        role.Name = record.update.Name;

                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Roles(record);
        }

        [HttpPost]
        public IActionResult RolesFillUpdate(Blood_parameters.Models.RoleTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        Role? role = db.Roles.Where(x => x.Id == record.fillUpdate).FirstOrDefault();
                        if (role == null)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Roles(record);
                        }
                        record.update ??= new();

                        record.update.Id = role.Id;
                        record.update.Name = role.Name;
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Roles(record);
        }

        [HttpPost]
        public IActionResult RolesDelete(Blood_parameters.Models.RoleTable record)
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.Roles.Where(x => x.Id == record.id);
                        if (!query.Any())
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Roles(record);
                        }
                        db.Roles.Remove(query.First());
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }
            return Roles(record);
        }


        [HttpGet]
        public IActionResult RegisterPatient(Blood_parameters.Models.RegisterPatient registerPatient)
        {
            if (registerPatient.patient == null&& registerPatient.user == null)
            {
                registerPatient.patient = new Patient();
                registerPatient.patient.Surname = "Іванеко";
                registerPatient.patient.Name = "Іван";
                registerPatient.patient.Patronymic = "Іванович";
                registerPatient.patient.Dob = new DateOnly(1999, 1, 1);
                registerPatient.patient.Gender = "ч";
                registerPatient.patient.Phone = "+380961321103";
                registerPatient.patient.Address = "м. Кривий Ріг вул. Поштова 3";
                registerPatient.patient.EmergencyContactSurname = "Іванеко";
                registerPatient.patient.EmergencyContactName = "Іван";
                registerPatient.patient.EmergencyContactPatronymic = "Петрович";
                registerPatient.patient.EmergencyContactPhone = "+380973908070";

                registerPatient.user = new User();
                registerPatient.user.Email = "ivanov.ivan@gmail.com";
                registerPatient.user.Password = "MyPassword1";
                ModelState.Clear();
            }
            ViewBag.Title = "RegisterPatient";

            return View("RegisterPatient", registerPatient);
        }

        [HttpPost]
        public IActionResult RegisterPatientPost(Blood_parameters.Models.RegisterPatient registerPatient)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        int rows = db.Database.ExecuteSql($"RegisterPatient {registerPatient.patient.Surname}, {registerPatient.patient.Name}, {registerPatient.patient.Patronymic}, {registerPatient.patient.Dob}, {registerPatient.patient.Gender}, {registerPatient.patient.Phone}, {registerPatient.patient.Address}, {registerPatient.patient.EmergencyContactSurname}, {registerPatient.patient.EmergencyContactName}, {registerPatient.patient.EmergencyContactPatronymic}, {registerPatient.patient.EmergencyContactPhone}, {registerPatient.patient.DoctorId}, {registerPatient.user.Email}, {registerPatient.user.Password}");
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }
            }
            return RegisterPatient(new RegisterPatient());
        }


        [HttpGet]
        public IActionResult RegisterDoctor(Blood_parameters.Models.RegisterDoctor registerDoctor)
        {
            if (registerDoctor.doctor == null&& registerDoctor.user == null)
            {
                registerDoctor.doctor = new Doctor();
                registerDoctor.doctor.YearsOfExperience = 10;
                registerDoctor.doctor.RecordNumber = "REC-101";
                registerDoctor.doctor.Surname = "Іванеко";
                registerDoctor.doctor.Name = "Іван";
                registerDoctor.doctor.Patronymic = "Іванович";
                registerDoctor.doctor.Qualifications = "Сертифікований терапевт";
                registerDoctor.doctor.CurrentPosition = "Терапевт";
                registerDoctor.doctor.Education = "Медичний коледж";
                registerDoctor.doctor.ContactPhone = "+380961321103";

                registerDoctor.user = new User();
                registerDoctor.user.Email = "ivanov.ivan@gmail.com";
                registerDoctor.user.Password = "MyPassword1";
                ModelState.Clear();
            }
            ViewBag.Title = "RegisterDoctor";

            return View("RegisterDoctor", registerDoctor);
        }

        [HttpPost]
        public IActionResult RegisterDoctorPost(Blood_parameters.Models.RegisterDoctor registerDoctor)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        int rows = db.Database.ExecuteSql($"RegisterDoctor {registerDoctor.doctor.YearsOfExperience}, {registerDoctor.doctor.RecordNumber}, {registerDoctor.doctor.Surname}, {registerDoctor.doctor.Name}, {registerDoctor.doctor.Patronymic}, {registerDoctor.doctor.Qualifications}, {registerDoctor.doctor.CurrentPosition}, {registerDoctor.doctor.Education}, {registerDoctor.doctor.ContactPhone},  {registerDoctor.user.Email}, {registerDoctor.user.Password}");
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }
            }
            return RegisterDoctor(new RegisterDoctor());
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}