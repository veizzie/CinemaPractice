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
            ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile posterImage, int[] genreIds)
        {
            ModelState.Remove("PosterUrl");
            ModelState.Remove("Sessions");

            if (ModelState.IsValid)
            {
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
                        ViewBag.Genres = new SelectList(_context.Genres, "Id", "Name");
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
                        var oldMovie = await _context.Movies
                            .AsNoTracking()
                            .FirstOrDefaultAsync(m => m.Id == id);
                        movie.PosterUrl = oldMovie?.PosterUrl ?? "/images/no-poster.jpg";
                    }

                    _context.Update(movie);
                    await _context.SaveChangesAsync();

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