using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebTestDotNet.Pages
{
    public class AddModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public AddModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }


        public void OnGet()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {

                Response.Redirect("Login");
            }
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Unauthorized. Please login again.";
                return Page();
            }

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var productPayload = new
            {
                id=0,
                name = Name,
                description = Description,
                price = Price,
                createdAt = DateTime.UtcNow,
                createBy = ""  
            };

            var json = JsonSerializer.Serialize(productPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            
            var response = await client.PostAsync($"{baseUrl}/api/Product/addProduct", content);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Product successfully added!";
                Name = Description = "";
                Price = 0;
            }
            else
            {
                ErrorMessage = $"Failed to add product. ({response.StatusCode})";
            }

            return Page();
        }
    }
}