using Microsoft.AspNetCore.Mvc;

namespace CinemaWeb.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
