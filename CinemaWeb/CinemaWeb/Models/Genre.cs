using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.Models;

public partial class Genre
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Назва жанру є обов'язковою!")] //Перевірка наявності назви жанру
    [Display(Name = "Назва жанру")] //Відображення тексту "Назва жанру" у формі, замість "Name"
    public string Name { get; set; } = null!;

    public virtual ICollection<Moviegenre> Moviegenres { get; set; } = new List<Moviegenre>();
}
