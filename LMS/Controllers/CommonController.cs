using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class CommonController : Controller
    {
        //If your context class is named differently, fix this
        //and the constructor parameter
        private readonly LMSContext db;

        public CommonController(LMSContext _db)
        {
            db = _db;
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var depts =
                (from d in db.Departments
                select new { name = d.Name, subject = d.Abbreviation });

            return Json(depts);
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var catalog =
                from d in db.Departments
                select new
                {
                    subject = d.Abbreviation,
                    dname = d.Name,
                    courses =
                            (from c in db.Courses
                             where d.Abbreviation == c.Department
                             select new
                             {
                                 number = c.Number, cname = c.Name
                             }).ToArray()
                };

            return Json(catalog.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            var catalog_ID =
                (from c in db.Courses
                where c.Department == subject
                && c.Number == number
                select c.CatalogId);


            var offerings =
                (from c in db.Classes
                 where c.CatalogId == catalog_ID.First()
                 from p in db.Professors
                 where p.UId == c.ProfessorId
                 select new { season = c.Semester, year = c.Year, location = c.Location, start = c.StartTime.ToString(), end = c.EndTime.ToString(), fname = p.FirstName, lname = p.LastName});

            return Json(offerings);
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            var asgn =
              from co in db.Courses
              where co.Department == subject && co.Number == num
              join cl in db.Classes
              on co.CatalogId equals cl.CatalogId
              into classes_and_courses

              from cc in classes_and_courses
              where cc.Semester == season && cc.Year == year
              join ac in db.AssignmentCategories
              on cc.ClassId equals ac.ClassId
              into categories

              from cat in categories
              where cat.Name == category
              join a in db.Assignments
              on cat.CategoryId equals a.CategoryId
              into final

              from f in final
              where f.Name == asgname
              select f.Contents;

            return Content(asgn.First());
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {

            var asg_text =
                (from s in db.Submissions
                 where s.StudentId == uid && s.AssignmentName == asgname
                 select s.Contents);

            if (asg_text.Any())
            {
                return Content(asg_text.First());
            }
            else
            {
                return Content("");
            }
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            var student =
                (from s in db.Students
                 where s.UId == uid
                 select new
                 {fname = s.FirstName, lname = s.LastName, uid = s.UId, department = s.MajorDept });

            if (student.Any()) {
                return Json(student.First());
            }

            var prof =
                (from p in db.Professors
                 where p.UId == uid
                 select new
                 { fname = p.FirstName, lname = p.LastName, uid = p.UId, department = p.WorkDept });

            if (prof.Any())
            {
                return Json(prof.First());
            }


            var ad =
                (from a in db.Administrators
                 where a.UId == uid
                 select new
                 { fname = a.FirstName, lname = a.LastName, uid = a.UId });

            if (ad.Any())
            {
                return Json(ad.First());
            }

            return Json(new { success = false });
        }
    }
}

