using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WebTestDotNet.Pages
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public EditModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Unauthorized. Please login again.";
                return RedirectToPage("/Login");
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var baseUrl = _configuration["ApiSettings:BaseUrl"]; 
            var response = await client.GetAsync($"{baseUrl}/api/Product/ViewGrid?type=id&orderby={id}");

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Failed to retrieve product.";
                return Page();
            }

            var json = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<ProductDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var product = products?.FirstOrDefault();
            if (product == null)
            {
                ErrorMessage = "Product not found.";
                return Page();
            }

            Id = product.Id;
            Name = product.Name;
            Description = product.Description;
            Price = product.Price;
            CreatedAt = product.CreatedAt;

            return Page();
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
                id = Id,
                name = Name,
                description = Description,
                price = Price,
                createdAt = CreatedAt,
                createBy = "" // Optional: parse from JWT if needed
            };

            var json = JsonSerializer.Serialize(productPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var response = await client.PostAsync($"{baseUrl}/api/Product/editProduct", content);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = "Product successfully updated!";
                return RedirectToPage("/Home");
            }
            else
            {
                ErrorMessage = $"Failed to update product. ({response.StatusCode})";
                return Page();
            }
        }

        private class ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public DateTime CreatedAt { get; set; }
            public string CreateBy { get; set; }
        }
    }
}