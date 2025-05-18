using InventoryManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;
using InventoryManagement.Data;

namespace InventoryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly InventoryDbContext _context;

        public AccountController(InventoryDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if the company name already exists (case-insensitive)
                var existingCompany = _context.Users
             .FirstOrDefault(u => u.CompanyName.ToLower() == user.CompanyName.ToLower());

                if (existingCompany != null)
                {
                    ModelState.AddModelError("CompanyName", "This company name is already taken.");
                    return View(user);
                }

                // Disallow reserved emails
                if (user.Email.ToLower() == "admin@gmail.com")
                {
                    ModelState.AddModelError("Email", "This email is not allowed for registration.");
                    return View(user);
                }

                // Check if email is already in use
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(user);
                }

                user.Password = HashPassword(user.Password);
                user.VerificationCode = GenerateVerificationCode();
                user.IsVerified = false;

                _context.Users.Add(user);
                _context.SaveChanges();

                // Send the email
                SendVerificationEmail(user.Email, user.VerificationCode);

                TempData["ShowVerificationForm"] = true;
                TempData["Email"] = user.Email;
                TempData["Message"] = $"A verification code was sent to {user.Email}.";

                return RedirectToAction("Register");
            }

            return View(user);
        }


        // GENERATE VERIFICATION CODE
        private string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }


        // Password hashing method (SHA256 example)
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        // GET: /Account/Login
        [HttpGet]

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(LoginViewModel user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);

                if (existingUser != null && existingUser.Password == HashPassword(user.Password))
                {
                    if (!existingUser.IsVerified)
                    {
                        existingUser.VerificationCode = GenerateVerificationCode();
                        _context.SaveChanges();

                        SendVerificationEmail(existingUser.Email, existingUser.VerificationCode);

                        TempData["ShowVerificationForm"] = true;
                        TempData["Email"] = existingUser.Email;
                        TempData["Message"] = "Your email is not verified. A new verification code has been sent to your email.";

                        return RedirectToAction("Register");
                    }

                    HttpContext.Session.SetString("UserEmail", existingUser.Email);
                    HttpContext.Session.SetString("UserName", existingUser.CompanyName);
                    HttpContext.Session.SetInt32("UserId", existingUser.Id);

                    TempData["LoginSuccess"] = "You have logged in successfully.";

                    return RedirectToAction("Index", "Products");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Email or Password.");
                }
            }

            return View(user);
        }


        // SEND EMAIL
        private void SendVerificationEmail(string email, string code)
        {
            var fromAddress = new MailAddress("travelplanningteam@gmail.com", "INVEN2E Management System");
            var toAddress = new MailAddress(email);
            const string fromPassword = "tqay ehtt dmeg xnou";

            string subject = "Your Verification Code";
            string body = $"Hello,\n\nYour verification code is: {code}\n\nThank you!";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }

        // Trigger forgot password section visibility
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            TempData["ForgotPassword"] = true;
            return RedirectToAction("Login");
        }

        // Send Reset Code
        [HttpPost]
        public IActionResult SendResetCode(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["ForgotPasswordError"] = "Email not found.";
                TempData["ForgotPassword"] = true;
                return RedirectToAction("Login");
            }

            string verificationCode = GenerateVerificationCode();
            user.VerificationCode = verificationCode;
            _context.SaveChanges();

            SendVerificationEmail(user.Email, verificationCode);

            TempData["ForgotPassword"] = true;
            TempData["Step"] = "CodeSent";
            TempData["ResetEmail"] = email;

            return RedirectToAction("Login");
        }


        // Verify Reset Code
        [HttpPost]
        public IActionResult VerifyResetCode(string email, string code)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || user.VerificationCode != code)
            {
                TempData["CodeVerificationError"] = "Invalid verification code.";
                TempData["ForgotPassword"] = true;
                TempData["Step"] = "CodeSent";
                TempData["ResetEmail"] = email;
                return RedirectToAction("Login");
            }

            TempData["ForgotPassword"] = true;
            TempData["Step"] = "CodeVerified";
            TempData["ResetEmail"] = email;

            return RedirectToAction("Login");
        }

        // Reset Password
        [HttpPost]
        public IActionResult ResetPassword(string email, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["PasswordResetError"] = "User not found.";
                return RedirectToAction("Login");
            }

            user.Password = HashPassword(newPassword);
            user.VerificationCode = null;
            _context.SaveChanges();

            TempData["PasswordResetSuccess"] = "Password successfully changed. You can now log in.";
            return RedirectToAction("Login");
        }


        // GENERATE VERIFICATION CODE
        [HttpPost]
        public IActionResult VerifyCode(string email, string code)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null && user.VerificationCode == code)
            {
                user.IsVerified = true;
                _context.SaveChanges();


                SendSuccessRegistrationEmail(user.Email, user.CompanyName);

                TempData["RegisterSuccess"] = "Your account has been verified. You can now log in.";
                return RedirectToAction("Login");
            }

            TempData["ShowVerificationForm"] = true;
            TempData["Email"] = email;
            TempData["Message"] = "Incorrect verification code. Please try again.";
            return RedirectToAction("Register");
        }

        // SEND SUCCESS EMAIL
        private void SendSuccessRegistrationEmail(string email, string companyName)
        {
            var fromAddress = new MailAddress("travelplanningteam@gmail.com", "INVEN2E Management System");
            var toAddress = new MailAddress(email);
            const string fromPassword = "tqay ehtt dmeg xnou";

            string subject = "Registration Successful";
            string body = $"Hello {companyName},\n\n" +
                          "Your registration has been successfully completed and your email has been verified.\n\n" +
                          "Thank you for joining INVEN2E!";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }

        


    }
}
