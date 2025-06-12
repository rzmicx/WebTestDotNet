using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;

namespace WebTestDotNet.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public RegisterModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var payload = new
            {
                username = Username,
                passcode = Password
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var response = await client.PostAsync($"{baseUrl}/api/Auth/register", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Login");
            }
            else
            {
                var result = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Register failed: {result}";
                return Page();
            }
        }
    }
}