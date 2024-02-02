using Blood_parameters.Models.Database;
using System;
using System.Collections.Generic;

namespace Blood_parameters.Models;

public class DoctorAndPicture
{
    public Doctor? doctor { get; set; }
    public IFormFile? picture { get; set; }
}

