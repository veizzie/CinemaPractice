using System;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CinemaWeb.Models;

public partial class User : IdentityUser<int>
{
    public string FullName { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
