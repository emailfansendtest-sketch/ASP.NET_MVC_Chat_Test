#nullable disable

using Contracts.Interfaces;
using DomainModels;

using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace MVC_SSL_Chat.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Register page's code behind implementation - almost identical to the default.
    /// Confirmation email is sent by the custom sender implemented in the scope of the solution.
    /// </summary>
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ChatUser> _signInManager;
        private readonly UserManager<ChatUser> _userManager;
        private readonly IUserStore<ChatUser> _userStore;
        private readonly IUserEmailStore<ChatUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IConfirmationEmailSender _confirmationSender;

        public RegisterModel(
            UserManager<ChatUser> userManager,
            IUserStore<ChatUser> userStore,
            SignInManager<ChatUser> signInManager,
            ILogger<RegisterModel> logger,
            IConfirmationEmailSender confirmationSender )
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _confirmationSender = confirmationSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [DataType( DataType.Text )]
            [StringLength( 100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6 )]
            [Display( Name = "Username" )]
            public string Username { get; set; }

            [Required]
            [EmailAddress]
            [Display( Name = "Email" )]
            public string Email { get; set; }

            [Required]
            [StringLength( 100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6 )]
            [DataType( DataType.Password )]
            [Display( Name = "Password" )]
            public string Password { get; set; }

            [DataType( DataType.Password )]
            [Display( Name = "Confirm password" )]
            [Compare( "Password", ErrorMessage = "The password and confirmation password do not match." )]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync( string returnUrl = null )
        {
            ReturnUrl = returnUrl;

            ExternalLogins = ( await _signInManager.GetExternalAuthenticationSchemesAsync() ).ToList();
        }

        public async Task<IActionResult> OnPostAsync( string returnUrl = null )
        {
            returnUrl ??= Url.Content( "~/" );
            ExternalLogins =  ( await _signInManager.GetExternalAuthenticationSchemesAsync() ).ToList();
            if ( ModelState.IsValid )
            {

                var user = CreateUser();

                await _userStore.SetUserNameAsync( user, Input.Username, CancellationToken.None );
                await _emailStore.SetEmailAsync( user, Input.Email, CancellationToken.None );

                var result = await _userManager.CreateAsync( user, Input.Password );

                if (result.Succeeded)
                {
                    _logger.LogInformation( "User created a new account with password." );

                    var userId = await _userManager.GetUserIdAsync( user );
                    var tokenPregenerated = await _userManager.GenerateEmailConfirmationTokenAsync( user );
                    var tokenEncoded = WebEncoders.Base64UrlEncode( Encoding.UTF8.GetBytes( tokenPregenerated ) );
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = tokenEncoded, returnUrl = returnUrl },
                        protocol: Request.Scheme);
                    
                    //The confirmed account is mandatory with present implementation.
                    await _confirmationSender.SendConfirmationAsync( Input.Email, callbackUrl );

                    return RedirectToPage( "RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl } );
                }
                foreach ( var error in result.Errors )
                {
                    ModelState.AddModelError( string.Empty, error.Description );
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ChatUser CreateUser()
        {
            try
            {
                return new ChatUser();
            }
            catch
            {
                throw new InvalidOperationException( $"Can't create an instance of '{ nameof(ChatUser) }'. " +
                    $"Ensure that '{ nameof(ChatUser) }' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml" );
            }
        }

        private IUserEmailStore<ChatUser> GetEmailStore()
        {
            if ( !_userManager.SupportsUserEmail )
            {
                throw new NotSupportedException( "The default UI requires a user store with email support." );
            }
            return ( IUserEmailStore<ChatUser> )_userStore;
        }
    }
}
