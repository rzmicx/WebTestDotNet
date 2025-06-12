using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebTestDotNet.Pages
{
    public class LogoutModel : PageModel
    {
        public void OnGet()
        {
            HttpContext.Session.Remove("JwtToken");


            Response.Redirect("Login");
        }
    }
}
