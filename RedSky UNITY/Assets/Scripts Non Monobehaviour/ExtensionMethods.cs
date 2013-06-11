using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class ExtensionMethods
{

    public static double DegreeToRadians(this double degrees)
    {
        return degrees*(180.0f/Math.PI);
    }

}

