using Microsoft.AspNetCore.Mvc;

namespace WhiteHotel.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
