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

        // GET: Sessions
        public async Task<IActionResult> Index()
        {
            var cinemaDbContext = _context.Sessions.Include(s => s.Hall).Include(s => s.Movie);
            return View(await cinemaDbContext.ToListAsync());
        }

        // GET: Sessions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (session == null)
            {
                return NotFound();
            }

            return View(session);
        }

        // GET: Sessions/Create
        public IActionResult Create()
        {
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name");
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title");
            return View();
        }

        // POST: Sessions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MovieId,HallId,StartTime")] Session session)
        {
            if (session.StartTime < DateTime.Now)
            {
                ModelState.AddModelError("StartTime", "Сеанс не може бути у минулому часі.");
            }

            ModelState.Remove("Movie");
            ModelState.Remove("Hall");

            if (ModelState.IsValid)
            {
                _context.Add(session);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", session.HallId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", session.MovieId);
            return View(session);
        }

        // GET: Sessions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", session.HallId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", session.MovieId);
            return View(session);
        }

        // POST: Sessions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,HallId,StartTime")] Session session)
        {
            if (id != session.Id)
            {
                return NotFound();
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
                    if (!SessionExists(session.Id))
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

            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", session.HallId);
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", session.MovieId);
            return View(session);
        }

        // GET: Sessions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var session = await _context.Sessions
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (session == null)
            {
                return NotFound();
            }

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
            ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name");
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title");

            // Створюємо модель зі значеннями за замовчуванням
            var model = new GroupSessionViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7),
                Time = new TimeSpan(19, 0, 0) // 19:00
            };

            return View(model);
        }

        // POST: Sessions/CreateGroup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(GroupSessionViewModel model)
        {
            // Валідація дат
            if (model.EndDate < model.StartDate)
            {
                ModelState.AddModelError("EndDate", "Дата завершення не може бути раніше дати початку.");
            }

            if (model.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("StartDate", "Дата початку не може бути в минулому.");
            }

            // Перевірка чи вибрано хоча б один день
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
                ViewData["HallId"] = new SelectList(_context.Halls, "Id", "Name", model.HallId);
                ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", model.MovieId);
                return View(model);
            }

            var newSessions = new List<Session>();
            var conflictDates = new List<DateTime>();

            // Проходимо по кожному дню в діапазоні
            for (var date = model.StartDate.Date; date <= model.EndDate.Date; date = date.AddDays(1))
            {
                // Перевіряємо чи це вибраний день тижня
                if (!selectedDays.Contains(date.DayOfWeek))
                    continue;

                var sessionDateTime = date + model.Time;

                // Перевірка, чи не минулий час
                if (sessionDateTime < DateTime.Now)
                {
                    conflictDates.Add(date);
                    continue;
                }

                // Перевірка зайнятості залу
                bool isHallBusy = await _context.Sessions
                    .AnyAsync(s => s.HallId == model.HallId
                        && s.StartTime.Date == date
                        && s.StartTime.TimeOfDay == model.Time);

                if (isHallBusy)
                {
                    conflictDates.Add(date);
                    continue;
                }

                // Створюємо новий сеанс
                var session = new Session
                {
                    MovieId = model.MovieId,
                    HallId = model.HallId,
                    StartTime = sessionDateTime
                };

                newSessions.Add(session);
            }

            // Зберігаємо сеанси
            if (newSessions.Count > 0)
            {
                await _context.Sessions.AddRangeAsync(newSessions);
                await _context.SaveChangesAsync();
            }

            // Формуємо повідомлення
            if (conflictDates.Count > 0 && newSessions.Count > 0)
            {
                var conflictDays = string.Join(", ", conflictDates.Select(d => d.ToString("dd.MM")));
                TempData["Warning"] = $"Успішно створено {newSessions.Count} сеансів. " +
                                     $"Не створено для дат: {conflictDays} (зал зайнятий або минулий час).";
            }
            else if (newSessions.Count > 0)
            {
                TempData["Success"] = $"Успішно створено {newSessions.Count} сеансів!";
            }
            else if (conflictDates.Count > 0)
            {
                TempData["Error"] = "Не вдалося створити жодного сеансу. Усі вибрані дні зайняті або в минулому.";
            }
            else
            {
                TempData["Error"] = "Не вдалося створити жодного сеансу.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SessionExists(int id)
        {
            return _context.Sessions.Any(e => e.Id == id);
        }
    }
}