using Application.Interfaces.User;
using DomainModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Implementations.User
{
    internal class IdentityUserRepository : IUserRepository
    {
        private readonly UserManager<ChatUser> _userManager;
        public IdentityUserRepository( UserManager<ChatUser> userManager ) 
        {
            _userManager = userManager;
        }

        public async Task<ChatUser?> GetByPrincipal( ClaimsPrincipal principal )
        {
            return await _userManager.GetUserAsync( principal );
        }
    }
}
