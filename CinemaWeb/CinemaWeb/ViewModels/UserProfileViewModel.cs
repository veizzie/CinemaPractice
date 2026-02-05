using CinemaWeb.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.ViewModels
{
    public class UserProfileViewModel
    {
        // --- Інфо про користувача ---
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public string Role { get; set; }

        // --- Списки квитків ---
        public List<Ticket> ActiveTickets { get; set; } = new List<Ticket>();
        public List<Ticket> HistoryTickets { get; set; } = new List<Ticket>();

        // --- Зміна пароля ---
        [DataType(DataType.Password)]
        [Display(Name = "Поточний пароль")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Новий пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження пароля")]
        [Compare("NewPassword", ErrorMessage = "Паролі не співпадають.")]
        public string ConfirmNewPassword { get; set; }
    }
}