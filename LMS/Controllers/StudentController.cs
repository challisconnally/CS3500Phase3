using System.Diagnostics;
using System.Runtime.CompilerServices;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]

namespace LMS.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly LMSContext db;

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
        Debug.WriteLine(subject + num);
        ViewData["subject"] = subject;
        ViewData["num"] = num;
        return View();
    }


    /*******Begin code to modify********/

    /// <summary>
    ///     Returns a JSON array of the classes the given student is enrolled in.
    ///     Each object in the array should have the following fields:
    ///     "subject" - The subject abbreviation of the class (such as "CS")
    ///     "number" - The course number (such as 5530)
    ///     "name" - The course name
    ///     "season" - The season part of the semester
    ///     "year" - The year part of the semester
    ///     "grade" - The grade earned in the class, or "--" if one hasn't been assigned
    /// </summary>
    /// <param name="uid">The uid of the student</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetMyClasses(string uid)
    {
        var classes =
            from e in db.Enrolleds
            join cl in db.Classes on e.ClassId equals cl.ClassId
            join c in db.Courses on cl.CourseId equals c.CourseId
            where e.UId == uid
            select new
            {
                subject = c.Subject,
                number = c.CourseNum,
                name = c.Name,
                season = cl.Season,
                year = cl.Year,
                grade = e.Grade ?? "--"
            };
        return Json(classes.ToArray());
    }

    /// <summary>
    ///     Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
    ///     Each object in the array should have the following fields:
    ///     "aname" - The assignment name
    ///     "cname" - The category name that the assignment belongs to
    ///     "due" - The due Date/Time
    ///     "score" - The score earned by the student, or null if the student has not submitted to this assignment.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="uid"></param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
    {
        var query =
            from e in db.Enrolleds
            join c in db.Classes on e.ClassId equals c.ClassId
            join cour in db.Courses on c.CourseId equals cour.CourseId
            join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
            join assign in db.Assignments on ac.AssignmentCategoriesId equals assign.AssignmentCategoriesId
            join sub in db.Submissions on assign.AssignmentId equals sub.AssignmentId
            where e.UId == uid
            where cour.CourseNum == num
            where c.Season == season
            where c.Year == year
            where cour.Subject == subject
            select new
            {
                aname = assign.Name,
                cname = ac.Name,
                due = assign.DueDate,
                score = sub.Score
            };

        return Json(null);
    }


    /// <summary>
    ///     Adds a submission to the given assignment for the given student
    ///     The submission should use the current time as its DateTime
    ///     You can get the current time with DateTime.Now
    ///     The score of the submission should start as 0 until a Professor grades it
    ///     If a Student submits to an assignment again, it should replace the submission contents
    ///     and the submission time (the score should remain the same).
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
        var assignmentQuery = 
            (from course in db.Courses
                join c in db.Classes on course.CourseId equals c.CourseId
                join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                join a in db.Assignments on ac.AssignmentCategoriesId equals a.AssignmentCategoriesId
                where course.Subject == subject
                      && course.CourseNum == num
                      && c.Season == season
                      && c.Year == year
                      && ac.Name == category
                      && a.Name == asgname
                        select a).FirstOrDefault();

        if (assignmentQuery == null)
        {
            return Json(new { success = false });
        }

        var submissionQuery =
            (from s in db.Submissions
                where s.UId == uid && s.AssignmentId == assignmentQuery.AssignmentId
                select s).FirstOrDefault();

        if (submissionQuery != null)
        {
            submissionQuery.Contents = contents;
            submissionQuery.SubmissionTime = DateTime.Now;
        }
        else
        {
            var newSubmission = new Submission
            {
                UId = uid,
                AssignmentId = assignmentQuery.AssignmentId,
                Contents = contents,
                SubmissionTime = DateTime.Now,
                Score = 0
            };
            db.Submissions.Add(newSubmission);
        }

        db.SaveChanges();

        return Json(new { success = true });
    }


    /// <summary>
    ///     Enrolls a student in a class.
    /// </summary>
    /// <param name="subject">The department subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester</param>
    /// <param name="year">The year part of the semester</param>
    /// <param name="uid">The uid of the student</param>
    /// <returns>
    ///     A JSON object containing {success = {true/false}.
    ///     false if the student is already enrolled in the class, true otherwise.
    /// </returns>
    public IActionResult Enroll(string subject, int num, string season, int year, string uid)
    {
        var query =
            from cl in db.Classes
            join course in db.Courses on cl.CourseId equals course.CourseId
            where course.Subject == subject
                  && course.CourseNum == num
                  && cl.Season == season
                  && cl.Year == year
            select cl;

        var classObj = query.FirstOrDefault();

        if (classObj == null)
        {
            return Json(new { success = false });
        }

        var enrolledQuery =
            from e in db.Enrolleds
            where e.UId == uid && e.ClassId == classObj.ClassId
            select e;

        if (enrolledQuery.Any())
        {
            return Json(new { success = false });
        }

        var enrollment = new Enrolled
        {
            UId = uid,
            ClassId = classObj.ClassId,
            Grade = "--"
        };

        db.Enrolleds.Add(enrollment);
        db.SaveChanges();

        return Json(new { success = true });
    }


    /// <summary>
    ///     Calculates a student's GPA
    ///     A student's GPA is determined by the grade-point representation of the average grade in all their classes.
    ///     Assume all classes are 4 credit hours.
    ///     If a student does not have a grade in a class ("--"), that class is not counted in the average.
    ///     If a student is not enrolled in any classes, they have a GPA of 0.0.
    ///     Otherwise, the point-value of a letter grade is determined by the table on this page:
    ///     https://advising.utah.edu/academic-standards/gpa-calculator-new.php
    /// </summary>
    /// <param name="uid">The uid of the student</param>
    /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
    public IActionResult GetGPA(string uid)
    {
        double calc_gpa = 0.0;
        var query =
            from e in db.Enrolleds
            where e.UId == uid
            select e.Grade;

        if (!query.Any())
        {
            return Json(new { gpa = calc_gpa });
        }

        List<String> gradeList = query.ToList();
        Console.WriteLine("This is the grade list:" + gradeList.First());
        double totalGradePoints = 0;
        int totalClasses = 0;
        foreach (var grade in gradeList)
        {
            if (grade == "A")
            {
                totalGradePoints += 4.0;
                totalClasses++;
            }
            else if (grade == "A-")
            {
                totalGradePoints += 3.7;
                totalClasses++;
            }
            else if (grade == "B+")
            {
                totalGradePoints += 3.3;
                totalClasses++;
            }
            else if (grade == "B")
            {
                totalGradePoints += 3.0;
                totalClasses++;
            }
            else if (grade == "B-")
            {
                totalGradePoints += 2.7;
                totalClasses++;
            }
            else if (grade == "C+")
            {
                totalGradePoints += 2.3;
                totalClasses++;
            }
            else if (grade == "C")
            {
                totalGradePoints += 2.0;
                totalClasses++;
            }
            else if (grade == "C-")
            {
                totalGradePoints += 1.7;
                totalClasses++;
            }
            else if (grade == "D+")
            {
                totalGradePoints += 1.3;
                totalClasses++;
            }
            else if (grade == "D")
            {
                totalGradePoints += 1.0;
                totalClasses++;
            }
            else if (grade == "D-")
            {
                totalGradePoints += 0.7;
                totalClasses++;
            }
            else if (grade == "E")
            {
                totalGradePoints += 0.0;
                totalClasses++;
            } else
            {
                continue;
            }
        }
        
        if (totalClasses == 0)
        {
            calc_gpa = 0.0;
        }
        else
        {
            calc_gpa = (totalGradePoints / totalClasses);
        }

        return Json( new {gpa = calc_gpa});
    }

    /*******End code to modify********/
}