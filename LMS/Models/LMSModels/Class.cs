using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCategories = new HashSet<AssignmentCategory>();
            EnrollmentGrades = new HashSet<EnrollmentGrade>();
        }

        public uint ClassId { get; set; }
        public string? Semester { get; set; }
        public string Location { get; set; } = null!;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public uint CatalogId { get; set; }
        public string ProfessorId { get; set; } = null!;
        public int? Year { get; set; }

        public virtual Course Catalog { get; set; } = null!;
        public virtual Professor Professor { get; set; } = null!;
        public virtual ICollection<AssignmentCategory> AssignmentCategories { get; set; }
        public virtual ICollection<EnrollmentGrade> EnrollmentGrades { get; set; }
    }
}
