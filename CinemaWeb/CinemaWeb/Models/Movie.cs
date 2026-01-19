using System;
using System.Collections.Generic;

namespace CinemaWeb.Models;

public partial class Movie
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Director { get; set; } = null!;

    public string Cast { get; set; } = null!;

    public short Duration { get; set; }

    public DateOnly ReleaseDate { get; set; }

    public string PosterUrl { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
