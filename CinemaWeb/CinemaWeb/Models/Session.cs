using System;
using System.Collections.Generic;

namespace CinemaWeb.Models;

public partial class Session
{
    public int Id { get; set; }

    public int MovieId { get; set; }

    public byte HallId { get; set; }

    public DateTime StartTime { get; set; }

    public virtual Hall Hall { get; set; } = null!;

    public virtual Movie Movie { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
