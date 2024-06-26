﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {

        //If your context class is named something different,
        //fix this member var and the constructor param
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {

            var already_existent_dept =
                (from d in db.Departments
                 where d.Abbreviation == subject
                 select d.Abbreviation);

            if (already_existent_dept.Any())
            {
                return Json(new { success = false });
            }

            Department dept = new Department();
            dept.Name = name;
            dept.Abbreviation = subject;

            db.Departments.Add(dept);
            db.SaveChanges();

            return Json(new { success = true});
        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)

        {
            var courses =
                (from c in db.Courses
                 where c.Department == subject
                 select new { number = c.Number, name = c.Name });

            return Json(courses);
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)

        {
            var profs =
                (from p in db.Professors
                 where p.WorkDept == subject
                 select new { lname = p.LastName, fname = p.FirstName, uid = p.UId });

            return Json(profs); 
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {

            var already_existent_course =
                (from c in db.Courses
                 where c.Number == number
                 && c.Department == subject
                 select c.Name);

            if (already_existent_course.Any())
            {
                return Json(new { success = false });
            }


            var max_CourseID =
                (from c in db.Courses
                 orderby c.CatalogId descending
                 select c.CatalogId).Take(1);

            uint courseID = 0;

            if (max_CourseID.Any())
            {
                courseID = max_CourseID.FirstOrDefault();
            }

            int max_cID = (int)Math.Max(0, courseID);
            int new_Uid = max_cID += 1;
            uint cID = (uint)new_Uid;

            Course course = new Course();
            course.CatalogId = cID;
            course.Department = subject;
            course.Number = (uint)number;
            course.Name = name;

            db.Courses.Add(course);
            db.SaveChanges();


            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year, DateTime start, DateTime end, string location, string instructor)
        {

            var catalog_ID =
                (from c in db.Courses
                where c.Department == subject
                && c.Number == number
                select c.CatalogId);

            var course_check =
                (from c in db.Classes
                 where c.Semester == season && c.Year == year && c.CatalogId == catalog_ID.First()
                 select c);

            if (course_check.Any())
            {
                return Json(new { success = false });
            }

            TimeOnly time_from_start = TimeOnly.FromDateTime(start);
            TimeOnly time_from_end = TimeOnly.FromDateTime(end);


            var location_check =
               (from c in db.Classes
                where c.Location == location && c.Semester == season && c.Year == year &&
                (c.StartTime <= time_from_start && time_from_start < c.EndTime
                || (c.StartTime < time_from_end && time_from_end <= c.EndTime)
                || (c.StartTime >= time_from_start && c.EndTime <= time_from_end))
                //(c.StartTime <= start && start < c.EndTime
                //|| (c.StartTime < end && end <= c.EndTime)
                //|| (c.StartTime >= start && c.EndTime <= end))

                select c);

            if (location_check.Any())

            {
                return Json(new { success = false });
            }



            //if () {  return Json(new { success = false}); }else {}

            var max_ClassID =
                (from c in db.Classes
                 orderby c.ClassId descending
                 select c.ClassId).Take(1);

            uint classID = 0;

            if (max_ClassID.Any())
            {
                classID = max_ClassID.FirstOrDefault();
            }

            int max_cID = (int)Math.Max(0, classID);
            int new_ID = max_cID += 1;
            uint cID = (uint)new_ID;

            Class cl = new Class();
            cl.ClassId = cID;
            cl.Semester = season;
            cl.Year = year;
            cl.Location = location;
            cl.StartTime = TimeOnly.FromDateTime(start);
            cl.EndTime = TimeOnly.FromDateTime(end);
            //cl.StartTime = start;
            //cl.EndTime = end;
            cl.CatalogId = catalog_ID.FirstOrDefault();
            cl.ProfessorId = instructor;

            db.Classes.Add(cl);
            db.SaveChanges();


            return Json(new { success = true } );
        }
    }
}

