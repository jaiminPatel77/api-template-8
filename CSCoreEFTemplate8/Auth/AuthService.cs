using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CSCoreEFTemplate8.AppSettings;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Services.DBServices;
using System.Security.Cryptography;
using CSCoreEFTemplate8.ViewModels;
using ConfidoSoft.Data.Domain.Consts;
using CSCoreEFTemplate8.Services;

namespace CSCoreEFTemplate8.Auth
{
    #region IAuthService interface

    public interface IAuthService
    {
        Task<JwtResponse> CreateJwtTokenAsync(User user, LoginBaseModel loginViewModel);
        Task<JwtResponse> RenewToken(String accessToken, String refreshToken);
        Task Logout(String accessToken, String refreshToken);
    }
    #endregion

    #region IAuthService implemenation

    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly JwtIssuerOptions _jwtOptions;
        //private readonly RoleManager<Role> _roleManager;
        private readonly IRoleService _roleService;
        private readonly IUserRefreshTokenService _userRefreshTokenService;
        private readonly ILogger _logger;

        public AuthService(UserManager<User> userManager,
            //RoleManager<Role> roleManager,
            IOptions<JwtIssuerOptions> jwtOptions,
            IUserRefreshTokenService userRefreshTokenService,
            IRoleService roleService,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
            //_roleManager = roleManager;
            _userRefreshTokenService = userRefreshTokenService;
            _logger = logger;
            _roleService = roleService;
        }

        #region IAuthService implemenation

        /// <summary>
        /// Create JWT token for a user and set RefreshToken baseed on LoginBaseModel information.
        /// </summary>
        /// <param name="user">application user</param>
        /// <param name="loginViewModel">client information and rememer me flag!</param>
        /// <returns></returns>
        public async Task<JwtResponse> CreateJwtTokenAsync(User user, LoginBaseModel loginViewModel)
        {
            await this._userRefreshTokenService.RemoveExpiredTokens(user.Id, loginViewModel.ClientType);
            var createToken = await CreateJwtToken(user);
            UserRefreshToken record = new UserRefreshToken()
            {
                ClientType = loginViewModel.ClientType,
                DeviceId = loginViewModel.DeviceId,
                UserId = user.Id,
                RefreshToken = createToken.refresh_token
            };
            if(false == loginViewModel.RememberMe)
            {
                //let see what will be the best default
                //record.ValidTill = DateTime.UtcNow.AddHours(_jwtOptions.RefreshTokenValidHrs);                
                record.ValidTill = DateTime.UtcNow.AddMinutes(_jwtOptions.ValidFor.TotalMinutes + 1);

            }            
            await this._userRefreshTokenService.CreateRefreshToken(record);
            return createToken;
        }

        /// <summary>
        /// Renew token from old accessToken and refreshToken value. 
        /// </summary>
        /// <param name="accessToken"> existing access token</param>
        /// <param name="refreshToken"> associated refresh token value</param>
        /// <returns>new access token value</returns>
        public async Task<JwtResponse> RenewToken(String accessToken, String refreshToken)
        {
            var existingToken = await this._userRefreshTokenService.GetRefreshToken(refreshToken);
            if(existingToken != null)
            {
                DateTime? validTill = null;
                if(existingToken.ValidTill != null)
                {
                    //validTill = DateTime.UtcNow.AddHours(_jwtOptions.RefreshTokenValidHrs);
                    validTill = DateTime.UtcNow.AddMinutes(_jwtOptions.ValidFor.TotalMinutes+1);
                }
                var principal = GetPrincipalFromExpiredToken(accessToken);
                if (principal != null)
                {
                    var userId = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sid).Value;
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var jwtToken = await CreateJwtToken(user);
                        await this._userRefreshTokenService.RefreshToken(existingToken, jwtToken.refresh_token, validTill);
                        return jwtToken;
                    }
                }
            }
            throw new Exception("Invalid Token");
        }

        /// <summary>
        /// Remove the refreshToken if authToken has valid principal..
        /// </summary>
        /// <param name="accessToken">access token</param>
        /// <param name="refreshToken">associated refresh token</param>
        /// <returns></returns>
        public async Task Logout(String accessToken, String refreshToken)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(accessToken);
                if (principal != null)
                {
                    var userId = principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sid).Value;
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await this._userRefreshTokenService.RemoveRefreshToken(refreshToken);
                    }
                }
            }
            catch(Exception ex)
            {
                this._logger.LogError(ex.Message);
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Create new token for given user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<JwtResponse> CreateJwtToken(User user)
        {
            // Create JWT claims
            var claims = new List<Claim>(new[]
            {
                // Issuer
                new Claim(JwtRegisteredClaimNames.Iss, _jwtOptions.Issuer),                

                //UniqueName is same as user name!
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName), 
                new Claim(JwtRegisteredClaimNames.Sub, user.FullName),

                // Sid as user PK!
                new Claim(JwtRegisteredClaimNames.Sid, user.Id.ToString()),   

                // Email is unique
                new Claim(JwtRegisteredClaimNames.Email, user.Email),        

                // Unique Id for all Jwt tokes
                new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()),
                // Issued at
                new Claim(JwtRegisteredClaimNames.Iat, JwtIssuerOptions.ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64)
              });

            //if (!string.IsNullOrEmpty(user.DisplayName))
            //{
            //    // DisplayName                
            //    claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, user.DisplayName));
            //}

            // Add userclaims from storage
            var userClaimslist = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaimslist);

            //Add role ids as claims
            EnumUserType userType = user.UserType;
            var userRoles = await this._roleService.GetUserRoles(user.Id);
            //May be we should avoid for Admin user!
            foreach( var role in userRoles)
            {
                var roleClaim = new Claim(CurrentUser.RoleIdStr, role.Id.ToString());
                claims.Add(roleClaim);
            }
            //added specific role type if any!
            if (userType != EnumUserType.CustomRoleBase)
            {
                var adminClaim = new Claim(CurrentUser.UserTypeStr, userType.ToString());
                claims.Add(adminClaim);
            }

            //// Add user role, they are converted to claims
            ////For now Removed Role from Claims as we have custom role validation in templete!
            //var roleNames = await _userManager.GetRolesAsync(user);
            // foreach (var roleName in roleNames)
            // {
            //     // Find IdentityRole by name
            //     var role = await _roleManager.FindByNameAsync(roleName);
            //     if (role != null)
            //     {
            //         // Convert Identity to claim and add 
            //         var roleClaim = new Claim(ClaimTypes.Role, role.Name, ClaimValueTypes.String, _jwtOptions.Issuer);
            //         claims.Add(roleClaim);

            //         // Add claims belonging to the role
            //         //var roleClaims = await _roleManager.GetClaimsAsync(role);
            //         //claims.AddRange(roleClaims);
            //     }
            // }

            // Prepare Jwt Token
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            // Serialize token
            var auth_token = new JwtSecurityTokenHandler().WriteToken(jwt);
            var response = new JwtResponse
            {
                id = user.Id.ToString(),
                access_token = auth_token,
                expiration = JwtIssuerOptions.ToUnixEpochDate(_jwtOptions.Expiration),
                expires_in = (int)_jwtOptions.ValidFor.TotalSeconds,
                refresh_token = GenerateRefreshToken()
            };
            return response;
        }

        /// <summary>
        /// Get Principal from Old token to see if we allow to refresh it or not.
        /// </summary>
        /// <param name="token">existing token value</param>
        /// <returns></returns>
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _jwtOptions.SigningCredentials.Key,
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }


        /// <summary>
        /// Get random refresh token value!
        /// </summary>
        /// <returns></returns>
        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString("N").ToUpperInvariant();
            //var randomNumber = new byte[32];
            //using (var rng = RandomNumberGenerator.Create())
            //{
            //    rng.GetBytes(randomNumber);                
            //}
            //return Convert.ToBase64String(randomNumber);
        }

        #endregion

    }
    #endregion

}
