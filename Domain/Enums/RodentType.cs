using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Enums
{
    public enum RodentType
    {
        
        // NormalRat ,

        //ClimbingRat,
        //NorwegianRat,
        //Unknown
        

        [Display(Name = "فار منزلى")]
        NormalRat ,

        [Display(Name = "جرذ متسلق")]
        ClimbingRat,

        [Display(Name = "جرذ نرويجى")]
        NorwegianRat ,

        [Display(Name = "غير معروف")]
        Unknown 
    }
}
