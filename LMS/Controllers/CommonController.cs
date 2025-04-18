using System.Runtime.CompilerServices;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]

namespace LMS.Controllers;

public class CommonController : Controller
{
    private readonly LMSContext db;

    public CommonController(LMSContext _db)
    {
        db = _db;
    }

    /*******Begin code to modify********/

    /// <summary>
    ///     Retreive a JSON array of all departments from the database.
    ///     Each object in the array should have a field called "name" and "subject",
    ///     where "name" is the department name and "subject" is the subject abbreviation.
    /// </summary>
    /// <returns>The JSON array</returns>
    public IActionResult GetDepartments()
    {
        var query = from d in db.Departments
                    select new
                    {
                        name = d.Name,
                        subject = d.Subject
                    };

        return Json(query.ToArray());
    }


    /// <summary>
    ///     Returns a JSON array representing the course catalog.
    ///     Each object in the array should have the following fields:
    ///     "subject": The subject abbreviation, (e.g. "CS")
    ///     "dname": The department name, as in "Computer Science"
    ///     "courses": An array of JSON objects representing the courses in the department.
    ///     Each field in this inner-array should have the following fields:
    ///     "number": The course number (e.g. 5530)
    ///     "cname": The course name (e.g. "Database Systems")
    /// </summary>
    /// <returns>The JSON array</returns>
    public IActionResult GetCatalog()
    {
        var query =
            from d in db.Departments
            select new
            {
                subject = d.Subject,
                dname = d.Name,
                courses = (from c in db.Courses
                           where c.Subject == d.Subject
                           select new
                           {
                               number = c.CourseNum,
                               cname = c.Name
                           }).ToList()
            };

        return Json(query.ToArray());
    }

    /// <summary>
    ///     Returns a JSON array of all class offerings of a specific course.
    ///     Each object in the array should have the following fields:
    ///     "season": the season part of the semester, such as "Fall"
    ///     "year": the year part of the semester
    ///     "location": the location of the class
    ///     "start": the start time in format "hh:mm:ss"
    ///     "end": the end time in format "hh:mm:ss"
    ///     "fname": the first name of the professor
    ///     "lname": the last name of the professor
    /// </summary>
    /// <param name="subject">The subject abbreviation, as in "CS"</param>
    /// <param name="number">The course number, as in 5530</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetClassOfferings(string subject, int number)
    {
        var query =

            from c in db.Courses
            where c.Subject == subject && c.CourseNum == number
            join stuff in db.Classes on c.CourseId equals stuff.CourseId
            join prof in db.Professors on stuff.UId equals prof.UId
            select new
            {
                season = stuff.Season,
                year = stuff.Year,
                location = stuff.Location,
                start = stuff.Start.ToString("hh:mm:ss"),
                end = stuff.End.ToString("hh:mm:ss"),
                fname = prof.FirstName,
                lname = prof.LastName
            };

        return Json(query.ToArray());
    }

    /// <summary>
    ///     This method does NOT return JSON. It returns plain text (containing html).
    ///     Use "return Content(...)" to return plain text.
    ///     Returns the contents of an assignment.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment in the category</param>
    /// <returns>The assignment contents</returns>
    public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category,
        string asgname)
    {
        var query =
        from c in db.Courses
        where c.Subject == subject && c.CourseNum == num
        join stuff in db.Classes on c.CourseId equals stuff.CourseId
        where stuff.Season.Equals(season) && stuff.Year == year
        join ac in db.AssignmentCategories on stuff.ClassId equals ac.ClassId
        where ac.Name.Equals(category)
        join a in db.Assignments on ac.AssignmentCategoriesId equals a.AssignmentCategoriesId
        where a.Name.Equals(asgname)
        select a;

        if (query.Any())
        {
            return Content(query.First().Contents);
        }
        else
            return Content("");
    }


    /// <summary>
    ///     This method does NOT return JSON. It returns plain text (containing html).
    ///     Use "return Content(...)" to return plain text.
    ///     Returns the contents of an assignment submission.
    ///     Returns the empty string ("") if there is no submission.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment in the category</param>
    /// <param name="uid">The uid of the student who submitted it</param>
    /// <returns>The submission text</returns>
    public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category,
        string asgname, string uid)
    {
        var query =
        from c in db.Courses
        where c.Subject == subject && c.CourseNum == num
        join stuff in db.Classes on c.CourseId equals stuff.CourseId
        where stuff.Season.Equals(season) && stuff.Year == year
        select stuff.ClassId;

        int cID = query.FirstOrDefault();

        var query2 =
            from cat in db.AssignmentCategories
            join ac in db.AssignmentCategories on cat.ClassId equals ac.ClassId
            where ac.Name.Equals(category)
            join a in db.Assignments on ac.AssignmentCategoriesId equals a.AssignmentCategoriesId
            where a.Name.Equals(asgname)
            join sub in db.Submissions on a.AssignmentId equals sub.AssignmentId into s
            from subs in s.DefaultIfEmpty()
            where subs.UId.Equals(uid)
            where ac.ClassId == cID
            select subs.Contents;

        if (query2.Any())
        {
            return Content(query2.First());
        }
        
            return Content("");
    }


    /// <summary>
    ///     Gets information about a user as a single JSON object.
    ///     The object should have the following fields:
    ///     "fname": the user's first name
    ///     "lname": the user's last name
    ///     "uid": the user's uid
    ///     "department": (professors and students only) the name (such as "Computer Science") of the department for the user.
    ///     If the user is a Professor, this is the department they work in.
    ///     If the user is a Student, this is the department they major in.
    ///     If the user is an Administrator, this field is not present in the returned JSON
    /// </summary>
    /// <param name="uid">The ID of the user</param>
    /// <returns>
    ///     The user JSON object
    ///     or an object containing {success: false} if the user doesn't exist
    /// </returns>
    public IActionResult GetUser(string uid)
    {
        // find the user in the student table
        // join w the department table to get the department name
        var studentQuery =
            from s in db.Students
            where s.UId == uid
            join d in db.Departments on s.Subject equals d.Subject
            select new
            {
                fname = s.FirstName,
                lname = s.LastName,
                uid = s.UId,
                department = d.Name
            };

        // see if student was found
        var student = studentQuery.FirstOrDefault();
        if (student != null)
        {
            return Json(student);
        }

        // then check prof table
        var professorQuery =
            from p in db.Professors
            where p.UId == uid
            join d in db.Departments on p.Subject equals d.Subject
            select new
            {
                fname = p.FirstName,
                lname = p.LastName,
                uid = p.UId,
                department = d.Name
            };

        // see if prof was found
        var professor = professorQuery.FirstOrDefault();
        if (professor != null)
        {
            return Json(professor);
        }

        // check admin table next
        var adminQuery =
            from a in db.Administrators
            where a.UId == uid
            select new
            {
                fname = a.FirstName,
                lname = a.LastName,
                uid = a.UId
            };

        // see if admin was found
        var admin = adminQuery.FirstOrDefault();
        if (admin != null)
        {
            return Json(admin);
        }

        return Json(new { success = false });
    }


    /*******End code to modify********/
}