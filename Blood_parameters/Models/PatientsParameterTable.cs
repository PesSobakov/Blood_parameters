using Blood_parameters.Models.Database;
using System;
using System.Collections.Generic;

namespace Blood_parameters.Models;

public class PatientsParameterTable
{
    public PatientsParameter? add { get; set; }
    public PatientsParameter? update { get; set; }
    public int? fillUpdate { get; set; }
    public int? id { get; set; }
}

