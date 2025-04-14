// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using LMS.Models;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LMS.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    //private readonly IUserEmailStore<IdentityUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;

    private readonly LMSContext db;
    //private readonly IEmailSender _emailSender;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        LMSContext _db
        /*IEmailSender emailSender*/)
    {
        _userManager = userManager;
        _userStore = userStore;
        //_emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        db = _db;
        //_emailSender = emailSender;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel Input { get; set; } = new();

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string ReturnUrl { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; }


    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            var uid = CreateNewUser(Input.FirstName, Input.LastName, Input.DOB, Input.Department, Input.Role);
            var user = new ApplicationUser { UserName = uid };

            await _userStore.SetUserNameAsync(user, uid, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
                await _userManager.AddToRoleAsync(user, Input.Role);

                var userId = await _userManager.GetUserIdAsync(user);

                await _signInManager.SignInAsync(user, false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    private IdentityUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                                                $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    /*******Begin code to modify********/


    /// <summary>
    ///     Create a new user of the LMS with the specified information and add it to the database.
    ///     Assigns the user a unique uID consisting of a 'u' followed by 7 digits.
    /// </summary>
    /// <param name="firstName">The user's first name</param>
    /// <param name="lastName">The user's last name</param>
    /// <param name="DOB">The user's date of birth</param>
    /// <param name="departmentAbbrev">The department abbreviation that the user belongs to (ignore for Admins) </param>
    /// <param name="role">The user's role: one of "Administrator", "Professor", "Student"</param>
    /// <returns>The uID of the new user</returns>
    private string CreateNewUser(string firstName, string lastName, DateTime DOB, string departmentAbbrev, string role)
    {
        var max_admin = 0;
        var max_prof = 0;
        var max_stu = 0;

        var query1 = from a in db.Administrators
            select a.UId;
        Console.WriteLine("this is query1 max: " + query1.Max());
        var query2 = from p in db.Professors
            select p.UId;
        Console.WriteLine("this is query2 max: " + query2.Max());

        var query3 = from s in db.Students
            select s.UId;
        Console.WriteLine("this is query3 max: " + query3.Max());


        if (int.TryParse(query1.Max().Substring(1), out var result))
        {
            max_admin = result;
            Console.WriteLine("this is max_admin: " + max_admin);
        }


        if (int.TryParse(query2.Max().Substring(1), out var result2))
        {
            max_prof = result2;
            Console.WriteLine("this is max_prof: " + max_prof);
        }


        if (int.TryParse(query3.Max().Substring(1), out var result3))
        {
            max_stu = result3;
            Console.WriteLine("this is max_stu: " + max_stu);
        }


        Console.WriteLine(max_admin + ", " + max_prof + ", " + max_stu);

        int max_uID;

        if (max_admin > max_prof && max_admin > max_stu)
            max_uID = max_admin;
        else if (max_prof > max_admin && max_prof > max_stu)
            max_uID = max_prof;
        else if (max_stu > max_prof && max_stu > max_admin)
            max_uID = max_stu;
        else
            max_uID = 0;


        Console.WriteLine("This is max_uid_num: " + max_uID);

        if (role.Equals("Administrator"))
        {
            var admin = new Administrator
            {
                UId = generateUID(max_uID),
                FirstName = firstName,
                LastName = lastName,
                Dob = DateOnly.FromDateTime(DOB)
            };

            db.Administrators.Add(admin);
            db.SaveChanges();

            return admin.UId;
        }

        if (role.Equals("Professor"))
        {
            var prof = new Professor
            {
                UId = generateUID(max_uID),
                FirstName = firstName,
                LastName = lastName,
                Dob = DateOnly.FromDateTime(DOB),
                Subject = departmentAbbrev
            };

            db.Professors.Add(prof);
            db.SaveChanges();

            return prof.UId;
        }

        var stu = new Student
        {
            UId = generateUID(max_uID),
            FirstName = firstName,
            LastName = lastName,
            Dob = DateOnly.FromDateTime(DOB),
            Subject = departmentAbbrev
        };

        db.Students.Add(stu);
        db.SaveChanges();

        return stu.UId;
    }

    // query all three databases, get the max uID by parsing them, make a variable and ad one to it,
    // then add 1 to it and use that as the new users uID
    private string generateUID(int max)
    {
        var newID_num = 0;

        if (max != 0) newID_num = max + 1;

        Console.WriteLine("this is newID_Num: " + newID_num);

        var max_num_string = "";

        var length = max_num_string.Length;

        max_num_string += "u";
        while (length < 6)
        {
            max_num_string += "0";
            length++;
            Console.WriteLine(max_num_string);
        }

        max_num_string += newID_num;
        Console.WriteLine("this is the complete new UID: " + max_num_string);
        // newID_num = max_num + 1;
        return max_num_string;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        [Required] [Display(Name = "Role")] public string Role { get; set; }

        public List<SelectListItem> Roles { get; } = new()
        {
            new SelectListItem { Value = "Student", Text = "Student" },
            new SelectListItem { Value = "Professor", Text = "Professor" },
            new SelectListItem { Value = "Administrator", Text = "Administrator" }
        };

        public string Department { get; set; }

        public List<SelectListItem> Departments { get; set; } = new()
        {
            new SelectListItem { Value = "None", Text = "NONE" }
        };

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Date of Birth")]
        [BindProperty]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; } = DateTime.Now;

        [Required]
        //[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /*******End code to modify********/
}