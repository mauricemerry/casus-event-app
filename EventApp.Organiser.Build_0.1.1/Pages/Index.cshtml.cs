using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EventApp.OrganiserPortal.Repositories;
using EventApp.OrganiserPortal.Models;

namespace EventApp.OrganiserPortal.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IOrganiserRepository _organiserRepository;
        private readonly PasswordHasher<OrganiserModel> _passwordHasher = new();

        public LoginModel(IOrganiserRepository organiserRepository)
        {
            _organiserRepository = organiserRepository;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public void OnGet() 
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
           if (!ModelState.IsValid)
            {
                return Page();
            }

            var organiser = await _organiserRepository.GetByEmailAsync(Input.Email);

            if (organiser == null)
            {
                ModelState.AddModelError(string.Empty, "Ongeldige inloggegevens.");
                return Page();
            }

            var verifyResult = _passwordHasher.VerifyHashedPassword(
                organiser,
                organiser.PasswordHash,
                Input.Password);

            if (verifyResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Ongeldige inloggegevens.");
                return Page();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, organiser.OrganiserId.ToString()),
                new Claim(ClaimTypes.Name, organiser.CompanyName),
                new Claim(ClaimTypes.Email, organiser.Email)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToPage("/Portal/Dashboard");
        }

        public class LoginInputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "E-mail")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Wachtwoord")]
            public string Password { get; set; } = string.Empty;
        }
    }
}