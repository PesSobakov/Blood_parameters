using Blood_parameters.Models.Database;
using System;
using System.Collections.Generic;

namespace Blood_parameters.Models;

public class RegisterPatient
{
    public Patient patient { get; set; }
    public User user { get; set; }
}

