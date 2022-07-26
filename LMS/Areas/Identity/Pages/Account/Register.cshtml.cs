// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using LMS.Models;
using LMS.Models.LMSModels;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Student = LMS.Models.LMSModels.Student;

namespace LMS.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        //private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        //private readonly IEmailSender _emailSender;

        private readonly LMSContext _db;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            LMSContext db
            /*IEmailSender emailSender*/)
        {
            _userManager = userManager;
            _userStore = userStore;
            //_emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            //_emailSender = emailSender;
            _db = db;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

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

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {

            [Required]
            [Display(Name = "Role")]
            public string Role { get; set; }

            public List<SelectListItem> Roles { get; } = new List<SelectListItem>
            {
                new SelectListItem { Value = "Student", Text = "Student" },
                new SelectListItem { Value = "Professor", Text = "Professor" },
                new SelectListItem { Value = "Administrator", Text = "Administrator"  }
            };

            public string Department { get; set; }

            public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>
            {
                new SelectListItem{Value = "None", Text = "NONE"}
            };

            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            public System.DateTime DOB { get; set; }

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

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);

                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
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
        /// Create a new user account and insert into your database (the one you designed in phase1/2 not the
        /// Authorization DB that dotnet creates for you.  Other code does that for you)
        /// </summary>
        /// <returns>
        /// A new unique uID that is not currently in use by any student, professor, or administrator
        /// </returns>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="DOB"></param>
        /// <param name="departmentAbbrev"></param>
        /// <param name="role"></param>
        string CreateNewUser(string firstName, string lastName, DateTime DOB, string departmentAbbrev, string role)
        {

            //check user not in database already, then create new uID
            //If db is empty, don't do the query


            var max_s_Uid =
                (from s in _db.Students
                 orderby s.UId descending
                 select s.UId).Take(1);

            var max_p_Uid =
                (from p in _db.Professors
                 orderby p.UId descending
                 select p.UId).Take(1);

            var max_a_Uid =
                (from a in _db.Administrators
                 orderby a.UId descending
                 select a.UId).Take(1);

            int s_Uid = 0;
            int p_Uid = 0;
            int a_Uid = 0;

            if (max_s_Uid.Any())
            {
                string string_id = (max_s_Uid.FirstOrDefault());
                s_Uid = int.Parse(string_id.Remove(0, 1));
            }

            if (max_p_Uid.Any())
            {
                string string_id = (max_p_Uid.FirstOrDefault());
                p_Uid = int.Parse(string_id.Remove(0, 1));
            }

            if (max_a_Uid.Any())
            {
                string string_id = (max_a_Uid.First());
                a_Uid = int.Parse(string_id.Remove(0, 1));
            }

            //int s_Uid = int.Parse(max_s_Uid.FirstOrDefault());
            //int p_Uid = int.Parse(max_p_Uid.FirstOrDefault());
            //int a_Uid = int.Parse(max_a_Uid.FirstOrDefault());

            int max_Uid = Math.Max(s_Uid, Math.Max(p_Uid, a_Uid));

            max_Uid = Math.Max(max_Uid, 0);

            //int max_Uid = Collections.max(Arrays.asList(s_Uid, p_Uid, a_Uid, 0));

            int new_Uid = max_Uid += 1;

            string uIDS_String = new_Uid.ToString();

            string uID = "u";

            while (uIDS_String.Length < 7)
            {
                uIDS_String = "0" + uIDS_String;
            }

            uID = uID + uIDS_String;


            if (role.Equals("Student"))
            {
                Console.WriteLine("ADDING STUDENT");
                Student st = new Student();
                st.Dob = DOB;
                st.FirstName = firstName;
                st.LastName = lastName;
                st.MajorDept = departmentAbbrev;
                //st.MajorDept = "test";
                st.UId = uID;
                
                _db.Students.Add(st);
                _db.SaveChanges();

                Console.WriteLine("FINISHED ADDING STUDENT");

            }
            else if (role.Equals("Professor"))
            {
                Console.WriteLine("ADDING PROFESSOR");
                Professor pf = new Professor();
                pf.FirstName = firstName;
                pf.LastName = lastName;
                pf.Dob = DOB;
                pf.WorkDept = departmentAbbrev;
                //pf.WorkDept = "test";
                pf.UId = uID;

                _db.Professors.Add(pf);
                _db.SaveChanges();

                Console.WriteLine("FINISHED ADDING PROFESSOR");


            }
            else if (role.Equals("Administrator"))
            {
                Console.WriteLine("ADDING ADMIN");
                Administrator ad = new Administrator();
                ad.FirstName = firstName;
                ad.LastName = lastName;
                ad.Dob = DOB;
                ad.UId = uID;

                _db.Administrators.Add(ad);
                _db.SaveChanges();

                Console.WriteLine("FINISHED ADDING ADMIN");

            }

            return uID;
        }

        /*******End code to modify********/
    }
}
