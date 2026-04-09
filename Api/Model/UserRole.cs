using System;
using System.Collections.Generic;

namespace PlantProduction.Api.Model;

public partial class UserRole
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
}
