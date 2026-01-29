using CinemaWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CinemaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly CinemaDbContext _context;

        public HomeController(CinemaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var activeMovies = await _context.Movies
                .Include(m => m.Sessions)
                .Where(m => m.Sessions.Any(s => s.StartTime > now))
                .ToListAsync();

            return View(activeMovies);
        }

        public async Task<IActionResult> AllMovies()
        {
            var allMovies = await _context.Movies.ToListAsync();
            return View(allMovies);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Sessions)
                .ThenInclude(s => s.Hall)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }
    }
}