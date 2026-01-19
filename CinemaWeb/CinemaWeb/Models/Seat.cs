using System;
using System.Collections.Generic;

namespace CinemaWeb.Models;

public partial class Seat
{
    public int Id { get; set; }

    public byte HallId { get; set; }

    public byte Row { get; set; }

    public byte Number { get; set; }

    public virtual Hall Hall { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
