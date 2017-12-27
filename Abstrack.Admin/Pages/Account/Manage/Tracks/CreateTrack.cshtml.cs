using Abstrack.Admin.Data;
using Abstrack.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Abstrack.Admin.Pages.Account.Manage.Tracks
{
    public class CreateTrackModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;

        public CreateTrackModel(
            UserManager<ApplicationUser> userManager,
            ILogger<EnableAuthenticatorModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Track Name")]
            public string Name { get; set; }

            [Display(Name = "Track Description")]
            public string Description { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // create track
            var createdTrack = await TrackRepository.CreateTrack(user.Id, Input.Name, Input.Description);


            _logger.LogInformation($"Track with ID '{createdTrack.id}' has been created by '{user.Id}'.");
            return RedirectToPage($"./{createdTrack.id}");
        }
    }
}