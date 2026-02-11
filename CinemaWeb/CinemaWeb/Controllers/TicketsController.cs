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

        // GET: Tickets
        public async Task<IActionResult> Index(
            DateTime? searchDate,
            int? movieId,
            int? hallId)
        {
            var ticketsQuery = _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Session)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.Session)
                    .ThenInclude(s => s.Hall)
                .Include(t => t.User)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int userId = int.Parse(userIdStr);
                ticketsQuery = ticketsQuery.Where(t => t.UserId == userId);
            }

            // Фільтр по даті сеансу
            if (searchDate.HasValue)
            {
                ticketsQuery = ticketsQuery
                    .Where(t => t.Session.StartTime.Date == searchDate.Value.Date);
            }

            // Фільтр по фільму
            if (movieId.HasValue)
            {
                ticketsQuery = ticketsQuery
                    .Where(t => t.Session.MovieId == movieId);
            }

            // Фільтр по залу
            if (hallId.HasValue)
            {
                ticketsQuery = ticketsQuery
                    .Where(t => t.Session.HallId == hallId);
            }

            // Сортування: Спочатку найсвіжіші покупки (або сеанси)
            ticketsQuery = ticketsQuery
                .OrderByDescending(t => t.Session.StartTime);

            ViewData["MovieId"] = new SelectList(
                _context.Movies, "Id", "Title", movieId);

            ViewData["HallId"] = new SelectList(
                _context.Halls, "Id", "Name", hallId);

            ViewData["CurrentDate"] = searchDate?.ToString("yyyy-MM-dd");

            return View(await ticketsQuery.ToListAsync());
        }

        // GET: Tickets/Create?sessionId
        [HttpGet]
        public async Task<IActionResult> Create(int? sessionId)
        {
            if (User.IsInRole("Admin"))
            {
                TempData["Error"] = "Адміністратори не можуть купувати квитки! " +
                                    "Зайдіть як звичайний користувач.";
                return RedirectToAction("Index", "Home");
            }

            if (sessionId == null) return NotFound();

            var session = await _context.Sessions
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                    .ThenInclude(h => h.Seats)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound();

            var sessionTickets = await _context.Tickets
                .Where(t => t.SessionId == sessionId)
                .Select(t => new { t.SeatId, t.UserId })
                .ToListAsync();

            var takenSeatIds = sessionTickets
                .Select(t => t.SeatId)
                .ToList();

            var mySeatIds = new List<int>();
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdStr)
                && int.TryParse(userIdStr, out int userId))
            {
                mySeatIds = sessionTickets
                    .Where(t => t.UserId == userId)
                    .Select(t => t.SeatId)
                    .ToList();
            }

            ViewBag.TakenSeats = takenSeatIds;
            ViewBag.TicketPrice = session.Movie.Price;
            ViewBag.MySeats = mySeatIds;

            return View(session);
        }

        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int sessionId,
            string selectedSeatIds)
        {
            if (User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(selectedSeatIds))
            {
                return RedirectToAction("Create", new { sessionId = sessionId });
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdStr);

            List<int> seatIds;
            try
            {
                seatIds = selectedSeatIds.Split(',')
                    .Select(int.Parse)
                    .ToList();
            }
            catch
            {
                return BadRequest("Некоректні дані місць.");
            }

            var alreadyTaken = await _context.Tickets
                .AnyAsync(t => t.SessionId == sessionId
                               && seatIds.Contains(t.SeatId));

            if (alreadyTaken)
            {
                TempData["Error"] = "На жаль, одне з обраних місць вже " +
                                    "було придбано кимось іншим щойно!";
                return RedirectToAction("Create", new { sessionId = sessionId });
            }

            var newTickets = new List<Ticket>();
            foreach (var seatId in seatIds)
            {
                newTickets.Add(new Ticket
                {
                    SessionId = sessionId,
                    UserId = userId,
                    SeatId = seatId,
                    PurchaseDate = DateTime.Now,
                    Status = 1
                });
            }

            _context.Tickets.AddRange(newTickets);
            await _context.SaveChangesAsync();

            return RedirectToAction("Profile", "Account");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.Tickets
                .Include(t => t.Seat)
                .Include(t => t.Session)
                    .ThenInclude(s => s.Movie)
                .Include(t => t.Session)
                    .ThenInclude(s => s.Hall)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticket == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.Parse(userIdStr);

            if (!User.IsInRole("Admin") && ticket.UserId != userId)
            {
                return Forbid();
            }

            return View(ticket);
        }

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
            return RedirectToAction("Profile", "Account");
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