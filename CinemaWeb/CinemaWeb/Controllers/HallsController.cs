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
            return View(await _context.Halls.ToListAsync());
        }

        // GET: Halls/Details/5
        public async Task<IActionResult> Details(byte? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hall = await _context.Halls
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hall == null)
            {
                return NotFound();
            }

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
        // Додав RowsCount, ColsCount у Bind та параметр SelectedSeats для реалізації вибору місць
        public async Task<IActionResult> Create([Bind("Id,Name,RowsCount,ColsCount")] Hall hall, string SelectedSeats)
        {
            // Перевірка, чи є вже зал з такою назвою
            if (await _context.Halls.AnyAsync(h => h.Name == hall.Name))
            {
                ModelState.AddModelError("Name", "Такий зал вже існує!");
            }

            // Обробка сітки місць
            List<Seat> seatsToAdd = new List<Seat>();

            if (!string.IsNullOrEmpty(SelectedSeats))
            {
                // Тут розбивка рядка на окремі координати "1-1, 1-2, 2-4" і т.д.
                var coords = SelectedSeats.Split(',');

                foreach (var coord in coords)
                {
                    var parts = coord.Split('-');
                    if (parts.Length == 2)
                    {
                        // Створюю ою'єкт місця в пам'яті
                        seatsToAdd.Add(new Seat
                        {
                            Row = byte.Parse(parts[0]),
                            Number = byte.Parse(parts[1])
                        });
                    }
                }
            }

            // Зробив автоматичний розрахунок місткості
            hall.Capacity = (short)seatsToAdd.Count;
            ModelState.Remove("Capacity");

            // Перевірка ліміту
            if (hall.Capacity > 150)
            {
                ModelState.AddModelError("", $"Занадто велика зала! Максимум 150 місць, а ви намалювали {hall.Capacity}");
                return View(hall);
            }

            if (ModelState.IsValid)
            {
                _context.Add(hall);
                await _context.SaveChangesAsync();

                // Прив'язка місця до залу
                foreach (var seat in seatsToAdd)
                {
                    seat.HallId = hall.Id;
                }

                // Збереження місць
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

            // Завантаження залу РАЗОМ з місцями
            var hall = await _context.Halls
                .Include(h => h.Seats)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hall == null) return NotFound();

            // Формування списку координат існуючих місць ("1-1,1-2,2-5")
            // Щоб JS знав, які квадратики малювати зеленими
            var existingSeatsCoords = hall.Seats
                .Select(s => s.Row + "-" + s.Number)
                .ToArray();

            ViewBag.ExistingSeats = string.Join(",", existingSeatsCoords);

            return View(hall);
        }

        // POST: Halls/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(byte id, [Bind("Id,Name,RowsCount,ColsCount")] Hall hall, string SelectedSeats)
        {
            if (id != hall.Id) return NotFound();

            // Отримання актуальних місць з бази (щоб знати, що видаляти)
            var hallInDb = await _context.Halls
                .Include(h => h.Seats)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hallInDb == null) return NotFound();

            // Парсинг нових координати з форми
            var newSeatCoords = string.IsNullOrEmpty(SelectedSeats)
                ? new HashSet<string>()
                : SelectedSeats.Split(',').ToHashSet();

            // Логіка СИНХРОНІЗАЦІЇ

            // А) Видалення місць, яких більше немає в новій схемі
            var seatsToDelete = hallInDb.Seats
                .Where(s => !newSeatCoords.Contains(s.Row + "-" + s.Number))
                .ToList();

            if (seatsToDelete.Any())
            {
                _context.Seats.RemoveRange(seatsToDelete);
            }

            // Б) Додавання нових місць, яких раніше не було
            var existingCoords = hallInDb.Seats.Select(s => s.Row + "-" + s.Number).ToHashSet();
            var seatsToAdd = new List<Seat>();

            foreach (var coord in newSeatCoords)
            {
                // Якщо такого місця ще немає в базі - створюємо
                if (!existingCoords.Contains(coord))
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

            // Оновлення параметрів залу
            hallInDb.Name = hall.Name;
            hallInDb.RowsCount = hall.RowsCount;
            hallInDb.ColsCount = hall.ColsCount;

            // Перераховуємо місткість: (старі - видалені + нові)
            hallInDb.Capacity = (short)(hallInDb.Seats.Count - seatsToDelete.Count + seatsToAdd.Count);

            ModelState.Remove("Capacity"); // Ігноруємо помилку валідації

            if (hallInDb.Capacity > 150)
            {
                ModelState.AddModelError("", $"Занадто велика зала! Максимум 150 місць, а ви намалювали {hallInDb.Capacity}");

                var existingSeatsCoords = hallInDb.Seats.Select(s => s.Row + "-" + s.Number).ToArray();
                ViewBag.ExistingSeats = string.Join(",", existingSeatsCoords);

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
                return RedirectToAction(nameof(Index));
            }
            return View(hall);
        }

        // GET: Halls/Delete/5
        public async Task<IActionResult> Delete(byte? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hall = await _context.Halls
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hall == null)
            {
                return NotFound();
            }

            return View(hall);
        }

        // POST: Halls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(byte id)
        {
            var hall = await _context.Halls
                .Include(h => h.Seats)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (hall != null)
            {
                // Видалення місць
                if (hall.Seats != null && hall.Seats.Any())
                {
                    _context.Seats.RemoveRange(hall.Seats);
                }

                // Тепер видаляємо зал
                _context.Halls.Remove(hall);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HallExists(byte id)
        {
            return _context.Halls.Any(e => e.Id == id);
        }
    }
}
