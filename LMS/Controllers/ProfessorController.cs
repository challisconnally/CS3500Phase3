using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query.Internal;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]

namespace LMS_CustomIdentity.Controllers;

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

    public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname,
        string uid)
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
    ///     Returns a JSON array of all the students in a class.
    ///     Each object in the array should have the following fields:
    ///     "fname" - first name
    ///     "lname" - last name
    ///     "uid" - user ID
    ///     "dob" - date of birth
    ///     "grade" - the student's grade in this class
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
    {
        var query =
            from course in db.Courses
            join cl in db.Classes on course.CourseId equals cl.CourseId into cc
            from stuff in cc.DefaultIfEmpty()
            join e in db.Enrolleds on stuff.ClassId equals e.ClassId into info
            from i in info.DefaultIfEmpty()
            join s in db.Students on i.UId equals s.UId into stu_info
            from si in stu_info.DefaultIfEmpty()
            where course.Subject.Equals(subject)
                  && course.CourseNum == num
                  && stuff.Season.Equals(season)
                  && stuff.Year == year
            select new
            {
                fname = si.FirstName,
                lname = si.LastName,
                uid = si.UId,
                dob = si.Dob,
                grade = i.Grade
            };
        return Json(query.ToArray());
    }


    /// <summary>
    ///     Returns a JSON array with all the assignments in an assignment category for a class.
    ///     If the "category" parameter is null, return all assignments in the class.
    ///     Each object in the array should have the following fields:
    ///     "aname" - The assignment name
    ///     "cname" - The assignment category name.
    ///     "due" - The due DateTime
    ///     "submissions" - The number of submissions to the assignment
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">
    ///     The name of the assignment category in the class,
    ///     or null to return assignments from all categories
    /// </param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
    {
        var query =
            (from course in db.Courses
             join c in db.Classes on course.CourseId equals c.CourseId
             where course.Subject.Equals(subject)
                   && course.CourseNum == num
                   && c.Season.Equals(season)
                   && c.Year == year
             select c.ClassId).FirstOrDefault();

        if (query == 0)
        {
            return Json(new List<object>()); // if class not found return empty array
        }

        var assignQuery =
            from ac in db.AssignmentCategories
            join a in db.Assignments on ac.AssignmentCategoriesId equals a.AssignmentCategoriesId
            where ac.ClassId == query
                  && (category == null || ac.Name == category)
            select new
            {
                aname = a.Name,
                cname = ac.Name,
                due = a.DueDate,
                submissions = db.Submissions.Count(s => s.AssignmentId == a.AssignmentId)
            };

        return Json(assignQuery.ToArray());
    }


    /// <summary>
    ///     Returns a JSON array of the assignment categories for a certain class.
    ///     Each object in the array should have the folling fields:
    ///     "name" - The category name
    ///     "weight" - The category weight
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
    {
        var query =
            from cour in db.Courses
            where cour.Subject.Equals(subject) && cour.CourseNum == num
            join cc in db.Classes on cour.CourseId equals cc.CourseId
            where cc.Season.Equals(season) && cc.Year == year
            join ac in db.AssignmentCategories on cc.ClassId equals ac.ClassId
            select new
            {
                name = ac.Name,
                weight = ac.GradeWeight
            };

        return Json(query.ToArray());
    }

    /// <summary>
    ///     Creates a new assignment category for the specified class.
    ///     If a category of the given class with the given name already exists, return success = false.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The new category name</param>
    /// <param name="catweight">The new category weight</param>
    /// <returns>A JSON object containing {success = true/false} </returns>
    public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category,
        int catweight)
    {
        Console.WriteLine(subject + ", " + num + ", " + year + ", " + season);

        var query =
            from cour in db.Courses
            where cour.Subject.Equals(subject) && cour.CourseNum == num
            join cc in db.Classes on cour.CourseId equals cc.CourseId
            where cc.Season.Equals(season) && cc.Year == year
            join ac in db.AssignmentCategories on cc.ClassId equals ac.ClassId
            where ac.Name == category
            select ac;

        if (query.Any())
        {
            return Json(new { success = false });
        }

        var query2 =
            from cour in db.Courses
            join cc in db.Classes on cour.CourseId equals cc.CourseId
            where cc.Season == season && cc.Year == year &&
                    cour.Subject == subject && cour.CourseNum == num
            select cc.ClassId;

        int id = query2.FirstOrDefault();

        Console.WriteLine("This is the id value: " + id);
        var cat = new AssignmentCategory
        {
            Name = category,
            GradeWeight = (uint)catweight,
            ClassId = id
        };

        db.AssignmentCategories.Add(cat);
        db.SaveChanges();

        return Json(new { success = true });

    }

    /// <summary>
    ///     Creates a new assignment for the given class and category.
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
    public IActionResult CreateAssignment(string subject, int num, string season, int year, string category,
        string asgname, int asgpoints, DateTime asgdue, string asgcontents)
    {
        var query =
        from c in db.Courses
        where c.Subject.Equals(subject) && c.CourseNum == num
        join stuff in db.Classes on c.CourseId equals stuff.CourseId
        where stuff.Season.Equals(season) && stuff.Year == year
        join ac in db.AssignmentCategories on stuff.ClassId equals ac.ClassId
        where ac.Name.Equals(category)
        join a in db.Assignments on ac.AssignmentCategoriesId equals a.AssignmentCategoriesId
        where a.Name.Equals(asgname)
        select a;

        if (query.Any())
        {
            return Json(new { success = false });
        }

        var query2 =
        from cour in db.Courses
        where cour.Subject.Equals(subject) && cour.CourseNum == num
        join cc in db.Classes on cour.CourseId equals cc.CourseId
        where cc.Season.Equals(season) && cc.Year == year
        join ac in db.AssignmentCategories on cc.ClassId equals ac.ClassId
        where ac.Name.Equals(category)
        select ac.AssignmentCategoriesId;

        int id = query2.First();

        var asgmnt = new Assignment
        {
            Name = asgname,
            Contents = asgcontents,
            MaxPoints = (uint)asgpoints,
            DueDate = asgdue,
            AssignmentCategoriesId = id,
        };

        db.Assignments.Add(asgmnt);
        db.SaveChanges();

        return Json(new { success = true });
    }


    /// <summary>
    ///     Gets a JSON array of all the submissions to a certain assignment.
    ///     Each object in the array should have the following fields:
    ///     "fname" - first name
    ///     "lname" - last name
    ///     "uid" - user ID
    ///     "time" - DateTime of the submission
    ///     "score" - The score given to the submission
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category,
        string asgname)
    {
        var query =

        from c in db.Courses
        where c.Subject.Equals(subject) && c.CourseNum == num
        join stuff in db.Classes on c.CourseId equals stuff.CourseId
        where stuff.Season.Equals(season) && stuff.Year == year
        select stuff.ClassId;

        int cID = query.FirstOrDefault();

        var query2 =
            from p in db.AssignmentCategories
            join ac in db.AssignmentCategories on p.ClassId equals ac.ClassId
            where ac.Name.Equals(category)
            join a in db.Assignments on ac.AssignmentCategoriesId equals a.AssignmentCategoriesId
            where a.Name.Equals(asgname)
            join sub in db.Submissions on a.AssignmentId equals sub.AssignmentId into s
            from f in s.DefaultIfEmpty()
            join stu in db.Students on f.UId equals stu.UId into k
            from total in k.DefaultIfEmpty()
            where p.ClassId == cID
            select new
            {
                fname = total.FirstName,
                lname = total.LastName,
                uid = f.UId,
                time = f.SubmissionTime,
                score = f.Score
            };


        return Json(query2.ToArray());
    }


    /// <summary>
    ///     Set the score of an assignment submission
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
    public IActionResult GradeSubmission(string subject, int num, string season, int year, string category,
        string asgname, string uid, int score)
    {
        var query =

        (from c in db.Courses
         where c.Subject.Equals(subject) && c.CourseNum == num
         join stuff in db.Classes on c.CourseId equals stuff.CourseId
         where stuff.Season.Equals(season) && stuff.Year == year
         join ac in db.AssignmentCategories on stuff.ClassId equals ac.ClassId
         where ac.Name.Equals(category)
         join a in db.Assignments on ac.AssignmentCategoriesId equals a.AssignmentCategoriesId
         where a.Name.Equals(asgname)
         join sub in db.Submissions on a.AssignmentId equals sub.AssignmentId
         where sub.UId.Equals(uid)
         select sub).FirstOrDefault();

        if (query != null)
        {
            query.Score = (uint)score;
            db.SaveChanges();
            
            computeLetterGrade(subject, num, season, year, category, asgname, uid, score);

            return Json(new { success = true });
        }
        else
            return Json(new { success = false });


    }


    /// <summary>
    ///     Returns a JSON array of the classes taught by the specified professor
    ///     Each object in the array should have the following fields:
    ///     "subject" - The subject abbreviation of the class (such as "CS")
    ///     "number" - The course number (such as 5530)
    ///     "name" - The course name
    ///     "season" - The season part of the semester in which the class is taught
    ///     "year" - The year part of the semester in which the class is taught
    /// </summary>
    /// <param name="uid">The professor's uid</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetMyClasses(string uid)
    {
        var query =

        from c in db.Courses
        join stuff in db.Classes on c.CourseId equals stuff.CourseId
        where stuff.UId.Equals(uid)
        select new
        {
            subject = c.Subject,
            number = c.CourseNum,
            name = c.Name,
            season = stuff.Season,
            year = stuff.Year,
        };

        return Json(query.ToArray());
    }




    private void computeLetterGrade(string subject, int num, string season, int year, string category,
        string asgname, string uid, int score)
    {
        var cQuery =
               //from s in db.Students
               //join se in db.Enrolleds on s.UId equals se.UId
               (from cl in db.Classes
                join c in db.Classes on cl.ClassId equals c.ClassId
                join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                join cour in db.Courses on c.CourseId equals cour.CourseId
                where cour.Subject.Equals(subject) && cour.CourseNum == num
                where c.Season.Equals(season) && c.Year == year
                select ac.Name).ToList();

        double scaledWeight = 0;
        double sumCatWeights = 0;

        foreach (string cat in cQuery)
        {
            var maxQuery =
                (from s in db.Students
                 join se in db.Enrolleds on s.UId equals se.UId
                 join c in db.Classes on se.ClassId equals c.ClassId
                 join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                 join a in db.Assignments on ac.AssignmentCategoriesId equals a.AssignmentCategoriesId
                 join cour in db.Courses on c.CourseId equals cour.CourseId
                 where s.UId.Equals(uid)
                 where cour.Subject.Equals(subject) && cour.CourseNum == num
                 where c.Season.Equals(season) && c.Year == year
                 where ac.Name.Equals(cat)

                 select a.MaxPoints).ToList();

            var pointsQuery =
                (from s in db.Students
                 join se in db.Enrolleds on s.UId equals se.UId
                 join c in db.Classes on se.ClassId equals c.ClassId
                 join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                 join a in db.Assignments on ac.AssignmentCategoriesId equals a.AssignmentCategoriesId
                 join cour in db.Courses on c.CourseId equals cour.CourseId
                 join sub in db.Submissions on a.AssignmentId equals sub.AssignmentId
                 where s.UId.Equals(uid)
                 where cour.Subject.Equals(subject) && cour.CourseNum == num
                 where c.Season.Equals(season) && c.Year == year
                 where ac.Name.Equals(cat)

                 select sub.Score).ToList();

            List<uint> max = maxQuery;
            List<uint> points = pointsQuery;

            double maxTotal = 0;
            double maxPoints = 0;

            foreach (var m in max)
            {
                maxTotal += (double)m;
            }

            foreach (var p in points)
            {
                maxPoints += (double)p;
            }

            double ratio = maxPoints / maxTotal;

            var catQuery = (from s in db.Students
                            join se in db.Enrolleds on s.UId equals se.UId
                            join c in db.Classes on se.ClassId equals c.ClassId
                            join ac in db.AssignmentCategories on c.ClassId equals ac.ClassId
                            join a in db.Assignments on ac.AssignmentCategoriesId equals a.AssignmentCategoriesId
                            join cour in db.Courses on c.CourseId equals cour.CourseId
                            where s.UId.Equals(uid)
                            where cour.Subject.Equals(subject) && cour.CourseNum == num
                            where c.Season.Equals(season) && c.Year == year
                            where ac.Name.Equals(cat)
                            select ac.GradeWeight).FirstOrDefault();

            double weighedRatio = ratio * catQuery;
            sumCatWeights += catQuery;
            scaledWeight += weighedRatio;

        }

        double scalingFactor = 100 / sumCatWeights;

        double totalScore = scalingFactor * scaledWeight;

        getLetterGrade(totalScore, uid);
    }

    private void getLetterGrade(double totalScore, string uid)
    {
        string letterGrade = "";

        if (totalScore <= 100 && totalScore >= 93)
        {
            letterGrade = "A";
        }
        else if (totalScore < 93 && totalScore >= 90)
        {
            letterGrade = "A-";
        }
        else if (totalScore < 90 && totalScore >= 87)
        {
            letterGrade = "B+";
        }
        else if (totalScore < 87 && totalScore >= 83)
        {
            letterGrade = "B";
        }
        else if (totalScore < 83 && totalScore >= 80)
        {
            letterGrade = "B-";
        }
        else if (totalScore < 80 && totalScore >= 77)
        {
            letterGrade = "C+";
        }
        else if (totalScore < 77 && totalScore >= 73)
        {
            letterGrade = "C";
        }
        else if (totalScore < 73 && totalScore >= 70)
        {
            letterGrade = "C-";
        }
        else if (totalScore < 70 && totalScore >= 67)
        {
            letterGrade = "D+";
        }
        else if (totalScore < 67 && totalScore >= 63)
        {
            letterGrade = "D";
        }
        else if (totalScore < 63 && totalScore >= 60)
        {
            letterGrade = "D-";
        }
        else if (totalScore < 60 && totalScore >= 0)
        {
            letterGrade = "E";
        }

        var enrollQuery =
            (from e in db.Enrolleds
             where e.UId == uid
             select e).FirstOrDefault();

        if (enrollQuery != null)
        {
            enrollQuery.Grade = letterGrade;
            db.SaveChanges();
        }
    }

    /*******End code to modify********/
}