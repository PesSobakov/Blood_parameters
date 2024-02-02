using Blood_parameters.Models.Database;
using System;
using System.Collections.Generic;

namespace Blood_parameters.Models;

public class ParameterTable
{
    public Parameter? add { get; set; }
    public Parameter? update { get; set; }
    public int? fillUpdate { get; set; }
    public int? id { get; set; }
}

