using System;
using System.Collections.Generic;

namespace CinemaWeb.Models;

public partial class Hall
{
    public byte Id { get; set; }

    public string Name { get; set; } = null!;

    public short Capacity { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
