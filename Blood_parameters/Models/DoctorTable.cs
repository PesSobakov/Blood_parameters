using Blood_parameters.Models.Database;
using System;
using System.Collections.Generic;

namespace Blood_parameters.Models;

public class DoctorTable
{
    public Doctor? add { get; set; }
    public Doctor? update { get; set; }
    public int? fillUpdate { get; set; }
    public int? id { get; set; }
    public IFormFile? picture { get; set; }
    public int? pictureId { get; set; }

}

