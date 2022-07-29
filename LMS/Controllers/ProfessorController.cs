using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        //If your context is named something else, fix this
        //and the constructor param
        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var myStudents =
                from co in db.Courses
                where co.Department == subject && co.Number == num
                join cl in db.Classes
                on co.CatalogId equals cl.CatalogId
                into classes_and_courses

                from cc in classes_and_courses
                where cc.Semester == season && cc.Year == year
                join e in db.EnrollmentGrades
                on cc.ClassId equals e.ClassId
                into cc_and_enrollments

                from ce in cc_and_enrollments
                join s in db.Students
                on ce.StudentId equals s.UId
                into all_components
                from ac in all_components

                select new
                {
                    fname = ac.FirstName,
                    lname = ac.LastName,
                    uid = ac.UId,
                    dob = ac.Dob,
                    grade = ce.Grade
                };

            return Json(myStudents);
        }


        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {

            if(category == null) {

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
                    join a in db.Assignments
                    on cat.CategoryId equals a.CategoryId
                    into final

                    from f in final
                    select new
                    {
                        aname = f.Name,
                        cname = cat.Name,
                        due = f.Due,
                        submissions = (from s in db.Submissions
                                       where s.AssignmentName == f.Name
                                       select s).Count()
                        //submissions = 0
                    };

                return Json(asgn);
            }

            else

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
                    select new
                        {
                            aname = f.Name,
                            cname = cat.Name,
                            due = f.Due,
                            submissions = (from s in db.Submissions
                                           where s.AssignmentName == f.Name
                                           select s).Count()
                    };

                return Json(asgn);
            }
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {

            // Get catalog id
            // Get classID
            // get ascat where classId is classID

            var asscats =
                from co in db.Courses
                where co.Department == subject && co.Number == num
                join cl in db.Classes
                on co.CatalogId equals cl.CatalogId
                into classes_and_courses

                from cc in classes_and_courses
                where cc.Semester == season && cc.Year == year
                join ac in db.AssignmentCategories
                on cc.ClassId equals ac.ClassId
                into everything

                from e in everything
                select new

                {
                    name = e.Name,
                    weight = e.Weight
                };

            return Json(asscats);
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {

            var catalog_ID =
                (from c in db.Courses
                 where c.Department == subject && c.Number == num
                 select c.CatalogId).First();

            var class_ID =
                (from c in db.Classes
                 where (c.CatalogId == catalog_ID && c.Semester == season && c.Year == year)
                 select c.ClassId).First();

            var exist_check =
                (from ac in db.AssignmentCategories
                 where (ac.ClassId == class_ID && ac.Name == category)
                 select ac);

            if (exist_check.Any())
            {
                return Json(new { success = false });
            }

            var max_CatID =
                (from ac in db.AssignmentCategories
                orderby ac.CategoryId descending
                select ac.CategoryId).Take(1);

            uint tmp_catID = 0;

            if (max_CatID.Any())
            {
                tmp_catID = max_CatID.FirstOrDefault();
            }

            int max_catID = (int)Math.Max(0, tmp_catID);
            int new_ID = max_catID += 1;
            uint catID = (uint)new_ID;

            AssignmentCategory asscat = new AssignmentCategory();
            asscat.Name = category;
            asscat.Weight = catweight;
            asscat.CategoryId = catID;
            asscat.ClassId = class_ID;


            db.AssignmentCategories.Add(asscat);
            db.SaveChanges();


            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var sameName =
                (from a in db.Assignments
                 where a.Name == asgname
                 select a);

            if (sameName.Any()) {
                return Json(new { success = false });
            }

            var catalog_ID =
                (from c in db.Courses
                where c.Department == subject && c.Number == num
                select c.CatalogId).First();

            var class_ID =
                (from c in db.Classes
                 where (c.CatalogId == catalog_ID && c.Semester == season && c.Year == year)
                 select c.ClassId).First();

            var category_ID =
                (from ac in db.AssignmentCategories
                 where (ac.ClassId == class_ID && ac.Name == category)
                 select ac.CategoryId).First();


            Assignment asg = new Assignment();
            asg.Name = asgname;
            asg.MaxPoints = (uint)asgpoints;
            asg.CategoryId = category_ID;
            asg.Due = asgdue;
            asg.Contents = asgcontents;

            db.Assignments.Add(asg);
            db.SaveChanges();


            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {

            var submissions =
                from su in db.Submissions
                where su.AssignmentName == asgname
                join st in db.Students
                on su.StudentId equals st.UId
                into sub_and_students

                from ss in sub_and_students
                select new
                {
                    fname = ss.FirstName,
                    lname = ss.LastName,
                    uid = ss.UId,
                    time = su.Time,
                    score = su.Score
                };
     
            return Json(submissions);
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {

            var submission =
                (from s in db.Submissions
                 where s.AssignmentName == asgname && s.StudentId == uid
                 select s);

            var max_score =
                (from a in db.Assignments
                 where a.Name == asgname
                 select a.MaxPoints).First();


            if (submission.Any())
            {
                var get_sub = submission.First();

                // Can't submit a score that's higher than the max score
                if (score > max_score)
                {
                    return Json(new { success = false });
                }
                else
                {

                    get_sub.Score = score;

                    db.SaveChanges();

                    // Now update the grade
                    string updated_grade = AutoGrade1Student(subject, num, season, year, uid);

                    // Need the class_id so we know which grade to update

                    var catalog_ID =
                        (from c in db.Courses
                         where c.Department == subject && c.Number == num
                         select c.CatalogId).First();

                    var class_ID =
                        (from c in db.Classes
                         where (c.CatalogId == catalog_ID && c.Semester == season && c.Year == year)
                         select c.ClassId).First();

                    var query_to_change_grade =
                        (from e in db.EnrollmentGrades
                         where e.StudentId == uid && e.ClassId == class_ID
                         select e);

                    if (query_to_change_grade.Any())
                    {
                        var enrollment = query_to_change_grade.First();
                        enrollment.Grade = updated_grade;
                        db.SaveChanges();
                    }

                    return Json(new { success = true });
                }
            }



            return Json(new { success = false });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {

            var myClasses =
                from p in db.Professors
                where p.UId == uid
                join cl in db.Classes
                on p.UId equals cl.ProfessorId
                into classes_profs

                from cp in classes_profs
                join co in db.Courses
                on cp.CatalogId equals co.CatalogId
                into everything

                from e in everything
                select new
                {
                    subject = e.Department,
                    number = e.Number,
                    name = e.Name,
                    season = cp.Semester,
                    year = cp.Year
                };


            return Json(myClasses);
        }

        private string AutoGrade1Student(string subject, int num, string season, int year, string uid)
        {
            // for each category in the class we need:
            // The category weight
            // For each assignment in the category we need:
            // The maximum points
            // The student's current score on the assignment

            var category_info =
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

                select new
                {
                    weight = cat.Weight,
                    asgn_scores = (from a in db.Assignments
                                   where a.CategoryId == cat.CategoryId
                                   select new
                                   {
                                       max_points = a.MaxPoints,
                                       student_points = (from s in db.Submissions
                                                         where s.AssignmentName == a.Name && s.StudentId == uid
                                                         select s.Score).FirstOrDefault()
                                   }
                                    ).ToArray()
                };

            Console.WriteLine("WEIGHT: " + category_info.First().weight.ToString());
            Console.WriteLine("MAX POINTS: " + category_info.First().asgn_scores.First().max_points.ToString());
            Console.WriteLine("STUDENT POINTS: " + category_info.First().asgn_scores.First().student_points.ToString());


            float total_scaled_cats = 0;
            float total_cat_weights = 0;

            foreach (var cat in category_info)
            {
                int category_weight = cat.weight;
                Console.WriteLine("my category weight" + category_weight.ToString());
                total_cat_weights += category_weight;

                float total_points_earned = 0;
                float total_max_points = 0;

                foreach (var asgn in cat.asgn_scores)
                {
                    total_max_points += asgn.max_points;

                    float student_points_to_add = 0;

                    if (asgn.student_points != null) {
                        student_points_to_add = (float)asgn.student_points;
                    }

                    total_points_earned += student_points_to_add;
                }

                Console.WriteLine("my total points earned" + total_points_earned.ToString());
                Console.WriteLine("my total max points" + total_max_points.ToString());


                // Calculate the percentage of (total points earned / total max points) of all assignments in the category
                float percent_all_cat_points = total_points_earned / total_max_points;
                Console.WriteLine("my percent all cat points" + percent_all_cat_points.ToString());


                // Multiply the percentage by the category weight. 
                float first_weighted = percent_all_cat_points * category_weight;
                Console.WriteLine("my first weighted" + first_weighted.ToString());


                // Compute the total of all scaled category totals from the previous step. 
                total_scaled_cats += first_weighted;
                Console.WriteLine("my total scaled cats" + total_scaled_cats.ToString());

            }

            Console.WriteLine("my total cat weights: " + total_cat_weights.ToString());

            // Re-scale
            float scaling_factor = 100 / total_cat_weights;
            Console.WriteLine("my scaling factor" + scaling_factor.ToString());


            // Total percentage
            float total_percentage = total_scaled_cats * scaling_factor;
            Console.WriteLine("my total percentage: " + total_percentage.ToString());

            string grade = getLetterGrade(total_percentage);

            return grade;
        }

        private string getLetterGrade(float total_percentage)
        {

            string grade = "";

            if (total_percentage >= 92)
            {
                grade = "A";
            }
            else if (total_percentage >= 90)
            {
                grade = "A-";
            }
            else if (total_percentage >= 87)
            {
                grade = "B+";
            }
            else if (total_percentage >= 82)
            {
                grade = "B";
            }
            else if (total_percentage >= 80)
            {
                grade = "B-";
            }
            else if (total_percentage >= 77)
            {
                grade = "C+";
            }
            else if (total_percentage >= 72)
            {
                grade = "C";
            }
            else if (total_percentage >= 70)
            {
                grade = "C-";
            }
            else if (total_percentage >= 67)
            {
                grade = "D+";
            }
            else if (total_percentage >= 62)
            {
                grade = "D";
            }
            else if (total_percentage >= 60)
            {
                grade = "D-";
            }
            else
            {
                grade = "E";
            }

            return grade;
        }

        //private int AutoGrade1(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        //{

        //var asgn_cats =
        //    from co in db.Courses
        //    where co.Department == subject && co.Number == num
        //    join cl in db.Classes
        //    on co.CatalogId equals cl.CatalogId
        //    into classes_and_courses

        //from cc in classes_and_courses
        //where cc.Semester == season && cc.Year == year
        //join ac in db.AssignmentCategories
        //on cc.ClassId equals ac.ClassId
        //into categories

        //        from cat in categories

        //        select new
        //        {
        //            weight = cat.Weight,
        //            name = cat.Name
        //        };

        //    // Need to sum all of the weights

        //    int totalWeights = 0;

        //    foreach (var cat in asgn_cats){

        //        totalWeights += cat.weight;

        //    }

        //    IDictionary<string, float> numberNames = new Dictionary<string, float>();

        //    foreach (var cat in asgn_cats)
        //    {
        //        numberNames.Add(cat.name, cat.weight / totalWeights);
        //    }


        //    IDictionary<string, float> catScores = new Dictionary<string, float>();

        //    foreach (var cat in asgn_cats)
        //    {
        //        var class_asgns = getAssignmentsInCategoryForGrades(subject, num, season, year, cat.name, asgname, uid, score);

        //        int sumScore = 0;
        //        int sumMaxScore = 0;

        //        foreach (var asg in class_asgns)
        //        {
        //            sumScore += asg.score;
        //        }
        //    }






        //    //int studentScoreSum = 0;

        //    //int maxScoreSum = 0;

        //    //List<IQueryable> allAssAllCats = new List<IQueryable>();

        //    //foreach(var cat in asgn_cats)
        //    //{
        //    //    var class_asgns = getAssignmentsInCategoryForGrades(subject, num, season, year, category, asgname, uid, score);
        //    //    allAssAllCats.Append(class_asgns);

        //    //}


        //    //foreach (var asscat in allAssAllCats)
        //    //{
        //    //    int studentScoreSum = 0;
        //    //    int maxScoreSum = 0;

        //    //    foreach (var q in asscat)
        //    //    {
        //    //        studentScoreSum += q.score;
        //    //    }

        //    //}

        //    // Get a list of all assignments for the class 







        //    //    from co in db.Courses
        //    //    where co.Department == subject && co.Number == num
        //    //    join cl in db.Classes
        //    //    on co.CatalogId equals cl.CatalogId
        //    //    into classes_and_courses

        //    //    from cc in classes_and_courses
        //    //    where cc.Semester == season && cc.Year == year
        //    //    join e in db.EnrollmentGrades
        //    //    on cc.ClassId equals e.ClassId
        //    //    into cc_and_enrollments

        //    //    from ce in cc_and_enrollments
        //    //    join s in db.Students
        //    //    on ce.StudentId equals s.UId
        //    //    into all_components

        //    //    from ac in all_components

        //    //    //from ac in all_components



        //    //    //select new
        //    //    //{
        //    //    //    fname = ac.FirstName,
        //    //    //    lname = ac.LastName,
        //    //    //    uid = ac.UId,
        //    //    //    dob = ac.Dob,
        //    //    //    grade = ce.Grade
        //    //    //};

        //    return 0;
        //}


        //private IQueryable getAssignmentsInCategoryForGrades(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        //{
        //    //var class_asgns =
        //    //     from co in db.Courses
        //    //     where co.Department == subject && co.Number == num
        //    //     join cl in db.Classes
        //    //     on co.CatalogId equals cl.CatalogId
        //    //     into classes_and_courses

        //    //     from cc in classes_and_courses
        //    //     where cc.Semester == season && cc.Year == year
        //    //     join ac in db.AssignmentCategories
        //    //     on cc.ClassId equals ac.ClassId
        //    //     into categories

        //    //     from cat in categories
        //    //     join a in db.Assignments
        //    //     on cat.CategoryId equals a.CategoryId
        //    //     into class_assignments

        //    //     from ca in class_assignments
        //    //     join s in db.Submissions
        //    //     on new { A = ca.Name, B = uid } equals new { A = s.AssignmentName, B = s.StudentId }
        //    //     into left_joined

        //    //     from j in left_joined.DefaultIfEmpty()

        //    //     select new
        //    //     {
        //    //         asgname = j.AssignmentName,
        //    //         uID = j.StudentId,
        //    //         score = j.Score,
        //    //         maxScore = ca.MaxPoints,
        //    //         sum = (from stuff in )
        //    //         //sum = (select j.Score).Sum()            
        //    //        };

        //    //return (class_asgns);

        //    var class_asgns =
        //         from co in db.Courses
        //         where co.Department == subject && co.Number == num
        //         join cl in db.Classes
        //         on co.CatalogId equals cl.CatalogId
        //         into classes_and_courses

        //         from cc in classes_and_courses
        //         where cc.Semester == season && cc.Year == year
        //         join ac in db.AssignmentCategories
        //         on cc.ClassId equals ac.ClassId
        //         into categories

        //         from cat in categories
        //         join a in db.Assignments
        //         on cat.CategoryId equals a.CategoryId
        //         into class_assignments

        //         from ca in class_assignments
        //         join s in db.Submissions
        //         on new { A = ca.Name, B = uid } equals new { A = s.AssignmentName, B = s.StudentId }
        //         into left_joined

        //         from j in left_joined.DefaultIfEmpty()
        //         group j by ca.CategoryId
        //         into grouped

        //         from g in grouped

        //         select new
        //         {
        //             //asgname = j.AssignmentName,
        //             //uID = j.StudentId,
        //             score = g.Sum(y => .score), //(j.Score ?? 0),
        //             maxScore = gc.MaxPoints,
        //             sum = (from stuff in )
        //                         //sum = (select j.Score).Sum()            
        //         };

        //    return (class_asgns);
        //}
    }
}

