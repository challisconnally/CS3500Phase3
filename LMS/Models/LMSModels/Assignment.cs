using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignment
    {
        public Assignment()
        {
            Submissions = new HashSet<Submission>();
        }

        public string Name { get; set; } = null!;
        public int AssignmentCategoriesId { get; set; }
        public string Contents { get; set; } = null!;
        public DateTime DueDate { get; set; }
        public uint MaxPoints { get; set; }
        public int AssignmentId { get; set; }

        public virtual AssignmentCategory AssignmentCategories { get; set; } = null!;
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
