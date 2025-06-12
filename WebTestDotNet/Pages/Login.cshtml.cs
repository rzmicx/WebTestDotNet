using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace WebTestDotNet.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        [Required]
        public string Username { get; set; }

        [BindProperty]
        [Required]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public LoginModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var client = _httpClientFactory.CreateClient();
            var loginPayload = new
            {
                username = Username,
                password = Password
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(loginPayload),
                Encoding.UTF8,
                "application/json");

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var response = await client.PostAsync($"{baseUrl}/api/Auth/login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                 
                using var document = JsonDocument.Parse(responseBody);
                var token = document.RootElement.GetProperty("token").GetString();
                 
                HttpContext.Session.SetString("JwtToken", token);
                 
                Response.Cookies.Append("JwtToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });
                return RedirectToPage("/Home");
            }
            else
            {
                ErrorMessage = "Login gagal. Periksa username dan password.";
                return Page();
            }
        }
    }
}