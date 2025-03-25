using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class AssignmentCategory
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
}
