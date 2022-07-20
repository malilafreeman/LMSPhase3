using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Student
    {
        public Student()
        {
            EnrollmentGrades = new HashSet<EnrollmentGrade>();
            Submissions = new HashSet<Submission>();
        }

        public string UId { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateOnly? Dob { get; set; }
        public string MajorDept { get; set; } = null!;

        public virtual Department MajorDeptNavigation { get; set; } = null!;
        public virtual ICollection<EnrollmentGrade> EnrollmentGrades { get; set; }
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
