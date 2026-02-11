using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введіть Email")]
    [Display(Name = "Електронна пошта")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Введіть пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; }

    [Display(Name = "Запам'ятати мене?")]
    public bool RememberMe { get; set; }
}