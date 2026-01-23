using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaWeb.Models;
using CinemaWeb.Services;

namespace CinemaWeb.Controllers
{
    public class MoviesController : Controller
    {
        private readonly CinemaDbContext _context;
        private readonly IImageService _imageService;

        public MoviesController(CinemaDbContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name");
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile posterImage, int genreId)
        {
            ModelState.Remove("PosterUrl");

            if (ModelState.IsValid)
            {
                // ВИПРАВЛЕНО: Обробка завантаження постера
                if (posterImage != null && posterImage.Length > 0)
                {
                    var result = await _imageService.UploadImageAsync(posterImage);

                    if (result.Success)
                    {
                        // Повертаємо шлях до файлу
                        movie.PosterUrl = $"/images/{result.FileName}";
                    }
                    else
                    {
                        // Додаємо помилку в ModelState
                        ModelState.AddModelError("posterImage", result.ErrorMessage);
                        ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", genreId);
                        return View(movie);
                    }
                }
                else
                {
                    // Якщо картинку не вибрали - заглушка
                    movie.PosterUrl = "/images/no-poster.jpg";
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();

                var movieGenre = new Moviegenre
                {
                    MovieId = movie.Id,
                    GenreId = genreId
                };
                _context.Moviegenres.Add(movieGenre);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", genreId);
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            // ВИПРАВЛЕНО: Додаємо вибір жанру для Edit
            var currentLink = await _context.Moviegenres
                .FirstOrDefaultAsync(m => m.MovieId == id);
            int selectedGenreId = currentLink?.GenreId ?? 0;

            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", selectedGenreId);
            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile posterImage, int genreId)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            ModelState.Remove("PosterUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    // ВИПРАВЛЕНО: Обробка завантаження постера для Edit
                    if (posterImage != null && posterImage.Length > 0)
                    {
                        var result = await _imageService.UploadImageAsync(posterImage);

                        if (result.Success)
                        {
                            movie.PosterUrl = $"/images/{result.FileName}";
                        }
                        else
                        {
                            ModelState.AddModelError("posterImage", result.ErrorMessage);
                            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", genreId);
                            return View(movie);
                        }
                    }
                    else
                    {
                        // Зберігаємо старий постер
                        var oldMovie = await _context.Movies
                            .AsNoTracking()
                            .FirstOrDefaultAsync(m => m.Id == id);

                        movie.PosterUrl = oldMovie?.PosterUrl ?? "/images/no-poster.jpg";
                    }

                    _context.Update(movie);
                    await _context.SaveChangesAsync();

                    // Оновлюємо жанр
                    var oldLinks = _context.Moviegenres.Where(mg => mg.MovieId == id);
                    _context.Moviegenres.RemoveRange(oldLinks);
                    await _context.SaveChangesAsync();

                    _context.Moviegenres.Add(new Moviegenre { MovieId = id, GenreId = genreId });
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", genreId);
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}