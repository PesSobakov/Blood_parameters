using System;
using System.Collections.Generic;

namespace Blood_parameters.Models.Database;

public partial class Parameter
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Norm> Norms { get; set; } = new List<Norm>();

    public virtual ICollection<PatientsParameter> PatientsParameters { get; set; } = new List<PatientsParameter>();
}
