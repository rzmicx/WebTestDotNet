using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace WebTestDotNet.Pages
{
    public class HomeModel : PageModel
    {
  
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public HomeModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public List<ProductDto> Products { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Type { get; set; } = "id"; // default

        [BindProperty(SupportsGet = true)]
        public string OrderBy { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {

                Response.Redirect("Login");
            }
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var allowedTypes = new[] { "id", "name", "description", "price", "createBy" };

            if (!allowedTypes.Contains(Type.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                Type = "id";  
            }
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var url = $"{baseUrl}/api/Product/ViewGrid?type={Uri.EscapeDataString(Type ?? "")}&orderby={Uri.EscapeDataString(OrderBy ?? "")}";

            try
            {
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var products = JsonSerializer.Deserialize<List<ProductDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (products != null)
                        Products = products;
                }
                else
                {
                   
                }
            }
            catch (Exception ex)
            {
                
            }

            return Page();
        }

        public class ProductDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public string CreateBy { get; set; }
        }
    }

}
