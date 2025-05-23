﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class CommonController : Controller
    {
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
            var query = 
                from d in db.Departments
                select new {name =  d.Name, subject = d.Subject};

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS") = Courses.Department
        /// "dname": The department name, as in "Computer Science" = 
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)  = Courses.Number
        ///            "cname": The course name (e.g. "Database Systems") = Courses.Name
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var query =
                from d in db.Departments
                select new
                { subject = d.Subject, dname = d.Name, courses = from c in d.Courses
                              select new { number = c.Number, cname = c.Name } };

            return Json(query.ToArray());
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
            // get the course ID
            var course = db.Courses.FirstOrDefault(co =>
            co.Department == subject &&
            co.Number == number);

            // get all classes with the above course ID
            var query =
                from c in db.Classes
                join p in db.Professors
                on c.TaughtBy equals p.UId
                where c.CourseId == course.CourseId
                select new {season = c.Season, year =  c.Year, location = c.Location, start = c.Start, end = c.End, fname = p.FirstName, lname = p.LastName};

            return Json(query.ToArray());
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
            // get the course ID
            var courseID = db.Courses.FirstOrDefault(co =>
            co.Department == subject &&
            co.Number == num);

            // get the class ID
            var classID = db.Classes.FirstOrDefault(cl =>
            cl.Season == season &&
            cl.Year == year &&
            cl.CourseId == courseID.CourseId);

            // get the category ID
            var cat = db.Categories.FirstOrDefault(ca =>
            ca.Name == category &&
            ca.ClassId == classID.ClassId);

            // get the assignment based off of the category ID
            var query = 
                from a in db.Assignments
                where a.Name == asgname && a.CatId == cat.CatId
                select a.Contents;

            return Content(query.ToJson());
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
            // get the course ID
            var courseID = db.Courses.FirstOrDefault(co =>
            co.Department == subject &&
            co.Number == num);

            // get the class ID
            var classID = db.Classes.FirstOrDefault(cl =>
            cl.Season == season &&
            cl.Year == year &&
            cl.CourseId == courseID.CourseId);

            // get the category ID
            var cat = db.Categories.FirstOrDefault(ca =>
            ca.Name == category &&
            ca.ClassId == classID.ClassId);

            // get the assignment ID
            var assignment = db.Assignments.FirstOrDefault(ass =>
            ass.Name == asgname &&
            ass.CatId == cat.CatId);

            // get the assignment based off of the category ID
            var query =
                from s in db.Submissions
                where s.UId == uid && s.AssignmentId == assignment.AssignmentId
                select s.Solution;

            return Content(query.ToJson());
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
            var studentQuery = 
                from s in db.Students
                where s.UId == uid
                select new { fname = s.FirstName, lname = s.LastName, uid = s.UId, department = s.Major };

            if (studentQuery.Count() > 0) { return Json(studentQuery.First()); }

            var professorQuery = 
                from p in db.Professors
                where p.UId == uid
                select new { fname = p.FirstName, lname = p.LastName, uid = p.UId, department = p.Department };

            if (professorQuery.Count() > 0) { return Json(professorQuery.First()); }

            var adminQuery = 
                from a in db.Administrators
                where a.UId == uid
                select new { fname = a.FirstName, lname = a.LastName, uid = a.UId };

            if (adminQuery.Count() > 0) { return Json(adminQuery.First()); }


            return Json(new { success = false });
        }


        /*******End code to modify********/
    }
}

