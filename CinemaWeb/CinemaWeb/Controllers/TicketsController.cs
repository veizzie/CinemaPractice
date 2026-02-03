using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CinemaWeb.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CinemaWeb.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly CinemaDbContext _context;

        public TicketsController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: Tickets (Показує квитки поточного користувача)
        public async Task<IActionResult> Index()
        {
            // Отримуємо ID поточного користувача
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdStr);

            // Якщо це Адмін - показуємо всі квитки, якщо Юзер - тільки свої
            var query = _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Session)
                .ThenInclude(s => s.Movie) // Підтягуємо назву фільму
                .Include(t => t.User)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(t => t.UserId == userId);
            }

            return View(await query.OrderByDescending(t => t.PurchaseDate).ToListAsync());
        }

        // GET: Tickets/Create?sessionId
        // Сторінка з візуальною картою залу
        [HttpGet]
        public async Task<IActionResult> Create(int? sessionId)
        {
            if (sessionId == null) return NotFound();

            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .ThenInclude(h => h.Seats) // вантажимо місця для малювання карти
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound();

            // Визначення ID місць, які вже зайняті на цей сеанс
            var sessionTickets = await _context.Tickets
                .Where(t => t.SessionId == sessionId)
                .Select(t => new {t.SeatId, t.UserId})
                .ToListAsync();

            var takenSeatIds = sessionTickets.Select(t => t.SeatId).ToList();
            var mySeatIds = new List<int>();
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
            {
                mySeatIds = sessionTickets
                    .Where(t => t.UserId == userId)
                    .Select(t => t.SeatId)
                    .ToList();
            }

            ViewBag.TakenSeats = takenSeatIds; // Список зайнятих передаємо у View
            ViewBag.TicketPrice = session.Movie.Price; // Передаємо ціну квитка
            ViewBag.MySeats = mySeatIds; // Список місць поточного користувача

            return View(session);
        }

        // POST: Tickets/Create
        // Обробка покупки (приймає список ID місць через кому)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int sessionId, string selectedSeatIds)
        {
            if (string.IsNullOrEmpty(selectedSeatIds))
            {
                return RedirectToAction("Create", new { sessionId = sessionId });
            }

            // Отримуємо ID поточного користувача
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdStr);

            // Парсинг ID місць ("5,8,12" -> [5, 8, 12])
            List<int> seatIds;
            try
            {
                seatIds = selectedSeatIds.Split(',').Select(int.Parse).ToList();
            }
            catch
            {
                return BadRequest("Некоректні дані місць.");
            }

            // ПЕРЕВІРКА: чи не купив хтось ці місця за секунду до нас
            var alreadyTaken = await _context.Tickets
                .AnyAsync(t => t.SessionId == sessionId && seatIds.Contains(t.SeatId));

            if (alreadyTaken)
            {
                TempData["Error"] = "На жаль, одне з обраних місць вже було придбано кимось іншим щойно!";
                return RedirectToAction("Create", new { sessionId = sessionId });
            }

            // Створюємо квитки
            var newTickets = new List<Ticket>();
            foreach (var seatId in seatIds)
            {
                newTickets.Add(new Ticket
                {
                    SessionId = sessionId,
                    UserId = userId,
                    SeatId = seatId,
                    PurchaseDate = DateTime.Now,
                    Status = 1 // 1 = Активний/Оплачений
                });
            }

            _context.Tickets.AddRange(newTickets);
            await _context.SaveChangesAsync();

            // Перенаправляємо в профіль
            return RedirectToAction("Profile", "Account");
        }

        // GET: Tickets/Delete/5
        // Дозволяємо видаляти (скасовувати) квиток
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Session)
                .ThenInclude(s => s.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();

            // Перевірка: чи це квиток поточного користувача (або адміна)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdStr);

            if (!User.IsInRole("Admin") && ticket.UserId != userId)
            {
                return Forbid(); // Заборонено видаляти чужі квитки
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Session)
                .ThenInclude(s => s.Movie)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();

            // Захист від перегляду чужих квитків
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdStr);
            if (!User.IsInRole("Admin") && ticket.UserId != userId)
            {
                return Forbid();
            }

            return View(ticket);
        }
    }
}