using Blood_parameters.Models.Database;
using Castle.Components.DictionaryAdapter;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Blood_parameters.Models;

public class Image
{
    static public string ToSrc(byte[]? img)
    {
        if (img != null)
        {
            return "data:image/png;base64," + Convert.ToBase64String(img, 0, img.Length);
        }
        else return "";
    }
}

