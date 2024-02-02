using Blood_parameters.Models.Database;
using System;
using System.Collections.Generic;

namespace Blood_parameters.Models;

public class MedicationsTable
{
    public Medication? add { get; set; }
    public int? id { get; set; }
    public int? fillUpdate { get; set; }
    public Medication? update { get; set; }
}

