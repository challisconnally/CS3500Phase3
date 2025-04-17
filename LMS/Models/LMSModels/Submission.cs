using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public string UId { get; set; } = null!;
        public string Contents { get; set; } = null!;
        public DateTime SubmissionTime { get; set; }
        public uint Score { get; set; }
        public int AssignmentId { get; set; }

        public virtual Assignment Assignment { get; set; } = null!;
        public virtual Student UIdNavigation { get; set; } = null!;
    }
}
