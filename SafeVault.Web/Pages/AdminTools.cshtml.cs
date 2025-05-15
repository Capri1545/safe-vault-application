using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SafeVault.Web.Pages
{
    [Authorize(Roles = "Admin")]
    public class AdminToolsModel : PageModel
    {
        public void OnGet() { }
    }
}
