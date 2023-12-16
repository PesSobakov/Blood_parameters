using Blood_parameters.Models;
using Blood_parameters.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Claims;
using static Blood_parameters.Models.ClusteringKMean;

namespace Blood_parameters.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public DoctorController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Info(Blood_parameters.Models.DoctorAndPicture record)
        {
            ViewBag.Title = "Info";

            using (BloodParametersContext db = new BloodParametersContext())
            {
                string? email = ControllerContext.HttpContext.User.Claims
                       .Where(x => x.Type == ClaimsIdentity.DefaultNameClaimType)
                       .Select(x => x.Value).FirstOrDefault();
                if (email == null)
                {
                    ViewBag.FatalError = "Немає запису лікаря, пов'язаного з вашим акаунтом";
                    return View("Info");
                }
                int? id = db.Users
                   .Where(x => x.Email == email)
                   .Select(x => x.DoctorId).FirstOrDefault();
                if (id == null)
                {
                    ViewBag.FatalError = "Немає запису лікаря, пов'язаного з вашим акаунтом";
                    return View("Info");
                }

                ViewBag.Head = new List<string>() {
                    "years_of_experience",
                    "record_number",
                    "Surname",
                    "name",
                    "Patronymic",
                    "qualifications",
                    "current_position",
                    "education",
                    "contact_phone"};
                ViewBag.Table = db.Doctors.Where(x => x.Id == id).ToList()
                    .Select(x => new List<string?>() {
                        x?.YearsOfExperience?.ToString(),
                        x?.RecordNumber,
                        x?.Surname,
                        x?.Name,
                        x?.Patronymic,
                        x?.Qualifications,
                        x?.CurrentPosition,
                        x?.Education,
                        x?.ContactPhone}).ToList();

                ViewBag.Doctor = db.Doctors.Where(x => x.Id == id).Include(x => x.Patients).ThenInclude(x => x.Users).ToList().FirstOrDefault();
                ViewBag.Email = email;

                if (record.doctor?.Id == 0)
                {
                    record.doctor = db.Doctors.Where(x => x.Id == id).FirstOrDefault();
                }
            }
            return View("Info", record);
        }

        [HttpPost]
        public IActionResult InfoUpdate(Blood_parameters.Models.DoctorAndPicture record)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        string? email = ControllerContext.HttpContext.User.Claims
                            .Where(x => x.Type == ClaimsIdentity.DefaultNameClaimType)
                            .Select(x => x.Value).FirstOrDefault();
                        if (email == null)
                        {
                            ViewBag.FatalError = "Немає запису лікаря, пов'язаного з вашим акаунтом";
                            return View("Info");
                        }
                        int? id = db.Users
                           .Where(x => x.Email == email)
                           .Select(x => x.DoctorId).FirstOrDefault();
                        if (id == null)
                        {
                            ViewBag.FatalError = "Немає запису лікаря, пов'язаного з вашим акаунтом";
                            return View("Info");
                        }
                        int rows = db.Database.ExecuteSql($"updateDoctor {id}, {record.doctor.YearsOfExperience}, {record.doctor.RecordNumber}, {record.doctor.Surname}, {record.doctor.Name}, {record.doctor.Patronymic}, {record.doctor.Qualifications}, {record.doctor.CurrentPosition}, {record.doctor.Education}, {record.doctor.ContactPhone}");
                        db.SaveChanges();
                    }
                    else
                    {
                        ViewBag.Update = true;
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                    ViewBag.Update = true;
                }

                return Info(record);
            }
        }

        [HttpPost]
        public IActionResult PictureUpdate(Blood_parameters.Models.DoctorAndPicture record)
        {
            if (record.picture == null)
            {
                ViewBag.Error = "Файл не вибраний";
                ViewBag.Update = true;
                return Info(record);
            }
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        string? email = ControllerContext.HttpContext.User.Claims
                            .Where(x => x.Type == ClaimsIdentity.DefaultNameClaimType)
                            .Select(x => x.Value).FirstOrDefault();
                        if (email == null)
                        {
                            ViewBag.FatalError = "Немає запису лікаря, пов'язаного з вашим акаунтом";
                            return View("Info");
                        }
                        int? id = db.Users
                           .Where(x => x.Email == email)
                           .Select(x => x.DoctorId).FirstOrDefault();
                        if (id == null)
                        {
                            ViewBag.FatalError = "Немає запису лікаря, пов'язаного з вашим акаунтом";
                            return View("Info");
                        }

                        Doctor doctor = db.Doctors.Where(x => x.Id == id).ToList().FirstOrDefault();



                        using var fileStream = record.picture.OpenReadStream();
                        byte[] bytes = new byte[(int)record.picture.Length];
                        fileStream.Read(bytes, 0, (int)record.picture.Length);

                        var png = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
                        if (bytes.Length < png.Length)
                        {
                            ViewBag.Error = "Файл повинен бути в форматі png";
                            ViewBag.Update = true;
                            return Info(record);
                        }
                        for (int i = 0; i < png.Length; i++)
                        {
                            if (bytes[i] != png[i])
                            {
                                ViewBag.Error = "Файл повинен бути в форматі png";
                                ViewBag.Update = true;
                                return Info(record);
                            }
                        }

                        doctor.Photo = bytes;
                        db.SaveChanges();
                    }
                    else
                    {
                        ViewBag.Update = true;
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                    ViewBag.Update = true;
                }

                return Info(record);
            }
        }


        [HttpGet]
        public IActionResult Patients()
        {
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
            return View();
        }

        public IActionResult MyPatients()
        {
            ViewBag.Title = "Patients";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                string? email = ControllerContext.HttpContext.User.Claims
                      .Where(x => x.Type == ClaimsIdentity.DefaultNameClaimType)
                      .Select(x => x.Value).FirstOrDefault();
                if (email == null)
                {
                    ViewBag.FatalError = "Немає запису лікаря, пов'язаного з вашим акаунтом";
                    return View("MyPatients");
                }
                int? id = db.Users
                   .Where(x => x.Email == email)
                   .Select(x => x.DoctorId).FirstOrDefault();
                if (id == null)
                {
                    ViewBag.FatalError = "Немає запису лікаря, пов'язаного з вашим акаунтом";
                    return View("MyPatients");
                }

                List<Patient> patients = db.Patients.Where(x => x.DoctorId == id).Include(x => x.Doctor).ToList();
                ViewBag.Patients = patients;

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
            return View();
        }

        public IActionResult AddAnalysisByid(int id)
        {
            Analysis analysis = new Analysis();
            analysis.dateOfCheck ??= DateOnly.FromDateTime( DateTime.Now);
            analysis.patient_id = id;

            return View("AddAnalysis", analysis);
        }
        public IActionResult ParametersByid(int id)
        {
            ViewBag.Id = id;

            return Parameters();
        }
        public IActionResult MedicationsByid(int id)
        {
            ViewBag.Id = id;
            MedicationsTable medications = new MedicationsTable();
            medications.id = id;
            medications.add = new Medication();
            medications.add.PatientId = id;

            return Medications(medications);
        }
        public IActionResult MedicalConditionsByid(int id)
        {
            ViewBag.Id = id;
            MedicalConditionsTable medicalConditions = new MedicalConditionsTable();
            medicalConditions.id = id;
            medicalConditions.add = new MedicalCondition();
            medicalConditions.add.PatientId = id;

            return MedicalConditions(medicalConditions);
        }
        public IActionResult AppointmentsByid(int id)
        {
            ViewBag.Id = id;
            AppointmentsTable appointments = new AppointmentsTable();
            appointments.id = id;
            appointments.add = new Appointment();
            appointments.add.PatientId = id;

            return Appointments(appointments);
        }

        [HttpGet]
        public IActionResult AddAnalysis(Blood_parameters.Models.Analysis analysis)
        {
            if (analysis.generalBloodTest == null)
            {
                analysis.generalBloodTest = new  Analysis.GeneralBloodTest();
                analysis.generalBloodTest.hemoglobin_count = 140;
                analysis.generalBloodTest.red_blood_cells_count = 4.5;
                analysis.generalBloodTest.white_blood_cells_count = 6;
                analysis.generalBloodTest.erythrocyte_sedimentation_rate_count = 7;
                analysis.generalBloodTest.eosinophil_count = 3;
                analysis.generalBloodTest.band_neutrophils_coun = 4;
                analysis.generalBloodTest.segmented_neutrophils_count = 60;
                analysis.generalBloodTest.lymphocytes_count = 25;
                analysis.generalBloodTest.monocytes_count = 7;
            }
          if (analysis.bloodGlucose == null)
            {
                analysis.bloodGlucose = new  Analysis.BloodGlucose();
                analysis.bloodGlucose.BloodGlucoseLevel = 4;
            }
            if (analysis.bloodCholesterol == null)
            {
                analysis.bloodCholesterol = new Analysis.BloodCholesterol();
                analysis.bloodCholesterol.BloodCholesterolLevel = 4;
            }
            if (analysis.biochemicalBloodAnalysis == null)
            {
                analysis.biochemicalBloodAnalysis = new  Analysis.BiochemicalBloodAnalysis();
                analysis.biochemicalBloodAnalysis.TotalBilirubin = 10;
                analysis.biochemicalBloodAnalysis.DirectBilirubin = 2;
                analysis.biochemicalBloodAnalysis.IndirectBilirubin = 8;
                analysis.biochemicalBloodAnalysis.AlanineAminotransferase = 25;
                analysis.biochemicalBloodAnalysis.AspartateAminotransferase = 20;
                analysis.biochemicalBloodAnalysis.CreatinineLevel = 90;
                analysis.biochemicalBloodAnalysis.UrineLevel = 5;
            }
            if (analysis.bloodPressure == null)
            {
                analysis.bloodPressure = new  Analysis.BloodPressure();
                analysis.bloodPressure.SystolicPressure = 120;
                analysis.bloodPressure.DiastolicPressure = 80;
                analysis.bloodPressure.PulseRate = 75;
            }

            ViewBag.Title = "Parameters";

            analysis.dateOfCheck ??= DateOnly.FromDateTime(DateTime.Now);

            return View("AddAnalysis", analysis);
        }

        [HttpPost]
        public IActionResult AddAnalysisGeneralBloodTest(Blood_parameters.Models.Analysis analysis)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.OpenTab = 0;
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Database.ExecuteSql($"InsertGeneralBloodTest {analysis.patient_id}, {analysis.dateOfCheck}, {analysis.generalBloodTest.hemoglobin_count}, {analysis.generalBloodTest.red_blood_cells_count}, {analysis.generalBloodTest.white_blood_cells_count}, {analysis.generalBloodTest.erythrocyte_sedimentation_rate_count}, {analysis.generalBloodTest.eosinophil_count}, {analysis.generalBloodTest.band_neutrophils_coun}, {analysis.generalBloodTest.segmented_neutrophils_count}, {analysis.generalBloodTest.lymphocytes_count}, {analysis.generalBloodTest.monocytes_count}");
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }
                return AddAnalysis(analysis);
            }
        }

        [HttpPost]
        public IActionResult AddAnalysisBloodGlucose(Blood_parameters.Models.Analysis analysis)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.OpenTab = 1;
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Database.ExecuteSql($"InsertBloodGlucose {analysis.patient_id}, {analysis.dateOfCheck}, {analysis.bloodGlucose.BloodGlucoseLevel}");
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }
                return AddAnalysis(analysis);
            }
        }

        [HttpPost]
        public IActionResult AddAnalysisBloodCholesterol(Blood_parameters.Models.Analysis analysis)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.OpenTab = 2;
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Database.ExecuteSql($"InsertBloodCholesterol {analysis.patient_id}, {analysis.dateOfCheck}, {analysis.bloodCholesterol.BloodCholesterolLevel}");
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }
                return AddAnalysis(analysis);
            }
        }

        [HttpPost]
        public IActionResult AddAnalysisBiochemicalBloodAnalysis(Blood_parameters.Models.Analysis analysis)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.OpenTab = 3;
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Database.ExecuteSql($"InsertBiochemicalBloodAnalysis {analysis.patient_id}, {analysis.dateOfCheck}, {analysis.biochemicalBloodAnalysis.TotalBilirubin}, {analysis.biochemicalBloodAnalysis.DirectBilirubin}, {analysis.biochemicalBloodAnalysis.IndirectBilirubin}, {analysis.biochemicalBloodAnalysis.AlanineAminotransferase}, {analysis.biochemicalBloodAnalysis.AspartateAminotransferase}, {analysis.biochemicalBloodAnalysis.CreatinineLevel}, {analysis.biochemicalBloodAnalysis.UrineLevel}");
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }
                return AddAnalysis(analysis);
            }
        }

        [HttpPost]
        public IActionResult AddAnalysisBloodPressure(Blood_parameters.Models.Analysis analysis)
        {
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.OpenTab = 4;
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Database.ExecuteSql($"InsertBloodPressure {analysis.patient_id}, {analysis.dateOfCheck}, {analysis.bloodPressure.SystolicPressure}, {analysis.bloodPressure.DiastolicPressure}, {analysis.bloodPressure.PulseRate}");
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }
                return AddAnalysis(analysis);
            }
        }

        [HttpGet]
        public IActionResult Parameters()
        {
            ViewBag.Title = "Parameters";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                ViewBag.Head = new List<string>() {
                    "ID пацієнта",
                    "Параметр",
                    "Значення",
                    "Дата проведення аналізу",
                    "Норма мінімум",
                    "Норма максимум",
                    "Відхилення від норми"};
                ViewBag.Table = db.PatientsParameters.Include(x => x.Parameter).ThenInclude(x => x.Norms).Include(x=>x.Patient).ToList()
                    .Select(x => new List<string?>() {
                        x?.PatientId.ToString(),
                        x?.Parameter?.Name,
                        x?.Value.ToString(),
                        x?.DateOfCheck?.ToShortDateString(),
                        x?.Parameter?.Norms.Where(y=>y.Gender==x.Patient?.Gender)?.FirstOrDefault()?.MinValue.ToString("0.###"),
                        x?.Parameter?.Norms.Where(y=>y.Gender==x.Patient?.Gender)?.FirstOrDefault()?.MaxValue.ToString("0.###"),
                        x?.Value<x?.Parameter?.Norms.Where(y=>y.Gender==x.Patient?.Gender)?.FirstOrDefault()?.MinValue?
                            (x?.Value - x?.Parameter?.Norms.Where(y=>y.Gender==x.Patient?.Gender)?.FirstOrDefault()?.MinValue)?.ToString("0.###")
                            :x?.Value>x?.Parameter?.Norms.Where(y=>y.Gender==x.Patient?.Gender)?.FirstOrDefault()?.MaxValue?
                            (x?.Value-x?.Parameter?.Norms.Where(y=>y.Gender==x.Patient?.Gender)?.FirstOrDefault()?.MaxValue)?.ToString("0.###")
                            :"0"??"0"}).ToList();
            }
            return View("Parameters");
        }

        [HttpGet]
        public IActionResult Medications(Blood_parameters.Models.MedicationsTable medications)
        {
            if (medications.add == null)
            {
                medications.add = new  Medication();
                medications.add.ReceptionDuration = "тривало";
                medications.add.PatientId = 1;
                medications.add.MedicationName = "Еналаприл";
                medications.add.MedicationDosage = "10 мг";
                medications.add.MedicationFrequency = "2 рази на добу";
                medications.add.DoctorId = 1;
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
                ViewBag.Table = db.Medications.ToList()
                    .Select(x => new List<string?>() {
                        x?.Id.ToString(),
                        x?.ReceptionDuration,
                        x?.PatientId.ToString(),
                        x?.MedicationName,
                        x?.MedicationDosage,
                        x?.MedicationFrequency,
                        x?.DoctorId.ToString()}).ToList();
            }
            return View("Medications", medications);
        }

        [HttpPost]
        public IActionResult MedicationsAdd(Blood_parameters.Models.MedicationsTable medications)
        {
            ViewBag.OpenTab = 1;
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Medications.Add(medications.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return Medications(medications);
            }
        }

        [HttpPost]
        public IActionResult MedicationsDelete(Blood_parameters.Models.MedicationsTable medications)
        {
            ViewBag.OpenTab = 2;
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.Medications.Where(x => x.Id == medications.id);
                        if (query.Count() == 0)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Medications(medications);
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
            return Medications(medications);
        }


        [HttpGet]
        public IActionResult MedicalConditions(Blood_parameters.Models.MedicalConditionsTable medicalConditions)
        {
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
                ViewBag.Table = db.MedicalConditions.ToList()
                    .Select(x => new List<string?>() {
                        x?.Id.ToString(),
                        x?.PatientId.ToString(),
                        x?.DoctorId.ToString(),
                        x?.Complaints,
                        x?.MedicalHistory,
                        x?.ClinicalHistory,
                        x?.ObjectiveCondition}).ToList();
            }
            return View("MedicalConditions", medicalConditions);
        }

        [HttpPost]
        public IActionResult MedicalConditionsAdd(Blood_parameters.Models.MedicalConditionsTable medicalConditions)
        {
            ViewBag.OpenTab = 1;
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.MedicalConditions.Add(medicalConditions.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return MedicalConditions(medicalConditions);
            }
        }

        [HttpPost]
        public IActionResult MedicalConditionsDelete(Blood_parameters.Models.MedicalConditionsTable medicalConditions)
        {
            ViewBag.Title = "MedicalConditions";
            ViewBag.OpenTab = 2;
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.MedicalConditions.Where(x => x.Id == medicalConditions.id);
                        if (query.Count() == 0)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return MedicalConditions(medicalConditions);
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
            return MedicalConditions(medicalConditions);
        }


        [HttpGet]
        public IActionResult Appointments(Blood_parameters.Models.AppointmentsTable appointments)
        {
            if (appointments.add == null)
            {
                appointments.add = new Appointment();
                appointments.add.TreatmentDate = DateOnly.FromDateTime(DateTime.Now);
                appointments.add.TreatmentTime = TimeOnly.FromDateTime(DateTime.Now);
                appointments.add.RecordNumber = "000101\\2023";
                appointments.add.PatientId = 1;
                appointments.add.DoctorId = 1;
                appointments.add.StartDate = DateOnly.FromDateTime(DateTime.Now);
                appointments.add.EndDate = DateOnly.FromDateTime(DateTime.Now);
                appointments.add.Diagnosis = "Текст діагнозу";
                appointments.add.Treatment = "Текст лікування";
                appointments.add.TreatmentAndWorkRecommendations = "Текст Медичних показаннь";
                appointments.add.Recommended = "Текст рекомендації";
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
                ViewBag.Table = db.Appointments.ToList()
                    .Select(x => new List<string?>() {
                        x?.Id.ToString(),
                        x?.TreatmentDate.ToShortDateString(),
                        x?.TreatmentTime.ToString(),
                        x?.RecordNumber,
                        x?.PatientId.ToString(),
                        x?.DoctorId.ToString(),
                        x?.StartDate.ToShortDateString(),
                        x?.EndDate.ToShortDateString(),
                        x?.Diagnosis,
                        x?.Treatment,
                        x?.TreatmentAndWorkRecommendations,
                        x?.Recommended}).ToList();
            }
            return View("Appointments", appointments);
        }

        public IActionResult AppointmentsAdd(Blood_parameters.Models.AppointmentsTable appointments)
        {
            ViewBag.OpenTab = 1;
            using (BloodParametersContext db = new BloodParametersContext())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Appointments.Add(appointments.add);
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
                }

                return Appointments(appointments);
            }
        }

        [HttpPost]
        public IActionResult AppointmentsDelete(Blood_parameters.Models.AppointmentsTable appointments)
        {
            ViewBag.Title = "MedicalConditions";
            ViewBag.OpenTab = 2;
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                    if (ModelState.IsValid)
                    {
                        var query = db.Appointments.Where(x => x.Id == appointments.id);
                        if (query.Count() == 0)
                        {
                            ViewBag.Error = "Запис з цим ID не існує";
                            return Appointments(appointments);
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
            return Appointments(appointments);
        }

        [HttpGet]
        public IActionResult RegisterPatient(RegisterPatient registerPatient)
        {
            if (registerPatient.patient == null && registerPatient.user == null)
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
            return RegisterPatient(registerPatient);
        }

        [HttpGet]
        public IActionResult GetSimillarDiagnoses()
        {
            ViewBag.Title = "GetSimillarDiagnoses";

            ViewBag.Head ??= new List<string>() {
                           "ID призначення",
                           "Співпадіння"};
            ViewBag.Table ??= new List<List<string>>();


            return View("GetSimillarDiagnoses");
        }

        [HttpPost]
        public IActionResult GetSimillarDiagnoses(Int32Container number)
        {
            ViewBag.Title = "GetSimillarDiagnoses";
            try
            {
                if (ModelState.IsValid)
                {
                    using (BloodParametersContext db = new BloodParametersContext())
                    {
                        List<getSimillarDiagnoses> diagnoses = db.getSimillarDiagnoses.FromSql($"getSimillarDiagnoses {number.id}").ToList();
                        List<Appointment> appointments = db.Appointments.ToList();
                        ViewBag.Head = new List<string>() {
                           "ID призначення",
                           "Співпадіння",
                           "ID пацієнта",
                           "Діагноз" };
                        ViewBag.Table = diagnoses
                            .Select(x => new List<string?>() {
                            x?.appointment_id.ToString(),
                            x?.match_count.ToString(),
                            appointments.Where(y=> y.Id==x?.appointment_id).FirstOrDefault()?.PatientId.ToString(),
                            appointments.Where(y=> y.Id==x?.appointment_id).FirstOrDefault()?.Diagnosis}).ToList();
                    }

                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message + "\n" + e.InnerException?.Message;
            }

            return GetSimillarDiagnoses();
        }



        [HttpGet]
        public IActionResult ClusteringGeneralBloodTest()
        {
            ViewBag.Title = "Clustering";
            ViewBag.Controller = "ClusteringGeneralBloodTestK";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                var parameters = db.PatientsParameters
                    .Include(x => x.Parameter)
                    .Where(x => new string[] { "Гемоглобін", "Еритроцити",  "Лейкоцити", "ШОЕ", "Еозинофіли", "Паличкоядерні", "Сегментоядерні", "Лімфоцити", "Моноцити" }.Contains(x.Parameter.Name))
                    .ToList()
                    .GroupBy(x => new { x.PatientId, x.DateOfCheck })
                    .Select(x => new
                    {
                        patient = x.Key.PatientId,
                        parameters = new List<double?>
                        {
                                x.FirstOrDefault(x => x.Parameter.Name == "Гемоглобін")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Еритроцити")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Лейкоцити")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "ШОЕ")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Еозинофіли")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Паличкоядерні")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Сегментоядерні")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Лімфоцити")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Моноцити")?.Value
                        }
                    })
                    .Where(x => x.parameters.All(x => x != null))
                    .Select(x => new
                    {
                        patient = x.patient,
                        parameters = x.parameters.ConvertAll(x => (double)x!)
                    })
                    .ToList();

                if (parameters.Count == 0)
                {
                    ViewBag.Error = "Не знайдено жодного набору аналізів";
                    ViewBag.FatalError = true;
                }
                else
                {
                    ClusteringKMean.ClusterizationResult clusterizationResult = ClusteringKMean.Clusterize(parameters.Select(x => x.parameters).ToList());
                    List<int> clusterIndexes = clusterizationResult.clusters;

                    ViewBag.Elbow = clusterizationResult.elbow;

                    List<List<double>> centroids = new List<List<double>>();
                    for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                    {
                        List<double> averages = new List<double>();
                        for (int j = 0; j < parameters[0].parameters.Count; j++)
                        {
                            double avg = 0;
                            for (int k = 0; k < parameters.Count; k++)
                            {
                                if (clusterIndexes[k] == i)
                                {
                                    avg += parameters[k].parameters[j];
                                }
                            }
                            avg /= clusterIndexes.Where(x => x == i).Count();
                            averages.Add(avg);
                        }
                        centroids.Add(averages);

                    }

                    ViewBag.Centroids = centroids;
                    ViewBag.CentroidsLabels = new List<string>() { "Гемоглобін", "Еритроцити",  "Лейкоцити", "ШОЕ", "Еозинофіли", "Паличкоядерні", "Сегментоядерні", "Лімфоцити", "Моноцити" };


                    //List<List<string?>> result = new List<List<string?>>();
                    //for (int i = 0; i < clusterIndexes.Count; i++)
                    //{
                    //    List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                    //    for (int j = 0; j < parameters[i].parameters.Count; j++)
                    //    {
                    //        temp.Add(parameters[i].parameters[j].ToString());
                    //    }
                    //    result.Add(temp);
                    //}
                    List<List<string?>> result = new List<List<string?>>();
                    for (int i = 0; i < clusterIndexes.Count; i++)
                    {
                        List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                        for (int j = 0; j < clusterizationResult.normalizedInput[i].Count; j++)
                        {
                            temp.Add(clusterizationResult.normalizedInput[i][j].ToString("0.###"));
                        }
                        result.Add(temp);
                    }

                    ViewBag.Head = new List<string>() {
                           "Номер кластеру",
                           "ID пацієнта",
                           "Гемоглобін",
                           "Еритроцити",
                            "Лейкоцити",
                          "ШОЕ",
                           "Еозинофіли",
                           "Паличкоядерні",
                           "Сегментоядерні",
                           "Лімфоцити",
                           "Моноцити" };
                    ViewBag.Table = result;
                }
            }

            return View("Clustering");
        }

        [HttpPost]
        public IActionResult ClusteringGeneralBloodTestK(Int32Container number)
        {
            ViewBag.Title = "Clustering";
            ViewBag.Controller = "ClusteringGeneralBloodTestK";
            if (ModelState.IsValid)
            {
                if (number.id < 1)
                {
                    ViewBag.Error = "кількість кластерів не може бути менше 1";
                    ViewBag.FatalError = true;
                }
                else
                {
                    using (BloodParametersContext db = new BloodParametersContext())
                    {
                        var parameters = db.PatientsParameters
                            .Include(x => x.Parameter)
                            .Where(x => new string[] { "Гемоглобін", "Еритроцити",  "Лейкоцити", "ШОЕ", "Еозинофіли", "Паличкоядерні", "Сегментоядерні", "Лімфоцити", "Моноцити" }.Contains(x.Parameter.Name))
                            .ToList()
                            .GroupBy(x => new { x.PatientId, x.DateOfCheck })
                            .Select(x => new
                            {
                                patient = x.Key.PatientId,
                                parameters = new List<double?>
                                {
                                x.FirstOrDefault(x => x.Parameter.Name == "Гемоглобін")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Еритроцити")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Лейкоцити")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "ШОЕ")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Еозинофіли")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Паличкоядерні")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Сегментоядерні")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Лімфоцити")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Моноцити")?.Value
                                }
                            })
                            .Where(x => x.parameters.All(x => x != null))
                            .Select(x => new
                            {
                                patient = x.patient,
                                parameters = x.parameters.ConvertAll(x => (double)x!)
                            })
                            .ToList();

                        if (parameters.Count == 0)
                        {
                            ViewBag.Error = "Не знайдено жодного набору аналізів";
                            ViewBag.FatalError = true;
                        }
                        else
                        {
                            ClusteringKMean.ClusterizationResult clusterizationResult = ClusteringKMean.Clusterize(parameters.Select(x => x.parameters).ToList(), number.id);
                            List<int> clusterIndexes = clusterizationResult.clusters;

                            List<List<double>> centroids = new List<List<double>>();
                            for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                            {
                                List<double> averages = new List<double>();
                                for (int j = 0; j < parameters[0].parameters.Count; j++)
                                {
                                    double avg = 0;
                                    for (int k = 0; k < parameters.Count; k++)
                                    {
                                        if (clusterIndexes[k] == i)
                                        {
                                            avg += parameters[k].parameters[j];
                                        }
                                    }
                                    avg /= clusterIndexes.Where(x => x == i).Count();
                                    averages.Add(avg);
                                }
                                centroids.Add(averages);

                            }

                            ViewBag.Centroids = centroids;
                            ViewBag.CentroidsLabels = new List<string>() { "Гемоглобін", "Еритроцити",  "Лейкоцити", "ШОЕ", "Еозинофіли", "Паличкоядерні", "Сегментоядерні", "Лімфоцити", "Моноцити" };


                            //List<List<string?>> result = new List<List<string?>>();
                            //for (int i = 0; i < clusterIndexes.Count; i++)
                            //{
                            //    List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                            //    for (int j = 0; j < parameters[i].parameters.Count; j++)
                            //    {
                            //        temp.Add(parameters[i].parameters[j].ToString());
                            //    }
                            //    result.Add(temp);
                            //}
                            List<List<string?>> result = new List<List<string?>>();
                            for (int i = 0; i < clusterIndexes.Count; i++)
                            {
                                List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                                for (int j = 0; j < clusterizationResult.normalizedInput[i].Count; j++)
                                {
                                    temp.Add(clusterizationResult.normalizedInput[i][j].ToString("0.###"));
                                }
                                result.Add(temp);
                            }

                            ViewBag.Head = new List<string>() {
                           "Номер кластеру",
                           "ID пацієнта",
                           "Гемоглобін",
                           "Еритроцити",
                           "Лейкоцити",
                           "ШОЕ",
                           "Еозинофіли",
                           "Паличкоядерні",
                           "Сегментоядерні",
                           "Лімфоцити",
                           "Моноцити" };
                            ViewBag.Table = result;
                        }
                    }
                }
            }
            else
            {
                ViewBag.FatalError = true;
            }
            return View("ClusteringK");
        }


        [HttpGet]
        public IActionResult ClusteringBloodGlucoseAndCholesterol()
        {
            ViewBag.Title = "Clustering";
            ViewBag.Controller = "ClusteringBloodGlucoseAndCholesterolK";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                var parameters = db.PatientsParameters
                    .Include(x => x.Parameter)
                    .Where(x => new string[] { "Глюкоза", "Холестерин" }.Contains(x.Parameter.Name))
                    .ToList()
                    .GroupBy(x => new { x.PatientId, x.DateOfCheck })
                    .Select(x => new
                    {
                        patient = x.Key.PatientId,
                        parameters = new List<double?>
                        {
                                x.FirstOrDefault(x => x.Parameter.Name == "Глюкоза")?.Value??0,
                                x.FirstOrDefault(x => x.Parameter.Name == "Холестерин")?.Value??0,
                        }
                    })
                    .Where(x => x.parameters.All(x => x != null))
                    .Select(x => new
                    {
                        patient = x.patient,
                        parameters = x.parameters.ConvertAll(x => (double)x!)
                    })
                    .ToList();

                if (parameters.Count == 0)
                {
                    ViewBag.Error = "Не знайдено жодного набору аналізів";
                    ViewBag.FatalError = true;
                }
                else
                {
                    ClusteringKMean.ClusterizationResult clusterizationResult = ClusteringKMean.Clusterize(parameters.Select(x => x.parameters).ToList());
                    List<int> clusterIndexes = clusterizationResult.clusters;

                    ViewBag.Elbow = clusterizationResult.elbow;

                    List<List<double>> centroids = new List<List<double>>();
                    for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                    {
                        List<double> averages = new List<double>();
                        for (int j = 0; j < parameters[0].parameters.Count; j++)
                        {
                            double avg = 0;
                            for (int k = 0; k < parameters.Count; k++)
                            {
                                if (clusterIndexes[k] == i)
                                {
                                    avg += parameters[k].parameters[j];
                                }
                            }
                            avg /= clusterIndexes.Where(x => x == i).Count();
                            averages.Add(avg);
                        }
                        centroids.Add(averages);

                    }

                    ViewBag.Centroids = centroids;
                    ViewBag.CentroidsLabels = new List<string>() { "Глюкоза", "Холестерин" };


                    //List<List<string?>> result = new List<List<string?>>();
                    //for (int i = 0; i < clusterIndexes.Count; i++)
                    //{
                    //    List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                    //    for (int j = 0; j < parameters[i].parameters.Count; j++)
                    //    {
                    //        temp.Add(parameters[i].parameters[j].ToString());
                    //    }
                    //    result.Add(temp);
                    //}
                    List<List<string?>> result = new List<List<string?>>();
                    for (int i = 0; i < clusterIndexes.Count; i++)
                    {
                        List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                        for (int j = 0; j < clusterizationResult.normalizedInput[i].Count; j++)
                        {
                            temp.Add(clusterizationResult.normalizedInput[i][j].ToString("0.###"));
                        }
                        result.Add(temp);
                    }

                    List<List<List<double>>> chartData = new List<List<List<double>>>();
                    for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                    {
                        chartData.Add(new List<List<double>>());
                    }
                    for (int i = 0; i < clusterIndexes.Count; i++)
                    {
                        List<double> temp = new List<double>();
                        for (int j = 0; j < parameters[i].parameters.Count; j++)
                        {
                            temp.Add(parameters[i].parameters[j]);
                        }
                        chartData[clusterIndexes[i]].Add(temp);
                    }


                    ViewBag.Head = new List<string>() {
                           "Номер кластеру",
                           "ID пацієнта",
                           "Глюкоза",
                            "Холестерин"};
                    ViewBag.Table = result;
                    ViewBag.Data = chartData;
                }
            }

            return View("ClusteringPop");
        }

        [HttpPost]
        public IActionResult ClusteringBloodGlucoseAndCholesterolK(Int32Container number)
        {
            ViewBag.Title = "Clustering";
            ViewBag.Controller = "ClusteringBloodGlucoseAndCholesterolK";
            if (ModelState.IsValid)
            {
                if (number.id < 1)
                {
                    ViewBag.Error = "кількість кластерів не може бути менше 1";
                    ViewBag.FatalError = true;
                }
                else
                {
                    using (BloodParametersContext db = new BloodParametersContext())
                    {
                        var parameters = db.PatientsParameters
                            .Include(x => x.Parameter)
                            .Where(x => new string[] { "Глюкоза" , "Холестерин"}.Contains(x.Parameter.Name))
                            .ToList()
                            .GroupBy(x => new { x.PatientId, x.DateOfCheck })
                            .Select(x => new
                            {
                                patient = x.Key.PatientId,
                                parameters = new List<double?>
                                {
                                x.FirstOrDefault(x => x.Parameter.Name == "Глюкоза")?.Value??0,
                                x.FirstOrDefault(x => x.Parameter.Name == "Холестерин")?.Value??0,
                                }
                            })
                            .Where(x => x.parameters.All(x => x != null))
                            .Select(x => new
                            {
                                patient = x.patient,
                                parameters = x.parameters.ConvertAll(x => (double)x!)
                            })
                            .ToList();

                        if (parameters.Count == 0)
                        {
                            ViewBag.Error = "Не знайдено жодного набору аналізів";
                            ViewBag.FatalError = true;
                        }
                        else
                        {
                            ClusteringKMean.ClusterizationResult clusterizationResult = ClusteringKMean.Clusterize(parameters.Select(x => x.parameters).ToList(), number.id);
                            List<int> clusterIndexes = clusterizationResult.clusters;
                        
                            List<List<double>> centroids = new List<List<double>>();
                            for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                            {
                                List<double> averages = new List<double>();
                                for (int j = 0; j < parameters[0].parameters.Count; j++)
                                {
                                    double avg = 0;
                                    for (int k = 0; k < parameters.Count; k++)
                                    {
                                        if (clusterIndexes[k] == i)
                                        {
                                            avg += parameters[k].parameters[j];
                                        }
                                    }
                                    avg /= clusterIndexes.Where(x => x == i).Count();
                                    averages.Add(avg);
                                }
                                centroids.Add(averages);

                            }

                            ViewBag.Centroids = centroids;
                            ViewBag.CentroidsLabels = new List<string>() { "Глюкоза" , "Холестерин" };


                            //List<List<string?>> result = new List<List<string?>>();
                            //for (int i = 0; i < clusterIndexes.Count; i++)
                            //{
                            //    List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                            //    for (int j = 0; j < parameters[i].parameters.Count; j++)
                            //    {
                            //        temp.Add(parameters[i].parameters[j].ToString());
                            //    }
                            //    result.Add(temp);
                            //}
                            List<List<string?>> result = new List<List<string?>>();
                            for (int i = 0; i < clusterIndexes.Count; i++)
                            {
                                List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                                for (int j = 0; j < clusterizationResult.normalizedInput[i].Count; j++)
                                {
                                    temp.Add(clusterizationResult.normalizedInput[i][j].ToString("0.###"));
                                }
                                result.Add(temp);
                            }

                            List<List<List<double>>> chartData = new List<List<List<double>>>();
                            for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                            {
                                chartData.Add(new List<List<double>>());
                            }
                            for (int i = 0; i < clusterIndexes.Count; i++)
                            {
                                List<double> temp = new List<double>();
                                for (int j = 0; j < parameters[i].parameters.Count; j++)
                                {
                                    temp.Add(parameters[i].parameters[j]);
                                }
                                chartData[clusterIndexes[i]].Add(temp);
                            }


                            ViewBag.Head = new List<string>() {
                           "Номер кластеру",
                           "ID пацієнта",
                           "Глюкоза",
                           "Холестерин"};
                            ViewBag.Table = result;
                            ViewBag.Data = chartData;
                        }
                    }
                }
            }
            else
            {
                ViewBag.FatalError = true;
            }
            return View("ClusteringKPop");
        }


        [HttpGet]
        public IActionResult ClusteringBiochemicalBloodAnalysis()
        {
            ViewBag.Title = "Clustering";
            ViewBag.Controller = "ClusteringBiochemicalBloodAnalysisK";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                var parameters = db.PatientsParameters
                    .Include(x => x.Parameter)
                    .Where(x => new string[] { "Загальний білірубін", "Прямий білірубін", "Непрямий білірубін", "АЛТ", "АСТ", "Креатинін", "Сечовина" }.Contains(x.Parameter.Name))
                    .ToList()
                    .GroupBy(x => new { x.PatientId, x.DateOfCheck })
                    .Select(x => new
                    {
                        patient = x.Key.PatientId,
                        parameters = new List<double?>
                        {
                                x.FirstOrDefault(x => x.Parameter.Name == "Загальний білірубін")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Прямий білірубін")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Непрямий білірубін")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "АЛТ")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "АСТ")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Креатинін")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Сечовина")?.Value
                        }
                    })
                    .Where(x => x.parameters.All(x => x != null))
                    .Select(x => new
                    {
                        patient = x.patient,
                        parameters = x.parameters.ConvertAll(x => (double)x!)
                    })
                    .ToList();

                if (parameters.Count == 0)
                {
                    ViewBag.Error = "Не знайдено жодного набору аналізів";
                    ViewBag.FatalError = true;
                }
                else
                {
                    ClusteringKMean.ClusterizationResult clusterizationResult = ClusteringKMean.Clusterize(parameters.Select(x => x.parameters).ToList());
                    List<int> clusterIndexes = clusterizationResult.clusters;

                    ViewBag.Elbow = clusterizationResult.elbow;

                    List<List<double>> centroids = new List<List<double>>();
                    for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                    {
                        List<double> averages = new List<double>();
                        for (int j = 0; j < parameters[0].parameters.Count; j++)
                        {
                            double avg = 0;
                            for (int k = 0; k < parameters.Count; k++)
                            {
                                if (clusterIndexes[k] == i)
                                {
                                    avg += parameters[k].parameters[j];
                                }
                            }
                            avg /= clusterIndexes.Where(x => x == i).Count();
                            averages.Add(avg);
                        }
                        centroids.Add(averages);

                    }

                    ViewBag.Centroids = centroids;
                    ViewBag.CentroidsLabels = new List<string>() { "Загальний білірубін", "Прямий білірубін", "Непрямий білірубін", "АЛТ", "АСТ", "Креатинін", "Сечовина" };

                    //List<List<string?>> result = new List<List<string?>>();
                    //for (int i = 0; i < clusterIndexes.Count; i++)
                    //{
                    //    List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                    //    for (int j = 0; j < parameters[i].parameters.Count; j++)
                    //    {
                    //        temp.Add(parameters[i].parameters[j].ToString());
                    //    }
                    //    result.Add(temp);
                    //
                    //}
                    List<List<string?>> result = new List<List<string?>>();
                    for (int i = 0; i < clusterIndexes.Count; i++)
                    {
                        List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                        for (int j = 0; j < clusterizationResult.normalizedInput[i].Count; j++)
                        {
                            temp.Add(clusterizationResult.normalizedInput[i][j].ToString("0.###"));
                        }
                        result.Add(temp);
                    }

                    ViewBag.Head = new List<string>() {
                           "Номер кластеру",
                           "ID пацієнта",
                           "Загальний білірубін",
                           "Прямий білірубін",
                           "Непрямий білірубін",
                           "АЛТ",
                           "АСТ",
                           "Креатинін",
                           "Сечовина" };
                    ViewBag.Table = result;
                }
            }

            return View("Clustering");
        }

        [HttpPost]
        public IActionResult ClusteringBiochemicalBloodAnalysisK(Int32Container number)
        {
            ViewBag.Title = "Clustering";
            ViewBag.Controller = "ClusteringBiochemicalBloodAnalysisK";
            if (ModelState.IsValid)
            {
                if (number.id < 1)
                {
                    ViewBag.Error = "кількість кластерів не може бути менше 1";
                    ViewBag.FatalError = true;
                }
                else
                {
                    using (BloodParametersContext db = new BloodParametersContext())
                    {
                        var parameters = db.PatientsParameters
                            .Include(x => x.Parameter)
                            .Where(x => new string[] { "Загальний білірубін", "Прямий білірубін", "Непрямий білірубін", "АЛТ", "АСТ", "Креатинін", "Сечовина" }.Contains(x.Parameter.Name))
                            .ToList()
                            .GroupBy(x => new { x.PatientId, x.DateOfCheck })
                            .Select(x => new
                            {
                                patient = x.Key.PatientId,
                                parameters = new List<double?>
                                {
                                x.FirstOrDefault(x => x.Parameter.Name == "Загальний білірубін")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Прямий білірубін")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Непрямий білірубін")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "АЛТ")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "АСТ")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Креатинін")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Сечовина")?.Value
                                }
                            })
                            .Where(x => x.parameters.All(x => x != null))
                            .Select(x => new
                            {
                                patient = x.patient,
                                parameters = x.parameters.ConvertAll(x => (double)x!)
                            })
                            .ToList();

                        if (parameters.Count == 0)
                        {
                            ViewBag.Error = "Не знайдено жодного набору аналізів";
                            ViewBag.FatalError = true;
                        }
                        else
                        {
                            ClusteringKMean.ClusterizationResult clusterizationResult = ClusteringKMean.Clusterize(parameters.Select(x => x.parameters).ToList(), number.id);
                            List<int> clusterIndexes = clusterizationResult.clusters;
                       
                            List<List<double>> centroids = new List<List<double>>();
                            for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                            {
                                List<double> averages = new List<double>();
                                for (int j = 0; j < parameters[0].parameters.Count; j++)
                                {
                                    double avg = 0;
                                    for (int k = 0; k < parameters.Count; k++)
                                    {
                                        if (clusterIndexes[k] == i)
                                        {
                                            avg += parameters[k].parameters[j];
                                        }
                                    }
                                    avg /= clusterIndexes.Where(x => x == i).Count();
                                    averages.Add(avg);
                                }
                                centroids.Add(averages);

                            }

                            ViewBag.Centroids = centroids;
                            ViewBag.CentroidsLabels = new List<string>() { "Загальний білірубін", "Прямий білірубін", "Непрямий білірубін", "АЛТ", "АСТ", "Креатинін", "Сечовина" };

                            //List<List<string?>> result = new List<List<string?>>();
                            //for (int i = 0; i < clusterIndexes.Count; i++)
                            //{
                            //    List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                            //    for (int j = 0; j < parameters[i].parameters.Count; j++)
                            //    {
                            //        temp.Add(parameters[i].parameters[j].ToString());
                            //    }
                            //    result.Add(temp);
                            //
                            //}
                            List<List<string?>> result = new List<List<string?>>();
                            for (int i = 0; i < clusterIndexes.Count; i++)
                            {
                                List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                                for (int j = 0; j < clusterizationResult.normalizedInput[i].Count; j++)
                                {
                                    temp.Add(clusterizationResult.normalizedInput[i][j].ToString("0.###"));
                                }
                                result.Add(temp);
                            }

                            ViewBag.Head = new List<string>() {
                           "Номер кластеру",
                           "ID пацієнта",
                           "Загальний білірубін",
                           "Прямий білірубін",
                           "Непрямий білірубін",
                           "АЛТ",
                           "АСТ",
                           "Креатинін",
                           "Сечовина" };
                            ViewBag.Table = result;
                        }
                    }
                }
            }
            else
            {
                ViewBag.FatalError = true;
            }

            return View("ClusteringK");
        }


        [HttpGet]
        public IActionResult ClusteringBloodPressure()
        {
            ViewBag.Title = "Clustering";
            ViewBag.Controller = "ClusteringBloodPressureK";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                var parameters = db.PatientsParameters
                    .Include(x => x.Parameter)
                    .Where(x => new string[] { "Систолічний тиск", "Діастолічний тиск", "Частота пульсу" }.Contains(x.Parameter.Name))
                    .ToList()
                    .GroupBy(x => new { x.PatientId, x.DateOfCheck })
                    .Select(x => new
                    {
                        patient = x.Key.PatientId,
                        parameters = new List<double?>
                        {
                                x.FirstOrDefault(x => x.Parameter.Name == "Систолічний тиск")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Діастолічний тиск")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Частота пульсу")?.Value
                        }
                    })
                    .Where(x => x.parameters.All(x => x != null))
                    .Select(x => new
                    {
                        patient = x.patient,
                        parameters = x.parameters.ConvertAll(x => (double)x!)
                    })
                    .ToList();

                if (parameters.Count == 0)
                {
                    ViewBag.Error = "Не знайдено жодного набору аналізів";
                    ViewBag.FatalError = true;
                }
                else
                {
                    ClusteringKMean.ClusterizationResult clusterizationResult = ClusteringKMean.Clusterize(parameters.Select(x => x.parameters).ToList());
                    List<int> clusterIndexes = clusterizationResult.clusters;

                    ViewBag.Elbow = clusterizationResult.elbow;

                    List<List<double>> centroids = new List<List<double>>();
                    for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                    {
                        List<double> averages = new List<double>();
                        for (int j = 0; j < parameters[0].parameters.Count; j++)
                        {
                            double avg = 0;
                            for (int k = 0; k < parameters.Count; k++)
                            {
                                if (clusterIndexes[k] == i)
                                {
                                    avg += parameters[k].parameters[j];
                                }
                            }
                            avg /= clusterIndexes.Where(x => x == i).Count();
                            averages.Add(avg);
                        }
                        centroids.Add(averages);

                    }

                    ViewBag.Centroids = centroids;
                    ViewBag.CentroidsLabels = new List<string>() { "Систолічний тиск", "Діастолічний тиск", "Частота пульсу" };

                    //List<List<string?>> result = new List<List<string?>>();
                    //for (int i = 0; i < clusterIndexes.Count; i++)
                    //{
                    //    List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                    //    for (int j = 0; j < parameters[i].parameters.Count; j++)
                    //    {
                    //        temp.Add(parameters[i].parameters[j].ToString());
                    //    }
                    //    result.Add(temp);
                    //}
                    List<List<string?>> result = new List<List<string?>>();
                    for (int i = 0; i < clusterIndexes.Count; i++)
                    {
                        List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                        for (int j = 0; j < clusterizationResult.normalizedInput[i].Count; j++)
                        {
                            temp.Add(clusterizationResult.normalizedInput[i][j].ToString("0.###"));
                        }
                        result.Add(temp);
                    }

                    List<List<List<double>>> chartData = new List<List<List<double>>>();
                    for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                    {
                        chartData.Add(new List<List<double>>());
                    }
                    for (int i = 0; i < clusterIndexes.Count; i++)
                    {
                        chartData[clusterIndexes[i]].Add(new List<double>() { parameters[i].parameters[0], parameters[i].parameters[1], parameters[i].parameters[2] });
                    }

                    ViewBag.Head = new List<string>() {
                           "Номер кластеру",
                           "ID пацієнта",
                           "Систолічний тиск",
                           "Діастолічний тиск",
                           "Частота пульсу" };
                    ViewBag.Table = result;
                    ViewBag.Data = chartData;
                }
            }

            return View("Clustering");
        }

        [HttpPost]
        public IActionResult ClusteringBloodPressureK(Int32Container number)
        {
            ViewBag.Title = "Clustering";
            ViewBag.Controller = "ClusteringBloodPressureK";
            if (ModelState.IsValid)
            {
                if (number.id < 1)
                {
                    ViewBag.Error = "кількість кластерів не може бути менше 1";
                    ViewBag.FatalError = true;
                }
                else
                {
                    using (BloodParametersContext db = new BloodParametersContext())
                    {
                        var parameters = db.PatientsParameters
                            .Include(x => x.Parameter)
                            .Where(x => new string[] { "Систолічний тиск", "Діастолічний тиск", "Частота пульсу" }.Contains(x.Parameter.Name))
                            .ToList()
                            .GroupBy(x => new { x.PatientId, x.DateOfCheck })
                            .Select(x => new
                            {
                                patient = x.Key.PatientId,
                                parameters = new List<double?>
                                {
                                x.FirstOrDefault(x => x.Parameter.Name == "Систолічний тиск")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Діастолічний тиск")?.Value,
                                x.FirstOrDefault(x => x.Parameter.Name == "Частота пульсу")?.Value
                                }
                            })
                            .Where(x => x.parameters.All(x => x != null))
                            .Select(x => new
                            {
                                patient = x.patient,
                                parameters = x.parameters.ConvertAll(x => (double)x!)
                            })
                            .ToList();

                        if (parameters.Count == 0)
                        {
                            ViewBag.Error = "Не знайдено жодного набору аналізів";
                            ViewBag.FatalError = true;
                        }
                        else
                        {
                            ClusteringKMean.ClusterizationResult clusterizationResult = ClusteringKMean.Clusterize(parameters.Select(x => x.parameters).ToList(), number.id);
                            List<int> clusterIndexes = clusterizationResult.clusters;
                         
                            List<List<double>> centroids = new List<List<double>>();
                            for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                            {
                                List<double> averages = new List<double>();
                                for (int j = 0; j < parameters[0].parameters.Count; j++)
                                {
                                    double avg = 0;
                                    for (int k = 0; k < parameters.Count; k++)
                                    {
                                        if (clusterIndexes[k] == i)
                                        {
                                            avg += parameters[k].parameters[j];
                                        }
                                    }
                                    avg /= clusterIndexes.Where(x => x == i).Count();
                                    averages.Add(avg);
                                }
                                centroids.Add(averages);

                            }

                            ViewBag.Centroids = centroids;
                            ViewBag.CentroidsLabels = new List<string>() { "Систолічний тиск", "Діастолічний тиск", "Частота пульсу" };


                            //List<List<string?>> result = new List<List<string?>>();
                            //for (int i = 0; i < clusterIndexes.Count; i++)
                            //{
                            //    List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                            //    for (int j = 0; j < parameters[i].parameters.Count; j++)
                            //    {
                            //        temp.Add(parameters[i].parameters[j].ToString());
                            //    }
                            //    result.Add(temp);
                            //}
                            List<List<string?>> result = new List<List<string?>>();
                            for (int i = 0; i < clusterIndexes.Count; i++)
                            {
                                List<string?> temp = new List<string?>() { clusterIndexes[i].ToString(), parameters[i].patient?.ToString() };
                                for (int j = 0; j < clusterizationResult.normalizedInput[i].Count; j++)
                                {
                                    temp.Add(clusterizationResult.normalizedInput[i][j].ToString("0.###"));
                                }
                                result.Add(temp);
                            }

                            List<List<List<double>>> chartData = new List<List<List<double>>>();
                            for (int i = 0; i < clusterIndexes.Max() + 1; i++)
                            {
                                chartData.Add(new List<List<double>>());
                            }
                            for (int i = 0; i < clusterIndexes.Count; i++)
                            {
                                chartData[clusterIndexes[i]].Add(new List<double>() { parameters[i].parameters[0], parameters[i].parameters[1], parameters[i].parameters[2] });
                            }


                            ViewBag.Head = new List<string>() {
                           "Номер кластеру",
                           "ID пацієнта",
                           "Систолічний тиск",
                           "Діастолічний тиск",
                           "Частота пульсу" };
                            ViewBag.Table = result;
                            ViewBag.Data = chartData;
                        }
                    }
                }
            }
            else
            {
                ViewBag.FatalError = true;
            }

            return View("ClusteringK");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}