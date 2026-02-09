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

        public async Task<IActionResult> Index(
            string searchString,
            string selectedGenre,
            int? selectedYear,
            DateTime? selectedDate)
        {
            var moviesQuery = _context.Movies
                .Where(m => !m.IsArchived)
                .Include(m => m.Sessions)
                .Include(m => m.Moviegenres)
                    .ThenInclude(mg => mg.Genre)
                .AsQueryable();

            // Фільтр: пошук
            if (!string.IsNullOrEmpty(searchString))
            {
                moviesQuery = moviesQuery
                    .Where(s => s.Title.Contains(searchString));
            }

            // Фільтр: жанр
            if (!string.IsNullOrEmpty(selectedGenre))
            {
                moviesQuery = moviesQuery
                    .Where(m => m.Moviegenres
                        .Any(mg => mg.Genre.Name == selectedGenre));
            }

            // Фільтр: рік
            if (selectedYear.HasValue)
            {
                moviesQuery = moviesQuery
                    .Where(x => x.ReleaseDate.Year == selectedYear);
            }

            // Календар
            if (selectedDate.HasValue)
            {
                moviesQuery = moviesQuery
                    .Where(m => m.Sessions.Any(s => 
                        s.StartTime.Date == selectedDate.Value.Date &&
                        s.StartTime > DateTime.Now
                    ));

                ViewData["SelectedDate"] = selectedDate.Value.Date;
            }
            else
            {
                var now = DateTime.Now;
                moviesQuery = moviesQuery
                    .Where(m => m.Sessions
                        .Any(s => s.StartTime > now));

                ViewData["SelectedDate"] = null;
            }

            ViewBag.Genres = await _context.Genres
                .OrderBy(g => g.Name)
                .Select(g => g.Name)
                .ToListAsync();

            ViewBag.Years = await _context.Movies
                .Where(m => !m.IsArchived)
                .Select(m => m.ReleaseDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentGenre"] = selectedGenre;
            ViewData["CurrentYear"] = selectedYear;

            return View(await moviesQuery.ToListAsync());
        }

        public async Task<IActionResult> AllMovies()
        {
            var allMovies = await _context.Movies
                .Include(m => m.Moviegenres)
                    .ThenInclude(mg => mg.Genre)
                .OrderByDescending(m => m.ReleaseDate)
                .ToListAsync();

            return View(allMovies);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
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