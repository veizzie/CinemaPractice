using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace CinemaWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SessionsController : Controller
    {
        private readonly CinemaDbContext _context;

        public SessionsController(CinemaDbContext context)
        {
            _context = context;
        }

        private async Task<bool> IsHallBusy(
            int hallId,
            DateTime newStart,
            int movieId,
            int? excludeSessionId = null)
        {
            // Дізнаємося тривалість фільму, який хочемо поставити
            var newMovie = await _context.Movies.FindAsync(movieId);
            if (newMovie == null) return false;

            // Розраховуємо час закінчення нового сеансу
            var newEnd = newStart.AddMinutes(newMovie.Duration);

            // Шукаємо конфлікти в базі даних
            return await _context.Sessions
                .Include(s => s.Movie)
                .Where(s => s.HallId == hallId && s.Id != excludeSessionId)
                .AnyAsync(s =>
                    newStart < s.StartTime.AddMinutes(s.Movie.Duration) &&
                    newEnd > s.StartTime
                );
        }

        // GET: Sessions
        public async Task<IActionResult> Index(
            DateTime? searchDate,
            int? movieId,
            int? hallId)
        {
            var sessionsQuery = _context.Sessions
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .AsQueryable();

            // Фільтр по даті
            if (searchDate.HasValue)
            {
                sessionsQuery = sessionsQuery
                    .Where(s => s.StartTime.Date == searchDate.Value.Date);
            }

            // Фільтр по фільму
            if (movieId.HasValue)
            {
                sessionsQuery = sessionsQuery
                    .Where(s => s.MovieId == movieId);
            }

            // Фільтр по залу
            if (hallId.HasValue)
            {
                sessionsQuery = sessionsQuery
                    .Where(s => s.HallId == hallId);
            }

            // Сортування: Спочатку найближчі сеанси
            sessionsQuery = sessionsQuery.OrderBy(s => s.StartTime);

            ViewData["MovieId"] = new SelectList(
                _context.Movies, "Id", "Title", movieId);

            ViewData["HallId"] = new SelectList(
                _context.Halls, "Id", "Name", hallId);

            ViewData["CurrentDate"] = searchDate?.ToString("yyyy-MM-dd");

            return View(await sessionsQuery.ToListAsync());
        }

        // GET: Sessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Seats)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (session == null) return NotFound();

            var takenSeatIds = await _context.Tickets
                .Where(t => t.SessionId == session.Id)
                .Select(t => t.SeatId)
                .ToListAsync();

            ViewBag.TakenSeats = takenSeatIds;

            return View(session);
        }

        // GET: Sessions/Create
        public IActionResult Create()
        {
            // Тільки активні зали
            ViewData["HallId"] = new SelectList(
                _context.Halls.Where(h => !h.IsArchived),
                "Id", "Name");

            // Тільки активні фільми
            ViewData["MovieId"] = new SelectList(
                _context.Movies.Where(m => !m.IsArchived),
                "Id", "Title");

            return View();
        }

        // POST: Sessions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,MovieId,HallId,StartTime")] Session session)
        {
            if (session.StartTime < DateTime.Now)
            {
                ModelState.AddModelError("StartTime",
                    "Сеанс не може бути у минулому часі.");
            }

            if (await IsHallBusy(
                session.HallId,
                session.StartTime,
                session.MovieId))
            {
                ModelState.AddModelError("StartTime",
                    "У цей час (або під час показу фільму) зал зайнятий!");
            }

            ModelState.Remove("Movie");
            ModelState.Remove("Hall");

            if (ModelState.IsValid)
            {
                _context.Add(session);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["HallId"] = new SelectList(
                _context.Halls, "Id", "Name", session.HallId);

            ViewData["MovieId"] = new SelectList(
                _context.Movies, "Id", "Title", session.MovieId);

            return View(session);
        }

        // GET: Sessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.Sessions.FindAsync(id);
            if (session == null) return NotFound();

            ViewData["HallId"] = new SelectList(
                _context.Halls.Where(h => !h.IsArchived),
                "Id", "Name", session.HallId);

            ViewData["MovieId"] = new SelectList(
                _context.Movies.Where(m => !m.IsArchived),
                "Id", "Title", session.MovieId);

            return View(session);
        }

        // POST: Sessions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,MovieId,HallId,StartTime")] Session session)
        {
            if (id != session.Id) return NotFound();

            if (await IsHallBusy(
                session.HallId,
                session.StartTime,
                session.MovieId,
                session.Id))
            {
                ModelState.AddModelError("StartTime",
                    "Конфлікт часу! Зал зайнятий іншим фільмом.");
            }

            ModelState.Remove("Movie");
            ModelState.Remove("Hall");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(session);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SessionExists(session.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["HallId"] = new SelectList(
                _context.Halls, "Id", "Name", session.HallId);

            ViewData["MovieId"] = new SelectList(
                _context.Movies, "Id", "Title", session.MovieId);

            return View(session);
        }

        // GET: Sessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var session = await _context.Sessions
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (session == null) return NotFound();

            return View(session);
        }

        // POST: Sessions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session != null)
            {
                _context.Sessions.Remove(session);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Sessions/CreateGroup
        public IActionResult CreateGroup()
        {
            ViewData["HallId"] = new SelectList(
                _context.Halls.Where(h => !h.IsArchived), "Id", "Name");

            ViewData["MovieId"] = new SelectList(
                _context.Movies.Where(m => !m.IsArchived), "Id", "Title");

            var model = new GroupSessionViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                Time = new TimeSpan(19, 0, 0)
            };

            return View(model);
        }

        // POST: Sessions/CreateGroup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(GroupSessionViewModel model)
        {
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate",
                    "Дата завершення не може бути раніше дати початку.");
            }

            if (model.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("StartDate",
                    "Дата початку не може бути в минулому.");
            }

            var selectedDays = new List<DayOfWeek>();
            if (model.Monday) selectedDays.Add(DayOfWeek.Monday);
            if (model.Tuesday) selectedDays.Add(DayOfWeek.Tuesday);
            if (model.Wednesday) selectedDays.Add(DayOfWeek.Wednesday);
            if (model.Thursday) selectedDays.Add(DayOfWeek.Thursday);
            if (model.Friday) selectedDays.Add(DayOfWeek.Friday);
            if (model.Saturday) selectedDays.Add(DayOfWeek.Saturday);
            if (model.Sunday) selectedDays.Add(DayOfWeek.Sunday);

            if (selectedDays.Count == 0)
            {
                ModelState.AddModelError("", "Оберіть хоча б один день тижня.");
            }

            if (!ModelState.IsValid)
            {
                ViewData["HallId"] = new SelectList(
                    _context.Halls, "Id", "Name", model.HallId);

                ViewData["MovieId"] = new SelectList(
                    _context.Movies, "Id", "Title", model.MovieId);

                return View(model);
            }

            var newSessions = new List<Session>();
            var conflictDates = new List<DateTime>();

            for (var date = model.StartDate.Date;
                 date <= model.EndDate.Date;
                 date = date.AddDays(1))
            {
                if (!selectedDays.Contains(date.DayOfWeek)) continue;

                var sessionDateTime = date + model.Time;

                if (sessionDateTime < DateTime.Now)
                {
                    conflictDates.Add(date);
                    continue;
                }

                bool isBusy = await IsHallBusy(
                    model.HallId, sessionDateTime, model.MovieId);

                if (isBusy)
                {
                    conflictDates.Add(date);
                    continue;
                }

                var session = new Session
                {
                    MovieId = model.MovieId,
                    HallId = model.HallId,
                    StartTime = sessionDateTime
                };

                newSessions.Add(session);
            }

            if (newSessions.Count > 0)
            {
                await _context.Sessions.AddRangeAsync(newSessions);
                await _context.SaveChangesAsync();
            }

            if (conflictDates.Count > 0 && newSessions.Count > 0)
            {
                var conflictDays = string.Join(", ",
                    conflictDates.Select(d => d.ToString("dd.MM")));

                TempData["Warning"] =
                    $"Успішно створено {newSessions.Count} сеансів. " +
                    $"Не створено для дат: {conflictDays} (зал зайнятий або час).";
            }
            else if (newSessions.Count > 0)
            {
                TempData["Success"] =
                    $"Успішно створено {newSessions.Count} сеансів!";
            }
            else
            {
                TempData["Error"] =
                    "Не вдалося створити жодного сеансу. Усі вибрані дні зайняті.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SessionExists(int id)
        {
            return _context.Sessions.Any(e => e.Id == id);
        }
    }
}