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
    [Authorize(Roles = "Admin")] // Тільки для адміністраторів
    public class UsersController : Controller
    {
        private readonly CinemaDbContext _context;

        public UsersController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            foreach (var user in users)
            {
                user.Role = user.Email != null &&
                           user.Email.Contains("admin", StringComparison.OrdinalIgnoreCase)
                           ? "Admin" : "User";
            }

            return View(users);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Role = user.Email != null &&
                       user.Email.Contains("admin", StringComparison.OrdinalIgnoreCase)
                       ? "Admin" : "User";

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "User", Text = "Користувач" },
                new SelectListItem { Value = "Admin", Text = "Адміністратор" }
            };

            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Email,FullName,Role")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.FindAsync(id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    // Оновлюємо поля
                    existingUser.FullName = user.FullName;

                    // Змінюємо email в залежності від ролі
                    if (user.Role == "Admin" && !existingUser.Email.Contains("admin", StringComparison.OrdinalIgnoreCase))
                    {
                        existingUser.Email += ".admin";
                        existingUser.UserName += ".admin";
                    }
                    else if (user.Role == "User" && existingUser.Email.Contains("admin", StringComparison.OrdinalIgnoreCase))
                    {
                        existingUser.Email = existingUser.Email.Replace(".admin", "");
                        existingUser.UserName = existingUser.UserName.Replace(".admin", "");
                    }

                    _context.Update(existingUser);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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

            ViewBag.Roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "User", Text = "Користувач" },
                new SelectListItem { Value = "Admin", Text = "Адміністратор" }
            };

            return View(user);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}