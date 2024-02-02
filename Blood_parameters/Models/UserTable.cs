using Blood_parameters.Models.Database;
using System;
using System.Collections.Generic;

namespace Blood_parameters.Models;

public class UserTable
{
    public User? add { get; set; }
    public User? update { get; set; }
    public int? fillUpdate { get; set; }
    public int? id { get; set; }
}

