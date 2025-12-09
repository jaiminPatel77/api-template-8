using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Services.BLServices;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CSCoreEFTemplate8.Services
{
    /// <summary>
    /// https://dotnetcoretutorials.com/2017/01/05/accessing-httpcontext-asp-net-core/
    /// </summary>
    public class CurrentUser : ICurrentUserInfo
    {
        public const string RoleIdStr = "RoleId";
        public const string UserTypeStr = "UserType";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            var userIdentity = (ClaimsIdentity)_httpContextAccessor.HttpContext?.User?.Identity;

            if (userIdentity != null)
            {
                this.IsUserLoggedIn = userIdentity.IsAuthenticated;
                if (userIdentity.Claims != null)
                {
                    foreach (Claim claim in userIdentity.Claims)
                    {
                        
                        if ((claim.Type == ClaimTypes.NameIdentifier) || (claim.Type == JwtRegisteredClaimNames.Sub))
                        {
                            this.FullName = claim.Value;
                        }
                        else if ((claim.Type == JwtRegisteredClaimNames.Sid) || (claim.Type == ClaimTypes.Sid))
                        {
                            long userId = 0;
                            if(long.TryParse(claim.Value, out userId))
                            {
                                this.Id = userId;
                            }                            
                        }
                        else if ((claim.Type == JwtRegisteredClaimNames.Email) || (claim.Type == ClaimTypes.Email))
                        {
                            this.Email = claim.Value;
                        }
                        else if ((claim.Type == UserTypeStr))
                        {
                            //Set the role type..
                            EnumUserType userTypeVal = EnumUserType.CustomRoleBase;                            
                            if(Enum.TryParse<EnumUserType>(claim.Value, out userTypeVal))
                            {
                                this.UserType = userTypeVal;
                            }
                        }
                        else if ((claim.Type == RoleIdStr))
                        {
                            //Set the role type..
                            long roleId = 0;
                            long.TryParse(claim.Value, out roleId);
                            if (roleId > 0)
                            {
                                this.RoleIds.Add(roleId);
                            }
                        }
                    }
                }
            }
        }

        public string Email { get; private set; }

        public string FullName { get; private set; }

        public long? Id { get; private set; }

        public String UserId
        {
            get
            {
                if(this.Id != null)
                {
                    return this.Id.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool IsUserLoggedIn { get; private set; }
        public EnumUserType UserType { get; private set; }
        public List<long> RoleIds { get; } = new List<long>();
    }
}
