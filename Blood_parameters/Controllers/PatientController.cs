using Blood_parameters.Models;
using Blood_parameters.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;

namespace Blood_parameters.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public PatientController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Info(Blood_parameters.Models.Database.Patient record)
        {
            //ControllerContext.HttpContext.User.Claims;
            //ClaimsIdentity.DefaultNameClaimType
            ViewBag.Title = "Info";

            using (BloodParametersContext db = new BloodParametersContext())
            {
                string? email = ControllerContext.HttpContext.User.Claims
                       .Where(x => x.Type == ClaimsIdentity.DefaultNameClaimType)
                       .Select(x => x.Value).FirstOrDefault();
                if (email == null)
                {
                    ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                    return View("Info");
                }
                int? id = db.Users
                   .Where(x => x.Email == email)
                   .Select(x => x.PatientId).FirstOrDefault();
                if (id == null)
                {
                    ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                    return View("Info");
                }

                Patient? patient = db.Patients.Where(x => x.Id == id).Include(x => x.Doctor).ThenInclude(x=>x.Users).FirstOrDefault();
                if (patient == null)
                {
                    ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                    return View("Info");
                }
                ViewBag.Patient = patient;
                ViewBag.Email = email;

                // DateTime.Now.ToShortDateString()

                if (record.Id == 0)
                {
                    record = db.Patients.Where(x => x.Id == id).FirstOrDefault();
                }
            }
            return View("Info", record);
        }

        [HttpPost]
        public IActionResult InfoUpdate(Blood_parameters.Models.Database.Patient record)
        {
            //ValidateOnly("patient");
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
                            ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                            return View("Info");
                        }
                        int? id = db.Users
                           .Where(x => x.Email == email)
                           .Select(x => x.PatientId).FirstOrDefault();
                        if (id == null)
                        {
                            ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                            return View("Info");
                        }

                        Patient? patient = db.Patients.Where(x => x.Id == id).Include(x => x.Doctor).ThenInclude(x => x.Users).FirstOrDefault();
                        if (patient == null)
                        {
                            ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                            return View("Info");
                        }

                        int rows = db.Database.ExecuteSql($"updatePatient {id}, {record.Surname}, {patient.Name}, {patient.Patronymic}, {patient.Dob}, {patient.Gender}, {record.Phone}, {record.Address}, {record.EmergencyContactSurname}, {record.EmergencyContactName}, {record.EmergencyContactPatronymic}, {record.EmergencyContactPhone}");
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
        public IActionResult Parameters()
        {
            ViewBag.Title = "Parameters";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                string? email = ControllerContext.HttpContext.User.Claims
                       .Where(x => x.Type == ClaimsIdentity.DefaultNameClaimType)
                       .Select(x => x.Value).FirstOrDefault();
                if (email == null)
                {
                    ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                    return View("Parameters");
                }
                int? id = db.Users
                   .Where(x => x.Email == email)
                   .Select(x => x.PatientId).FirstOrDefault();
                if (id == null)
                {
                    ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                    return View("Parameters");
                }

                ViewBag.Head = new List<string>() {
                    "Параметр",
                    "Значення",
                    "Дата перевірки"};
                ViewBag.Table = db.PatientsParameters.Where(x => x.PatientId == id).Include(x => x.Parameter).ToList()
                    .Select(x => new List<string>() {
                        x?.Parameter?.Name,
                        x?.Value.ToString(),
                        x?.DateOfCheck?.ToShortDateString()}).ToList();
                if (ViewBag.Table.Count == 0)
                {
                    ViewBag.FatalError = "У вас ще немає аналізів";
                    return View("Parameters");
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult Doctors()
        {
            ViewBag.Title = "Doctors";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                string? email = ControllerContext.HttpContext.User.Claims
                       .Where(x => x.Type == ClaimsIdentity.DefaultNameClaimType)
                       .Select(x => x.Value).FirstOrDefault();
                if (email != null)
                {
                    int? id = db.Users
                      .Where(x => x.Email == email)
                      .Select(x => x.PatientId).FirstOrDefault();
                    if (id != null)
                    {
                        int? doctorid = db.Patients.Where(x => x.Id == id).Select(x => x.DoctorId).FirstOrDefault();
                        ViewBag.DoctorId = doctorid;
                    }
                }


                ViewBag.Doctors = db.Doctors.Include(x => x.Users).ToList();
            }
            return View();
        }

        [HttpGet]
        public IActionResult Medications()
        {
            ViewBag.Title = "Medications";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                string? email = ControllerContext.HttpContext.User.Claims
                       .Where(x => x.Type == ClaimsIdentity.DefaultNameClaimType)
                       .Select(x => x.Value).FirstOrDefault();
                if (email == null)
                {
                    ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                    return View("Medications");
                }
                int? id = db.Users
                   .Where(x => x.Email == email)
                   .Select(x => x.PatientId).FirstOrDefault();
                if (id == null)
                {
                    ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                    return View("Medications");
                }

                ViewBag.Head = new List<string>() {
                    "Тривалість прийому",
                    "Назва препарату",
                    "Дозування",
                    "Частота прийому",
                    "Прізвище лікаря",
                    "Ім’я лікаря",
                    "По батькові лікаря"};
                ViewBag.Table = db.Medications.Where(x => x.PatientId == id).Include(x => x.Doctor).ToList()
                    .Select(x => new List<string?>() {
                        x?.ReceptionDuration,
                        x?.MedicationName,
                        x?.MedicationDosage,
                        x?.MedicationFrequency,
                        x?.Doctor?.Surname,
                        x?.Doctor?.Name,
                        x?.Doctor?.Patronymic}).ToList();

                if (ViewBag.Table.Count == 0)
                {
                    ViewBag.FatalError = "У вас ще немає назначених ліків";
                    return View("Medications");
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult MedicalConditions()
        {
            ViewBag.Title = "MedicalConditions";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                string? email = ControllerContext.HttpContext.User.Claims
                       .Where(x => x.Type == ClaimsIdentity.DefaultNameClaimType)
                       .Select(x => x.Value).FirstOrDefault();
                if (email == null)
                {
                    ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                    return View("MedicalConditions");
                }
                int? id = db.Users
                   .Where(x => x.Email == email)
                   .Select(x => x.PatientId).FirstOrDefault();
                if (id == null)
                {
                    ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                    return View("MedicalConditions");
                }

                ViewBag.Head = new List<string>() {
                    "Прізвище лікаря",
                    "Ім’я лікаря",
                    "По батькові лікаря",
                    "Скарги",
                    "Історія хвороби",
                    "Історія захворювань пацієнта",
                    "Об’єктивний стан"};
                ViewBag.Table = db.MedicalConditions.Where(x => x.PatientId == id).Include(x => x.Doctor).ToList()
                    .Select(x => new List<string?>() {
                        x?.Doctor?.Surname,
                        x?.Doctor?.Name,
                        x?.Doctor?.Patronymic,
                        x?.Complaints,
                        x?.MedicalHistory,
                        x?.ClinicalHistory,
                        x?.ObjectiveCondition}).ToList();

                if (ViewBag.Table.Count == 0)
                {
                    ViewBag.FatalError = "У вас ще немає медичних показань";
                    return View("MedicalConditions");
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult Appointments()
        {
            ViewBag.Title = "Appointments";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                string? email = ControllerContext.HttpContext.User.Claims
                       .Where(x => x.Type == ClaimsIdentity.DefaultNameClaimType)
                       .Select(x => x.Value).FirstOrDefault();
                if (email == null)
                {
                    ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                    return View("Appointments");
                }
                int? id = db.Users
                   .Where(x => x.Email == email)
                   .Select(x => x.PatientId).FirstOrDefault();
                if (id == null)
                {
                    ViewBag.FatalError = "Немає запису пацієнта, пов'язаного з вашим акаунтом";
                    return View("Appointments");
                }

                ViewBag.Head = new List<string>() {
                    "Дата лікування",
                    "Час лікування",
                    "Код прийому",
                    "Прізвище лікаря",
                    "Ім’я лікаря",
                    "По батькові лікаря",
                    "Дата початку",
                    "Дата закінчення",
                    "Діагноз",
                    "Лікування",
                    "Рекомендації щодо лікування та роботи",
                    "Рекомендовано"};
                ViewBag.Table = db.Appointments.Where(x => x.PatientId == id).Include(x => x.Doctor).ToList()
                    .Select(x => new List<string?>() {
                        x?.TreatmentDate.ToShortDateString(),
                        x?.TreatmentTime.ToString(),
                        x?.RecordNumber,
                        x?.Doctor?.Surname,
                        x?.Doctor?.Name,
                        x?.Doctor?.Patronymic,
                        x?.StartDate.ToShortDateString(),
                        x?.EndDate.ToShortDateString(),
                        x?.Diagnosis,
                        x?.Treatment,
                        x?.TreatmentAndWorkRecommendations,
                        x?.Recommended}).ToList();
                if (ViewBag.Table.Count == 0)
                {
                    ViewBag.FatalError = "У вас ще немає призначень";
                    return View("Appointments");
                }
            }
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}