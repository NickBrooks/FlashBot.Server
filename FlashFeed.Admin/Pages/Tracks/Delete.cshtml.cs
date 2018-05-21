using FlashBot.Admin.Data;
using FlashBot.Admin.Pages.Account.Manage;
using FlashBot.Engine.Models;
using FlashBot.Engine.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FlashBot.Admin.Pages.Tracks
{
    public class DeleteModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;

        public DeleteModel(
            UserManager<ApplicationUser> userManager,
            ILogger<EnableAuthenticatorModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public TrackAuth Track { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public ExtendedUser ExtendedUser { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            CurrentUser = user ?? throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            Track = await TrackRepository.GetTrackVerifyOwner(id, user.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Track = await TrackRepository.GetTrackVerifyOwner(id, user.Id);

            // cant find or no permission to delete
            if (Track == null)
                return RedirectToPage("./Index");

            await TrackRepository.DeleteTrack(Track.RowKey);
            _logger.LogInformation($"Track with ID '{Track.RowKey}' has been deleted by '{user.Id}'.");
            return RedirectToPage("./Index");
        }
    }
}