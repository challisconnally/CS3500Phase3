namespace LMS.Models.LMSModels;

public class Enrolled
{
    public string UId { get; set; } = null!;
    public string Grade { get; set; } = null!;
    public int ClassId { get; set; }

    public virtual Course Class { get; set; } = null!;
    public virtual Student UIdNavigation { get; set; } = null!;
}