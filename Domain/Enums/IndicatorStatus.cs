using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Enums
{
    public enum IndicatorStatus
    {

        Red =0,       // Intense activity (first entry)
        Orange =1,    // Moderate (no entry 3 days)
        Yellow =2,    // Light (no entry 6 days)
        Green  =3     // No activity (no entry 7+ days)
    }
}
