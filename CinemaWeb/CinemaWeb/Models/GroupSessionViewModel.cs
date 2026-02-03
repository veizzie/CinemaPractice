using System;
using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.Models
{
    public class GroupSessionViewModel
    {
        [Required(ErrorMessage = "Оберіть фільм")]
        [Display(Name = "Фільм")]
        public int MovieId { get; set; }

        [Required(ErrorMessage = "Оберіть зал")]
        [Display(Name = "Зал")]
        public byte HallId { get; set; }  // ЗМІНЕНО з int на byte!

        [Required(ErrorMessage = "Вкажіть дату початку")]
        [Display(Name = "Дата початку")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Вкажіть дату завершення")]
        [Display(Name = "Дата завершення")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "Вкажіть час сеансу")]
        [Display(Name = "Час сеансу")]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; } = new TimeSpan(19, 0, 0);

        // Чекбокси для днів тижня
        [Display(Name = "Понеділок")]
        public bool Monday { get; set; } = true;

        [Display(Name = "Вівторок")]
        public bool Tuesday { get; set; } = true;

        [Display(Name = "Середа")]
        public bool Wednesday { get; set; } = true;

        [Display(Name = "Четвер")]
        public bool Thursday { get; set; } = true;

        [Display(Name = "П'ятниця")]
        public bool Friday { get; set; } = true;

        [Display(Name = "Субота")]
        public bool Saturday { get; set; } = true;

        [Display(Name = "Неділя")]
        public bool Sunday { get; set; } = true;
    }
}