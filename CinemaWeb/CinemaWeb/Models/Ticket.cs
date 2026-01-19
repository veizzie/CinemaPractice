using System;
using System.Collections.Generic;

namespace CinemaWeb.Models;

public partial class Ticket
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SessionId { get; set; }

    public int SeatId { get; set; }

    public byte Status { get; set; }

    public DateTime PurchaseDate { get; set; }

    public virtual Seat Seat { get; set; } = null!;

    public virtual Session Session { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
