using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.Models;

public partial class Hall
{
    public byte Id { get; set; }


    [Required(ErrorMessage = "Назва залу є обов'язковою")]
    [Display(Name = "Назва залу")] // Відображення тексту "Назва залу" у формі, замість "Name"
    [StringLength(50, ErrorMessage = "Назва занадто довга")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Вкажіть місткість залу")]
    [Display(Name = "Кількість місць")] // Відображення тексту "Кількість місць" у формі, замість "Name"
    [Range(10, 150, ErrorMessage = "Місткість має бути від 10 до 150 місць")]
    public short Capacity { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
