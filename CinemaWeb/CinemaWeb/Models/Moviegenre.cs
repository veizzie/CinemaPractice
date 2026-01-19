using System;
using System.Collections.Generic;

namespace CinemaWeb.Models;

public partial class Moviegenre
{
    public int MovieId { get; set; }

    public int GenreId { get; set; }

    public virtual Genre Genre { get; set; } = null!;

    public virtual Movie Movie { get; set; } = null!;
}
