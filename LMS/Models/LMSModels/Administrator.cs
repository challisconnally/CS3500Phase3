namespace LMS.Models.LMSModels;

public class Administrator
{
    public string UId { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateOnly Dob { get; set; }
}