using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введіть Email")]
    [EmailAddress(ErrorMessage = "Некоректний Email")]
    [Display(Name = "Електронна пошта")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Введіть повне ім'я")]
    [Display(Name = "Повне ім'я (Ім'я та прізвище)")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Введіть пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    [StringLength(100, ErrorMessage = "{0} повинен мати мінімум {2} символів.", MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
        ErrorMessage = "Пароль має містити: мінімум 8 символів, велику літеру, цифру та спец. символ")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Підтвердження пароля")]
    [Compare("Password", ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; }
}