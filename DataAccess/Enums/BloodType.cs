namespace DataAccess.Enums;

using System.ComponentModel;

public enum BloodType
{
    
    [Description("0 (I) +")]
    O_Positive,
    
    [Description("0 (I) -")]
    O_Negative,
    
   
    [Description("A (II) +")]
    A_Positive,
    
    [Description("A (II) -")]
    A_Negative,
    
    [Description("B (III) +")]
    B_Positive,
    
    [Description("B (III) -")]
    B_Negative,
    
    [Description("AB (IV) +")]
    AB_Positive,
    
    [Description("AB (IV) -")]
    AB_Negative
}
