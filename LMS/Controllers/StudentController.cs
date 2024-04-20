using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
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


        public IActionResult ClassListings(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query =
                from e in db.Enrolleds
                join cl in db.Classes
                on e.ClassId equals cl.ClassId
                join co in db.Courses
                on cl.CourseId equals co.CourseId
                where e.UId == uid
                select new { subject = co.Department, number = co.Number, name = co.Name, season = cl.Season, year = cl.Year, grade = e.Grade };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            // get the course ID
            var courseID = db.Courses.FirstOrDefault(co =>
            co.Department == subject &&
            co.Number == num);

            // get the class ID
            var classID = db.Classes.FirstOrDefault(cl =>
            cl.Season == season &&
            cl.Year == year &&
            cl.CourseId == courseID.CourseId);

            var query =
                from c in db.Categories                
                where c.ClassId == classID.ClassId
                join a in db.Assignments on c.CatId equals a.CatId
                into left1
                from l in left1
                join s in db.Submissions
                on new { l.AssignmentId, UId = uid } equals new { s.AssignmentId, s.UId }
                into left2
                from s in left2.DefaultIfEmpty() // Left join here
                select new
                {
                    aname = l.Name,
                    cname = c.Name,
                    due = l.DueDate,
                    score = s.Score // Use null conditional operator to handle null scores
                };



            return Json(query.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var course = db.Courses.FirstOrDefault(co =>
            co.Department == subject &&
            co.Number == num);

            // get the class that matches the parameters
            var specificClass = db.Classes.FirstOrDefault(cl =>
            cl.CourseId == course.CourseId &&
            cl.Season == season &&
            cl.Year == year);

            // get the category
            var cat = db.Categories.FirstOrDefault(ca =>
            ca.Name == category &&
            ca.ClassId == specificClass.ClassId);

            var assignment = db.Assignments.FirstOrDefault(assign =>
            assign.CatId == cat.CatId &&
            assign.Name == asgname);

            //Check if Submission already exists
            var checkSubmission = db.Submissions.FirstOrDefault(sub =>
            sub.UId == uid &&
            sub.AssignmentId == assignment.AssignmentId);

            if (checkSubmission != null)
            {
                checkSubmission.Solution = contents;
                checkSubmission.Submitted = DateTime.Now;
            }
            else
            {
                //Create new Submission
                var submission = new Submission();
                submission.Solution = contents;
                submission.AssignmentId = assignment.AssignmentId;
                submission.Score = 0;
                submission.Submitted = DateTime.Now;
                submission.UId = uid;

                db.Submissions.Add(submission);
            }

            try
            {
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            // get the course ID
            var courseID = db.Courses.FirstOrDefault(co =>
            co.Department == subject &&
            co.Number == num);

            // get the class ID
            var classID = db.Classes.FirstOrDefault(cl =>
            cl.Season == season &&
            cl.Year == year &&
            cl.CourseId == courseID.CourseId);

            // check to see if student is already enrolled in class
            var studentEnrolled = db.Enrolleds.FirstOrDefault(e =>
            e.UId == uid &&
            e.ClassId == classID.ClassId);

            try
            {
                // if they are not, enroll them
                if(studentEnrolled == null) 
                {
                    Enrolled enroll = new Enrolled();
                    enroll.UId = uid;
                    enroll.ClassId = classID.ClassId;
                    enroll.Grade = "--";

                    db.Enrolleds.Add(enroll);

                    db.SaveChanges();
                }
                else
                    return Json(new { success = false });
            }
            catch (Exception ex) { Console.WriteLine(ex); }

            return Json(new { success = true });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            double student_gpa = 0;

            var query =
                from e in db.Enrolleds
                where e.UId == uid
                select e.Grade;
            int classes = query.Count();
            foreach (string e in query)
            {
                switch(e)
                {
                    case "A":  student_gpa = 4.0; break;
                    case "A-": student_gpa = 3.7; break;
                    case "B+": student_gpa = 3.3; break;
                    case "B":  student_gpa = 3.0; break;
                    case "B-": student_gpa = 2.7; break;
                    case "C+": student_gpa = 2.3; break;
                    case "C":  student_gpa = 2.0; break;
                    case "C-": student_gpa = 1.7; break;
                    case "D+": student_gpa = 1.3; break;
                    case "D":  student_gpa = 1.0; break;
                    case "D-": student_gpa = 0.7; break;
                    case "E": break;
                    default:
                        classes--; break; //Dont Count towards gpa
                }
            }

            if (classes > 0)
                student_gpa /= classes;

            return Json(new { gpa = student_gpa });

        }

        /*******End code to modify********/

    }
}

