namespace LMS.Models.LMSModels;

public class Course
{
    public Course()
    {
        Classes = new HashSet<Class>();
        Enrolleds = new HashSet<Enrolled>();
    }

    public int CourseNum { get; set; }
    public string Name { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public int CourseId { get; set; }

    public virtual Department SubjectNavigation { get; set; } = null!;
    public virtual ICollection<Class> Classes { get; set; }
    public virtual ICollection<Enrolled> Enrolleds { get; set; }
}