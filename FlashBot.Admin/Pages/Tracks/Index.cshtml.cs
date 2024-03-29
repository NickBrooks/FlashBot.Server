using FlashBot.Admin.Data;
using FlashBot.Admin.Pages.Account.Manage;
using FlashBot.Engine.Models;
using FlashBot.Engine.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashBot.Admin.Pages.Tracks
{
    public class TracksModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;

        public TracksModel(
            UserManager<ApplicationUser> userManager,
            ILogger<EnableAuthenticatorModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public List<TrackAuth> _tracks { get; set; }
        public ApplicationUser _currentUser { get; set; }
        public ExtendedUser _extendedUser { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            _currentUser = user ?? throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            _tracks = await TrackRepository.GetTracksByOwnerId(user.Id);

            foreach (var track in _tracks)
            {
                track.track_key = AuthRepository.EncodeKeyAndSecret(track.track_key, track.track_secret);
            }

            _extendedUser = await ExtendedUserRepository.GetExtendedUser(user.Id);

            if (_extendedUser == null)
            {
                _extendedUser = await ExtendedUserRepository.CreateExtendedUser(new ExtendedUser(user.Id));
            }

            return Page();
        }
    }
}