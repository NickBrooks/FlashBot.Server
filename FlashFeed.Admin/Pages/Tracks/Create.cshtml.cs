﻿using FlashFeed.Admin.Data;
using FlashFeed.Admin.Pages.Account.Manage;
using FlashFeed.Engine;
using FlashFeed.Engine.Models;
using FlashFeed.Engine.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FlashFeed.Admin.Pages.Tracks
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

        public ApplicationUser CurrentUser { get; set; }
        public ExtendedUser ExtendedUser { get; set; }
        public int PrivateTracksLeft { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [MaxLength(80)]
            [Display(Name = "Track Name")]
            public string Name { get; set; }

            [Display(Name = "Track Description")]
            [DataType(DataType.Text)]
            [MaxLength(140)]
            public string Description { get; set; }

            [Display(Name = "Private")]
            public bool IsPrivate { get; set; }

            [Display(Name = "Tags")]
            public string Tags { get; set; }

            [Display(Name = "Track Image")]
            public IFormFile UploadTrackImage { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            CurrentUser = user ?? throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            ExtendedUser = await ExtendedUserRepository.GetExtendedUser(user.Id);
            PrivateTracksLeft = ExtendedUser.Private_Tracks_Max - ExtendedUser.Private_Tracks;

            return Page();
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

            var track = new Track()
            {
                owner_id = user.Id,
                description = Input.Description,
                is_private = Input.IsPrivate,
                name = Input.Name,
                tags = Tools.ValidateTags(Input.Tags.Split(',').ToList())
            };

            // create track
            var createdTrack = await TrackRepository.CreateTrack(track);
            _logger.LogInformation($"Track with ID '{createdTrack.id}' has been created by '{user.Id}'.");
            return RedirectToPage("./Index");
        }
    }
}