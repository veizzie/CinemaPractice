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
    public class HallsController : Controller
    {
        private readonly CinemaDbContext _context;

        public HallsController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: Halls
        public async Task<IActionResult> Index()
        {
            // Сортуємо: спочатку активні, потім архівні
            return View(await _context.Halls
                .OrderBy(h => h.IsArchived)
                .ToListAsync());
        }

        // GET: Halls/Details/5
        public async Task<IActionResult> Details(byte? id)
        {
            if (id == null) return NotFound();

            var hall = await _context.Halls
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hall == null) return NotFound();

            return View(hall);
        }

        // GET: Halls/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Halls/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,RowsCount,ColsCount")] Hall hall,
            string SelectedSeats)
        {
            if (await _context.Halls.AnyAsync(h => h.Name == hall.Name))
            {
                ModelState.AddModelError("Name", "Такий зал вже існує!");
            }

            List<Seat> seatsToAdd = new List<Seat>();

            if (!string.IsNullOrEmpty(SelectedSeats))
            {
                var coords = SelectedSeats.Split(',');
                foreach (var coord in coords)
                {
                    var parts = coord.Split('-');
                    if (parts.Length == 2)
                    {
                        seatsToAdd.Add(new Seat
                        {
                            Row = byte.Parse(parts[0]),
                            Number = byte.Parse(parts[1])
                        });
                    }
                }
            }

            hall.Capacity = (short)seatsToAdd.Count;
            ModelState.Remove("Capacity");

            if (hall.Capacity > 150)
            {
                ModelState.AddModelError("",
                    $"Занадто велика зала! Максимум 150 місць, " +
                    $"а ви намалювали {hall.Capacity}");
                return View(hall);
            }

            if (ModelState.IsValid)
            {
                _context.Add(hall);
                await _context.SaveChangesAsync();

                foreach (var seat in seatsToAdd)
                {
                    seat.HallId = hall.Id;
                }

                if (seatsToAdd.Count > 0)
                {
                    _context.Seats.AddRange(seatsToAdd);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(hall);
        }

        // GET: Halls/Edit/5
        public async Task<IActionResult> Edit(byte? id)
        {
            if (id == null) return NotFound();

            var hall = await _context.Halls
                .Include(h => h.Seats)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hall == null) return NotFound();

            var existingSeatsCoords = hall.Seats
                .Select(s => s.Row + "-" + s.Number)
                .ToArray();

            ViewBag.ExistingSeats = string.Join(",", existingSeatsCoords);

            return View(hall);
        }

        // POST: Halls/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            byte id,
            [Bind("Id,Name,RowsCount,ColsCount")] Hall hall,
            string SelectedSeats)
        {
            if (id != hall.Id) return NotFound();

            var hallInDb = await _context.Halls
                .Include(h => h.Seats)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hallInDb == null) return NotFound();

            // 1. Формуємо список нових координат
            var newSeatCoords = string.IsNullOrEmpty(SelectedSeats)
                ? new HashSet<string>()
                : SelectedSeats.Split(',').ToHashSet();

            // 2. Отримуємо поточні координати з БД
            var existingSeatCoords = hallInDb.Seats
                .Select(s => s.Row + "-" + s.Number)
                .ToHashSet();

            // 3. Перевіряємо, чи змінилася конфігурація місць
            bool isLayoutChanged = !newSeatCoords.SetEquals(existingSeatCoords);

            if (isLayoutChanged)
            {
                // Якщо схема змінилася, перевіряємо, чи є майбутні сеанси
                bool hasActiveSessions = await _context.Sessions
                    .AnyAsync(s => s.HallId == id
                                   && s.StartTime > DateTime.Now);

                if (hasActiveSessions)
                {
                    ModelState.AddModelError("",
                        "Неможливо змінити розстановку місць! " +
                        "У цьому залі є заплановані сеанси. " +
                        "Зміна конфігурації може призвести до помилок.");

                    ViewBag.ExistingSeats = string.Join(",",
                        existingSeatCoords);
                    return View(hall);
                }
            }

            // Видаляємо старі місця
            var seatsToDelete = hallInDb.Seats
                .Where(s => !newSeatCoords.Contains(s.Row + "-" + s.Number))
                .ToList();

            if (seatsToDelete.Any())
            {
                _context.Seats.RemoveRange(seatsToDelete);
            }

            // Додаємо нові місця
            var seatsToAdd = new List<Seat>();

            foreach (var coord in newSeatCoords)
            {
                if (!existingSeatCoords.Contains(coord))
                {
                    var parts = coord.Split('-');
                    seatsToAdd.Add(new Seat
                    {
                        HallId = id,
                        Row = byte.Parse(parts[0]),
                        Number = byte.Parse(parts[1])
                    });
                }
            }

            if (seatsToAdd.Any())
            {
                _context.Seats.AddRange(seatsToAdd);
            }

            // Оновлюємо властивості залу
            hallInDb.Name = hall.Name;
            hallInDb.RowsCount = hall.RowsCount;
            hallInDb.ColsCount = hall.ColsCount;

            // Перераховуємо місткість
            hallInDb.Capacity = (short)(hallInDb.Seats.Count -
                seatsToDelete.Count + seatsToAdd.Count);

            ModelState.Remove("Capacity");

            if (hallInDb.Capacity > 150)
            {
                ModelState.AddModelError("",
                    "Занадто велика зала! Максимум 150 місць.");
                ViewBag.ExistingSeats = string.Join(",", existingSeatCoords);
                return View(hall);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HallExists(hall.Id)) return NotFound();
                    else throw;
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("",
                        "Помилка збереження. Можливо, на видалені місця " +
                        "існують посилання у квитках.");
                    ViewBag.ExistingSeats = string.Join(",",
                        existingSeatCoords);
                    return View(hall);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ExistingSeats = string.Join(",", existingSeatCoords);
            return View(hall);
        }

        // GET: Halls/Delete/5
        public async Task<IActionResult> Delete(byte? id)
        {
            if (id == null) return NotFound();

            var hall = await _context.Halls
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hall == null) return NotFound();

            return View(hall);
        }

        // POST: Halls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(byte id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Перевірка: чи є майбутні сеанси з проданими квитками?
            var hasActiveTickets = await _context.Tickets
                .Include(t => t.Session)
                .AnyAsync(t => t.Session.HallId == id
                               && t.Session.StartTime > DateTime.Now);

            if (hasActiveTickets)
            {
                ViewBag.ErrorMessage =
                    "Неможливо архівувати зал! У ньому заплановані сеанси, " +
                    "на які вже продано квитки.";
                return View("Delete", hall);
            }

            // Видаляємо пусті майбутні сеанси
            var futureEmptySessions = await _context.Sessions
                .Where(s => s.HallId == id && s.StartTime > DateTime.Now)
                .ToListAsync();

            if (futureEmptySessions.Any())
            {
                _context.Sessions.RemoveRange(futureEmptySessions);
            }

            // Архівуємо зал
            hall.IsArchived = true;
            _context.Halls.Update(hall);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool HallExists(byte id)
        {
            return _context.Halls.Any(e => e.Id == id);
        }
    }
}