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
            if (id == null) return NotFound();

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return NotFound();

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            // Передаємо список для віджета
            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile posterImage, int[] genreIds)
        {
            // Прибираємо валідацію полів, яких немає у формі або заповнюються автоматично
            ModelState.Remove("PosterUrl");
            ModelState.Remove("Sessions");
            // Видаляємо перевірку MovieGenres, бо у твоїй моделі Movie цього поля може не бути

            if (ModelState.IsValid)
            {
                // 1. ЛОГІКА ЗАВАНТАЖЕННЯ КАРТИНКИ (Виправлено під твій сервіс)
                if (posterImage != null && posterImage.Length > 0)
                {
                    var result = await _imageService.UploadImageAsync(posterImage);

                    // Перевіряємо, чи успішно завантажилось (бо твій сервіс повертає об'єкт, а не рядок)
                    if (result.Success)
                    {
                        movie.PosterUrl = $"/images/{result.FileName}";
                    }
                    else
                    {
                        // Якщо помилка завантаження — показуємо її і повертаємо форму
                        ModelState.AddModelError("posterImage", result.ErrorMessage);
                        ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
                        return View(movie);
                    }
                }
                else
                {
                    movie.PosterUrl = "/images/no-poster.jpg";
                }

                // 2. ЗБЕРІГАЄМО ФІЛЬМ
                _context.Add(movie);
                await _context.SaveChangesAsync();

                // 3. ЗБЕРІГАЄМО ЖАНРИ (через прямий доступ до таблиці зв'язків)
                if (genreIds != null)
                {
                    foreach (var id in genreIds)
                    {
                        // Використовуємо твою назву класу Moviegenre (з малою 'g')
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

            // Отримуємо жанри. Використовуємо Moviegenres (як у тебе в контексті)
            var selectedGenreIds = await _context.Moviegenres
                .Where(mg => mg.MovieId == id)
                .Select(mg => mg.GenreId)
                .ToListAsync();

            ViewBag.GenreId = new MultiSelectList(_context.Genres, "Id", "Name", selectedGenreIds);
            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile posterImage, int[] genreIds)
        {
            if (id != movie.Id) return NotFound();

            ModelState.Remove("PosterUrl");
            ModelState.Remove("Sessions");

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Оновлення картинки
                    if (posterImage != null && posterImage.Length > 0)
                    {
                        var result = await _imageService.UploadImageAsync(posterImage);
                        if (result.Success)
                        {
                            movie.PosterUrl = $"/images/{result.FileName}";
                        }
                    }
                    else
                    {
                        // Зберігаємо старий URL, якщо новий файл не вибрали
                        var oldMovie = await _context.Movies
                            .AsNoTracking()
                            .FirstOrDefaultAsync(m => m.Id == id);
                        movie.PosterUrl = oldMovie?.PosterUrl ?? "/images/no-poster.jpg";
                    }

                    _context.Update(movie);
                    await _context.SaveChangesAsync();

                    // 2. Оновлення жанрів (Видалити старі -> Додати нові)
                    var oldGenres = _context.Moviegenres.Where(mg => mg.MovieId == id);
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

            ViewBag.GenreId = new MultiSelectList(_context.Genres, "Id", "Name", genreIds);
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var movie = await _context.Movies
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
            if (movie != null)
            {
                // Видаляємо зв'язки жанрів вручну, щоб уникнути помилок БД
                var genres = _context.Moviegenres.Where(mg => mg.MovieId == id);
                _context.Moviegenres.RemoveRange(genres);

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