using FlashFeed.Admin.Data;
using FlashFeed.Admin.Pages.Account.Manage;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlashFeed.Admin.Pages.Tracks
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

        public List<Track> Tracks { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public ExtendedUser ExtendedUser { get; set; }
        public string TrackKey { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            CurrentUser = user;
            Tracks = await TrackRepository.GetTracks(user.Id);

            foreach (var track in Tracks)
            {
                track.track_key = AuthRepository.EncodeKeyAndSecretToBase64(track.track_key, track.track_secret);
            }

            ExtendedUser = await ExtendedUserRepository.GetExtendedUser(user.Id);

            if (ExtendedUser == null)
            {
                ExtendedUser = await ExtendedUserRepository.CreateExtendedUser(new ExtendedUser(user.Id));
            }

            return Page();
        }
    }
}