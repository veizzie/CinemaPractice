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
        [StringLength(100, ErrorMessage = "{0} повинен мати мінімум {2} символів.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "Пароль має містити: мінімум 8 символів, велику літеру, цифру та спец. символ")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження пароля")]
        [Compare("NewPassword", ErrorMessage = "Паролі не співпадають.")]
        public string ConfirmNewPassword { get; set; }

        [Display(Name = "Номер телефону")]
        [Phone(ErrorMessage = "Некоректний формат телефону")]
        [RegularExpression(@"^(\+?380|0)?\d{9}$", ErrorMessage = "Введіть номер у форматі +380xxxxxxxxx")]
        public string? PhoneNumber { get; set; }
    }
}