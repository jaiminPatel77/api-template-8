using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using CSCoreEFTemplate8.AppSettings;
using CSCoreEFTemplate8.Auth;
using CSCoreEFTemplate8.Helpers;
using CSCoreEFTemplate8.ViewModels;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Infrastructure.Extensions;

namespace CSCoreEFTemplate8.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/Auth")]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IAuthService _authService;
        private readonly ILogger _logger;

        public AuthController(UserManager<User> userManager, 
            SignInManager<User> signInManager, 
            RoleManager<Role> roleManager,
            IAuthService authService,
            IOptions<JwtIssuerOptions> jwtOptions,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
            _logger = logger;
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginViewModel model)
        {
            try
            {
                
                if (!ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN_FAILED, ModelState, _logger);
                }
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the Login action, {model.Email}");
                }
                JwtResponse jwtResponse = null;

                var userRecord = await _userManager.FindByEmailAsync(model.Email);
                if(userRecord != null )
                {
                    if (userRecord.Status == EnumUserStatus.Invited)
                    {
                        return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN_FAILED_INVITATION, _logger);
                    }
                    else if (userRecord.Status == EnumUserStatus.AdminResetPassword)
                    {                        
                        return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN_FAILED_RESET, _logger);
                    }
                }
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    //Login passed handing here..
                    jwtResponse = await _authService.CreateJwtTokenAsync(userRecord, model);
                    await _signInManager.SignOutAsync(); // as not token is generated!
                    return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN, jwtResponse);
                }
                else
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN_FAILED, _logger);
                }
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN_FAILED, ex, _logger);
            }
        }

        /// <summary>
        /// Renew Token for existing authToken and RefreshToken value! 
        /// Return new token if associated refeshToken id is valid and not expired.
        /// </summary>
        /// <param name="jwtInfo">exising jwt token values</param>
        /// <returns></returns>
        [HttpPost("renew-token")]
        public async Task<IActionResult> RenewToken([FromBody] JwtResponse jwtInfo)
        {            
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation("Calling the RenewToken action");
                }
                var result = await _authService.RenewToken(jwtInfo.access_token, jwtInfo.refresh_token);
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_RENEW_TOKEN, result);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_RENEW_TOKEN_FAILED, ex, _logger);
            }
        }

        /// <summary>
        /// logout user associated with given JwtInfo.
        /// </summary>
        /// <param name="jwtInfo">existing jwt token values</param>
        /// <returns></returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] JwtInfo jwtInfo)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation("Calling the Logout action");
                }
                await _authService.Logout(jwtInfo.access_token, jwtInfo.refresh_token);
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_LOGOUT_SUCCESS, true);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGOUT_FAILED, ex, _logger);
            }
        }
    }
}