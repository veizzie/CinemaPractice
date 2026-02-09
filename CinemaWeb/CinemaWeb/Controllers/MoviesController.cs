using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaWeb.Models;
using CinemaWeb.Services;
using Microsoft.AspNetCore.Authorization;

namespace CinemaWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MoviesController : Controller
    {
        private readonly CinemaDbContext _context;
        private readonly IImageService _imageService;
        private readonly IWebHostEnvironment _appEnvironment;

        public MoviesController(
            CinemaDbContext context,
            IImageService imageService,
            IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _imageService = imageService;
            _appEnvironment = appEnvironment;
        }

        // GET: Movies
        public async Task<IActionResult> Index(
            string searchString,
            int? genreId,
            decimal? minPrice,
            decimal? maxPrice,
            string statusFilter)
        {
            var moviesQuery = _context.Movies
                .Include(m => m.Moviegenres)
                    .ThenInclude(mg => mg.Genre)
                .AsQueryable();

            // Фільтр: статус (Активні / Архів / Всі)
            if (statusFilter == "active")
            {
                moviesQuery = moviesQuery.Where(m => !m.IsArchived);
            }
            else if (statusFilter == "archived")
            {
                moviesQuery = moviesQuery.Where(m => m.IsArchived);
            }
            else
            {
                moviesQuery = moviesQuery.OrderBy(m => m.IsArchived);
            }

            // Фільтр: пошук
            if (!string.IsNullOrEmpty(searchString))
            {
                moviesQuery = moviesQuery
                    .Where(m => m.Title.Contains(searchString) ||
                                m.Director.Contains(searchString));
            }

            // Фільтр: жанр
            if (genreId.HasValue)
            {
                moviesQuery = moviesQuery
                    .Where(m => m.Moviegenres.Any(g => g.GenreId == genreId));
            }

            // Фільтр: ціна
            if (minPrice.HasValue)
            {
                moviesQuery = moviesQuery
                    .Where(m => m.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                moviesQuery = moviesQuery
                    .Where(m => m.Price <= maxPrice.Value);
            }

            ViewData["GenreId"] = new SelectList(
                _context.Genres, "Id", "Name", genreId);

            ViewData["CurrentFilter"] = searchString;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["StatusFilter"] = statusFilter;

            return View(await moviesQuery.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Moviegenres)
                    .ThenInclude(mg => mg.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Movie movie,
            IFormFile posterImage,
            int[] genreIds)
        {
            ModelState.Remove("PosterUrl");
            ModelState.Remove("Sessions");

            if (ModelState.IsValid)
            {
                if (posterImage != null && posterImage.Length > 0)
                {
                    var result = await _imageService
                        .UploadImageAsync(posterImage);

                    if (result.Success)
                    {
                        movie.PosterUrl = $"/images/{result.FileName}";
                    }
                    else
                    {
                        ModelState.AddModelError("posterImage", result.ErrorMessage);
                        ViewBag.Genres = new SelectList(
                            _context.Genres, "Id", "Name");
                        return View(movie);
                    }
                }
                else
                {
                    movie.PosterUrl = "/images/no-poster.jpg";
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();

                if (genreIds != null)
                {
                    foreach (var id in genreIds)
                    {
                        _context.Moviegenres.Add(new Moviegenre
                        {
                            MovieId = movie.Id,
                            GenreId = id
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            var selectedGenreIds = await _context.Moviegenres
                .Where(mg => mg.MovieId == id)
                .Select(mg => mg.GenreId)
                .ToListAsync();

            ViewBag.GenreId = new MultiSelectList(
                _context.Genres, "Id", "Name", selectedGenreIds);

            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Movie movie,
            IFormFile? posterImage,
            int[] genreIds)
        {
            if (id != movie.Id) return NotFound();

            ModelState.Remove("PosterUrl");
            ModelState.Remove("Sessions");
            ModelState.Remove("Moviegenres");
            ModelState.Remove("posterImage");

            if (ModelState.IsValid)
            {
                try
                {
                    var movieInDb = await _context.Movies
                        .Include(m => m.Moviegenres)
                        .FirstOrDefaultAsync(m => m.Id == id);

                    if (movieInDb == null) return NotFound();

                    movieInDb.Title = movie.Title;
                    movieInDb.Description = movie.Description;
                    movieInDb.Director = movie.Director;
                    movieInDb.Cast = movie.Cast;
                    movieInDb.Duration = movie.Duration;
                    movieInDb.ReleaseDate = movie.ReleaseDate;
                    movieInDb.Price = movie.Price;

                    if (posterImage != null && posterImage.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(movieInDb.PosterUrl)
                            && !movieInDb.PosterUrl.Contains("no-poster"))
                        {
                            try
                            {
                                string fullPath = Path.Combine(
                                    _appEnvironment.WebRootPath,
                                    movieInDb.PosterUrl.TrimStart('/'));

                                if (System.IO.File.Exists(fullPath))
                                {
                                    System.IO.File.Delete(fullPath);
                                }
                            }
                            catch { }
                        }

                        var result = await _imageService
                            .UploadImageAsync(posterImage);

                        if (result.Success)
                        {
                            movieInDb.PosterUrl = $"/images/{result.FileName}";
                        }
                        else
                        {
                            ModelState.AddModelError(
                                "posterImage",
                                result.ErrorMessage);

                            ViewBag.GenreId = new MultiSelectList(
                                _context.Genres, "Id", "Name", genreIds);
                            return View(movie);
                        }
                    }

                    var oldGenres = _context.Moviegenres
                        .Where(mg => mg.MovieId == id);

                    _context.Moviegenres.RemoveRange(oldGenres);

                    if (genreIds != null)
                    {
                        foreach (var gId in genreIds)
                        {
                            _context.Moviegenres.Add(new Moviegenre
                            {
                                MovieId = id,
                                GenreId = gId
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(movie.PosterUrl))
            {
                var dbMovie = await _context.Movies.AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (dbMovie != null)
                {
                    movie.PosterUrl = dbMovie.PosterUrl;
                }
            }

            ViewBag.GenreId = new MultiSelectList(
                _context.Genres, "Id", "Name", genreIds);

            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .Include(m => m.Moviegenres)
                    .ThenInclude(mg => mg.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var hasActiveTickets = await _context.Tickets
                .Include(t => t.Session)
                .AnyAsync(t => t.Session.MovieId == id
                          && t.Session.StartTime > DateTime.Now);

            if (hasActiveTickets)
            {
                ViewBag.ErrorMessage =
                    "Не можна архівувати фільм! На майбутні сеанси вже продано квитки. " +
                    "Спочатку скасуйте їх або дочекайтесь завершення сеансів.";
                return View("Delete", movie);
            }

            var futureEmptySessions = await _context.Sessions
                .Where(s => s.MovieId == id && s.StartTime > DateTime.Now)
                .ToListAsync();

            if (futureEmptySessions.Any())
            {
                _context.Sessions.RemoveRange(futureEmptySessions);
            }

            movie.IsArchived = true;
            _context.Movies.Update(movie);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Movies/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null && movie.IsArchived)
            {
                movie.IsArchived = false;
                _context.Update(movie);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}