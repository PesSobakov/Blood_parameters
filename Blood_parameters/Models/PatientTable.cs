using Blood_parameters.Models.Database;
using System;
using System.Collections.Generic;

namespace Blood_parameters.Models;

public class PatientTable
{
    public Patient? add { get; set; }
    public Patient? update { get; set; }
    public int? fillUpdate { get; set; }
    public int? id { get; set; }
}

