using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Введіть поточний пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Поточний пароль")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Введіть новий пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Новий пароль")]
        [MinLength(4, ErrorMessage = "Новий пароль повинен містити щонайменше 4 символи")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердіть новий пароль")]
        [Compare("NewPassword", ErrorMessage = "Паролі не співпадають")]
        public string ConfirmNewPassword { get; set; }
    }
}
