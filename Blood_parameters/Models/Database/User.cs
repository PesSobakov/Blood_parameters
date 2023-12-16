using System;
using System.Collections.Generic;

namespace Blood_parameters.Models.Database;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? PatientId { get; set; }

    public int? DoctorId { get; set; }

    public int RoleId { get; set; }

    public virtual Doctor? Doctor { get; set; }

    public virtual Patient? Patient { get; set; }

    public virtual Role? Role { get; set; } = null!;
}
