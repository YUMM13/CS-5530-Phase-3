﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

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
            // get the course that matches the parameters
            var course = db.Courses.FirstOrDefault(co =>
            co.Department == subject &&
            co.Number == num);

            // get the class that matches the parameters
            var specificClass = db.Classes.FirstOrDefault(cl =>
            cl.CourseId == course.CourseId &&
            cl.Season == season &&
            cl.Year == year);

            // get the list of students from the class
            var query = 
                from s in db.Students
                join e in db.Enrolleds
                on s.UId equals e.UId
                where e.ClassId == specificClass.ClassId
                select new { fname = s.FirstName, lname = s.LastName, uid = s.UId, dob = s.Dob, grade = e.Grade};

            return Json(query.ToArray());
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
            // if category is null, then print all assignments
            if(category == null)
            {
                // get the course that matches the parameters
                var course = db.Courses.FirstOrDefault(co =>
                co.Department == subject &&
                co.Number == num);

                // get the class that matches the parameters
                var specificClass = db.Classes.FirstOrDefault(cl =>
                cl.CourseId == course.CourseId &&
                cl.Season == season &&
                cl.Year == year);

                var query =
                    from cat in db.Categories
                    join a in db.Assignments
                    on cat.CatId equals a.CatId
                    where cat.ClassId == specificClass.ClassId
                    select new
                    {
                        aname = a.Name,
                        cname = cat.Name,
                        due = a.DueDate,
                        submissions = a.Submissions.Count()
                    };

                return Json(query.ToArray());
            }
            // else print out all assignments in the given category
            else
            {
                // get the course that matches the parameters
                var course = db.Courses.FirstOrDefault(co =>
                co.Department == subject &&
                co.Number == num);

                // get the class that matches the parameters
                var specificClass = db.Classes.FirstOrDefault(cl =>
                cl.CourseId == course.CourseId &&
                cl.Season == season &&
                cl.Year == year);

                var query =
                    from cat in db.Categories
                    join a in db.Assignments
                    on cat.CatId equals a.CatId
                    where cat.Name == category && cat.ClassId == specificClass.ClassId
                    select new
                    {
                        aname = a.Name,
                        cname = cat.Name,
                        due = a.DueDate,
                        submissions = a.Submissions.Count()
                    };

                return Json(query.ToArray());
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
            // get the course that matches the parameters
            var course = db.Courses.FirstOrDefault(co =>
            co.Department == subject &&
            co.Number == num);

            // get the class that matches the parameters
            var specificClass = db.Classes.FirstOrDefault(cl =>
            cl.CourseId == course.CourseId &&
            cl.Season == season &&
            cl.Year == year);

            var query =
                from cat in db.Categories
                where cat.ClassId == specificClass.ClassId
                select new
                {
                    name = cat.Name,
                    weight = cat.Weight
                };

            return Json(query.ToArray());
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
            // get the course that matches the parameters
            var course = db.Courses.FirstOrDefault(co =>
            co.Department == subject &&
            co.Number == num);

            // get the class that matches the parameters
            var specificClass = db.Classes.FirstOrDefault(cl =>
            cl.CourseId == course.CourseId &&
            cl.Season == season &&
            cl.Year == year);

            // check to see if the category already exists
            var categoryCheck = db.Categories.FirstOrDefault(ca =>
            ca.Name == category &&
            ca.ClassId == specificClass.ClassId);

            try
            {
                // if the category does not exist, create it
                if(categoryCheck == null)
                {
                    Category cat = new Category();
                    cat.Name = category;
                    cat.Weight = (uint)catweight; 
                    cat.ClassId = specificClass.ClassId;

                    db.Categories.Add(cat);

                    db.SaveChanges();

                    return Json(new { success = true });
                }
                // else return false
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception ex) { Console.WriteLine(ex); }

            return Json(new { success = false });
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
            // get the course that matches the parameters
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
            ca.Name.Equals(System.Uri.UnescapeDataString(category)) &&
            ca.ClassId == specificClass.ClassId);

            try
            {
                Assignment assignment = new Assignment();
                assignment.Name = asgname;
                assignment.MaxPoints = (uint)asgpoints;
                assignment.DueDate = asgdue;
                assignment.Contents = asgcontents;
                assignment.CatId = cat.CatId;

                db.Assignments.Add(assignment);

                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex) { Console.WriteLine(ex); }

            return Json(new { success = false });
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
            // Find the one assignment
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

            //Need to get the name from the uid


            //Join that assignment with submissions
            //Join submissions with Student to get name
            var query =
                from sub in db.Submissions
                join student in db.Students on sub.UId equals student.UId
                where sub.AssignmentId == assignment.AssignmentId

                select new
                {
                    fname = student.FirstName,
                    lname = student.LastName,
                    uid = student.UId,
                    time = sub.Submitted,
                    score = sub.Score
                };
            
            return Json(query.ToArray());
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
            // Find the one assignment
            uint uscore = (uint)score;
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

            var submission = db.Submissions.FirstOrDefault(sub =>
            sub.AssignmentId == assignment.AssignmentId &&
            sub.UId == uid);

            var enrolled = db.Enrolleds.FirstOrDefault(en =>
            en.ClassId == specificClass.ClassId &&
            en.UId == uid);

            try
            {
                //Update sumbmission score
                submission.Score = uscore;

                //Update Class grade
                string letter = LetterGrade(specificClass.ClassId, uid);
                enrolled.Grade = letter;

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="classID"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public String LetterGrade(uint classID, string uid)
        {
            double grade = 0;
            double sumOfCatWeights = 0;
            //Get all the categories
            var query =
                from c in db.Categories
                where c.ClassId == classID
                select c;
            //Loop over categories
            var categories = query.ToList();
            
            foreach (var cat in categories)
            {
                double catPoints = 0;
                double studentScore = 0;

                //Max point of the class
                var maxPoints =
                    from a in db.Assignments
                        join sub in db.Submissions on a.AssignmentId equals sub.AssignmentId
                        where a.CatId == cat.CatId && sub.UId == uid
                        select new { max = a.MaxPoints, score = sub.Score };
                if (maxPoints.Count() == 0)
                    continue;
                foreach (var points in maxPoints)
                {
                    catPoints += points.max;
                    studentScore += points.score ?? 0;
                }
                
                grade += (studentScore/catPoints) * cat.Weight;
                sumOfCatWeights += cat.Weight;
            }
            //Scale
            grade *= (100 / sumOfCatWeights);

            if (grade >= 93)
                return "A";
            if (grade >= 90)
                return "A-";
            if (grade >= 87)
                return "B+"; 
            if (grade >= 83)
                return "B";
            if (grade >= 80)
                return "B-";
            if (grade >= 77)
                return "C+";
            if (grade >= 73)
                return "C";
            if (grade >= 70)
                return "C-";
            if (grade >= 67)
                return "D+";
            if (grade >= 63)
                return "D";
            if (grade >= 60)
                return "D-";
            return "E";
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
            var query = 
                from c in db.Classes
                where c.TaughtBy == uid
                select new { subject = c.Course.Department, number = c.Course.Number, name = c.Course.Name, season = c.Season, year = c.Year };

            return Json(query.ToArray());
        }
        /*******End code to modify********/
    }
}

