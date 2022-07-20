using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public string StudentId { get; set; } = null!;
        public string AssignmentName { get; set; } = null!;
        public DateTime? Time { get; set; }
        public float? Score { get; set; }
        public string? Contents { get; set; }

        public virtual Assignment AssignmentNameNavigation { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
