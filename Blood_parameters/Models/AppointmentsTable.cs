using Blood_parameters.Models.Database;
using System;
using System.Collections.Generic;

namespace Blood_parameters.Models;

public class AppointmentsTable
{
    public Appointment? add { get; set; }
    public int? id { get; set; }
    public int? fillUpdate { get; set; }
    public Appointment? update { get; set; }
}

