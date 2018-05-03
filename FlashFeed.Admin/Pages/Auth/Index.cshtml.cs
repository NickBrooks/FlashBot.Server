using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FlashFeed.Admin.Data;
using FlashFeed.Admin.Pages.Account.Manage;
using FlashFeed.Engine.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace FlashFeed.Admin.Pages.Auth
{
    public class TokenModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;

        public TokenModel(
            UserManager<ApplicationUser> userManager,
            ILogger<EnableAuthenticatorModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public string _refreshToken { get; set; }
        public ApplicationUser _currentUser { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            _currentUser = user ?? throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            _refreshToken = await AuthRepository.GenerateRefreshToken(user.Id);

            return Page();
        }
    }
}