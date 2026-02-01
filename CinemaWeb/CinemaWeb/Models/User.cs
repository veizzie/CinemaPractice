using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaWeb.Models;

public partial class User : IdentityUser<int>
{
    public string FullName { get; set; } = null!;

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    [NotMapped]
    public string Role { get; set; } = "User";
}
