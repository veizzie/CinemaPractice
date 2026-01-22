using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CinemaWeb.Models;

public partial class Session
{
    public int Id { get; set; }

    [Display(Name = "Назва фільму")] // Відображення тексту "Назва фільму" у формі, замість "Name"
    public int MovieId { get; set; }

    [Display(Name = "Назва залу")]
    public byte HallId { get; set; }

    [Display(Name = "Час початку сеансу")]
    public DateTime StartTime { get; set; }

    [Display(Name = "Назва залу")]
    public virtual Hall Hall { get; set; } = null!;

    [Display(Name = "Назва фільму")]
    public virtual Movie Movie { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
