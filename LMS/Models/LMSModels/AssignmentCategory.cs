namespace LMS.Models.LMSModels;

public class AssignmentCategory
{
    public AssignmentCategory()
    {
        Assignments = new HashSet<Assignment>();
    }

    public string Name { get; set; } = null!;
    public int ClassId { get; set; }
    public uint GradeWeight { get; set; }
    public int AssignmentCategoriesId { get; set; }

    public virtual Class Class { get; set; } = null!;
    public virtual ICollection<Assignment> Assignments { get; set; }
}