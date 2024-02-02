using Blood_parameters.Models;
using Blood_parameters.Models.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Security.Claims;

namespace Blood_parameters.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                using (BloodParametersContext db = new BloodParametersContext())
                {
                      ViewBag.Doctors = db.Doctors.ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
          
            return View("Index");
        }

        [HttpGet]
        public IActionResult OurDoctors()
        {
            ViewBag.Title = "Doctors";
            using (BloodParametersContext db = new BloodParametersContext())
            {
                byte[]? pic = db.Doctors.FirstOrDefault()?.Photo;
                if (pic != null)
                {
                    ViewBag.Image = "data:image/png;base64," + Convert.ToBase64String(pic, 0, pic.Length);
                }
                ViewBag.Doctors = db.Doctors.ToList();
            }
            return View();
        }



        [Authorize(Roles = "Administrator")]
        public IActionResult Reset()
        {
            DataBaseReset.ExecuteScript(this.ControllerContext.HttpContext.Request.Host.Value);
            return Index();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(Models.LoginPassword loginPassword)
        {
            string? roleName;
            Models.Database.User? user;
            using (BloodParametersContext db = new BloodParametersContext())
            {
                if (loginPassword.login == null || loginPassword.password == null)
                {
                    ViewBag.Error = "Заповніть логін та пароль";
                    return Login();
                }
                user = db.Users.Where(x => x.Email == loginPassword.login && x.Password == loginPassword.password).Include(x => x.Role).FirstOrDefault();
                if (user == null)
                {
                    ViewBag.Error = "Неправильний логін або пароль";
                    return Login();
                }
            }

            roleName = user?.Role?.Name;

            var claims = new List<Claim>{
            new Claim(ClaimsIdentity.DefaultNameClaimType, user?.Email??""),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, user?.Role?.Name??"")
         };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);
           
            switch (roleName)
            {
                case "Patient":
                    return RedirectToAction("Index", "Patient");
                case "Doctor":
                    return RedirectToAction("Index", "Doctor");
                case "Administrator":
                    return RedirectToAction("Index", "Administrator");
                default:
                    break;
            }

            ViewBag.Success = "Авторизація успішна";
            TempData["Success"] = "Авторизація успішна";

            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            ViewBag.Success = "Вихід з системи успішний";
            TempData["Success"] = "Вихід з системи успішний";
            return RedirectToAction("Login", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        /*
        [HttpGet]
        public async Task<IActionResult> GetAllRights()
        {
            var claims = new List<Claim>{
            new Claim(ClaimsIdentity.DefaultRoleClaimType, "Administrator"),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, "Doctor"),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, "Patient")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            HttpContext.User.IsInRole("Administrator");
             ViewBag.Success = "All Rights Login success";
            TempData["Success"] = "All Rights Login success";
            return RedirectToAction("Login", "Home");
            //return View("Login");
        }
        public async Task<IActionResult> GetPatientRights()
        {
            var claims = new List<Claim>{
            new Claim(ClaimsIdentity.DefaultNameClaimType, "abram.i.p@gmail.com"),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, "Patient")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);
            ViewBag.Success = "Patient Rights Login success";
            TempData["Success"] = "Patient Rights Login success";
            return RedirectToAction("Login", "Home");
      //      return View("Login");
        }
        public async Task<IActionResult> GetDoctorRights()
        {
            var claims = new List<Claim>{
            new Claim(ClaimsIdentity.DefaultNameClaimType, "akinshcheva.m.o@gmail.com"),
            new Claim(ClaimsIdentity.DefaultRoleClaimType, "Doctor")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);
            ViewBag.Success = "Doctor Rights Login success";
            TempData["Success"] = "Doctor Rights Login success";
            return RedirectToAction("Login", "Home");
      //      return View("Login");
        }
        */

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}