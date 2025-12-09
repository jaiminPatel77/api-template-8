using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace CSCoreEFTemplate8.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/ExternalAuth")]
    public class ExternalAuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly FacebookAuthSettings _fbAuthSettings;
        private readonly IAuthService _authService;
        private readonly ILogger _logger;
        private readonly AzureAdAuthSettings _azureAdAuthSettings;
        private static readonly HttpClient Client = new HttpClient();

        public ExternalAuthController(IOptions<FacebookAuthSettings> fbAuthSettingsAccessor,
            IOptions<AzureAdAuthSettings> azureAdAuthSettingsAccessor,
            UserManager<User> userManager,
            IAuthService authService,
            IOptions<JwtIssuerOptions> jwtOptions,
            ILogger<ExternalAuthController> logger)
        {
            _fbAuthSettings = fbAuthSettingsAccessor.Value;
            _azureAdAuthSettings = azureAdAuthSettingsAccessor.Value;
            _userManager = userManager;
            _authService = authService;
            _logger = logger;
        }

        // POST api/externalauth/facebook
        [HttpPost("facebook")]
        public async Task<IActionResult> Facebook([FromBody]FacebookAuthViewModel model)
        {
            _logger.LogDebug("Facebook called");

            // 1.generate an app access token
            var appAccessTokenResponse = await Client.GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={_fbAuthSettings.AppId}&client_secret={_fbAuthSettings.AppSecret}&grant_type=client_credentials");
            var appAccessToken = JsonConvert.DeserializeObject<FacebookAppAccessToken>(appAccessTokenResponse);
            // 2. validate the user access token
            var userAccessTokenValidationResponse = await Client.GetStringAsync($"https://graph.facebook.com/debug_token?input_token={model.AccessToken}&access_token={appAccessToken.AccessToken}");
            var userAccessTokenValidation = JsonConvert.DeserializeObject<FacebookUserAccessTokenValidation>(userAccessTokenValidationResponse);

            if (!userAccessTokenValidation.Data.IsValid)
            {
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid facebook token.", ModelState));
            }

            // 3. we've got a valid token so we can request user data from fb
            var userInfoResponse = await Client.GetStringAsync($"https://graph.facebook.com/v2.8/me?fields=id,email,first_name,last_name,name,gender,locale,birthday,picture&access_token={model.AccessToken}");
            var userInfo = JsonConvert.DeserializeObject<FacebookUserData>(userInfoResponse);

            // 4. ready to create the local user account (if necessary) and jwt
            var user = await _userManager.FindByEmailAsync(userInfo.Email);

            if (user == null)
            {
                var appUser = new User
                {
                    Email = userInfo.Email,
                    UserName = userInfo.Email,
                    Title = userInfo.Name,
                    //PictureUrl = userInfo.Picture.Data.Url
                };

                var result = await _userManager.CreateAsync(appUser, Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8));
                if (!result.Succeeded) return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));
            }

            // generate the jwt for the local user...
            var localUser = await _userManager.FindByNameAsync(userInfo.Email);
            if (localUser == null)
            {
                return BadRequest(Errors.AddErrorToModelState("login_failure", "Failed to create local user account.", ModelState));
            }
            var retVal = await _authService.CreateJwtTokenAsync(localUser, model);

            return new OkObjectResult(retVal);
        }

        [HttpPost("azuread")]
        public async Task<IActionResult> AzureAd([FromBody]ExternalLoginViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN_FAILED, ModelState, _logger);
                }

                await ValidateAzureAdToken(viewModel);

                bool isProviderLoginExists = false;
                var localUser = await _userManager.FindByLoginAsync(viewModel.LoginProvider, viewModel.ProviderKey);
                if (localUser == null)
                {
                    localUser = await _userManager.FindByEmailAsync(viewModel.Email);
                }
                else
                {
                    isProviderLoginExists = true;
                }

                if (localUser == null)
                {
                    localUser = new User
                    {
                        Email = viewModel.Email,
                        UserName = viewModel.Email,
                        FullName = viewModel.FullName,
                    };
                    var result1 = await this._userManager.CreateAsync(localUser);
                    if (!result1.Succeeded)
                    {
                        Errors.AddErrorsToModelState(result1, ModelState);
                        return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN_FAILED, ModelState, _logger);
                    }
                }
                if (isProviderLoginExists == false)
                {
                    UserLoginInfo userLoginInfo = new UserLoginInfo(viewModel.LoginProvider, viewModel.ProviderKey, viewModel.FullName);
                    var result = await this._userManager.AddLoginAsync(localUser, userLoginInfo);
                    if (!result.Succeeded)
                    {
                        Errors.AddErrorsToModelState(result, ModelState);
                        return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN_FAILED, ModelState, _logger);
                    }
                }
                var jwtResponse = await _authService.CreateJwtTokenAsync(localUser, viewModel);
                return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN, jwtResponse);
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_LOGIN_FAILED, ex, _logger);
            }
        }

        private async Task ValidateAzureAdToken(ExternalLoginViewModel viewModel)
        {
            //https://github.com/Azure-Samples/active-directory-dotnet-webapi-manual-jwt-validation/blob/master/TodoListService-ManualJwt/Global.asax.cs
            // Get tenant information that's used to validate incoming jwt tokens
            string stsDiscoveryEndpoint = string.Format("{0}/.well-known/openid-configuration", _azureAdAuthSettings.Authority);
            OpenIdConnectConfigurationRetriever openIdConnectConfigurationRetriever = new OpenIdConnectConfigurationRetriever();
            ConfigurationManager<OpenIdConnectConfiguration> configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, openIdConnectConfigurationRetriever);
            OpenIdConnectConfiguration config = await configManager.GetConfigurationAsync();
            //var _issuer = config.Issuer;
            //var _stsMetadataRetrievalTime = DateTime.UtcNow;


            // Setup handler for processing Jwt token
            var tokenHandler = new JwtSecurityTokenHandler();

            // Setup token checking
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { config.Issuer, $"{config.Issuer}/v2.0" },

                ValidateAudience = false,
                //ValidAudience = _jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = config.SigningKeys,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            SecurityToken validatedToken = null;

            var principal = tokenHandler.ValidateToken(viewModel.Token, tokenValidationParameters, out validatedToken);

            // cast needed to access Claims property
            var securityToken = validatedToken as JwtSecurityToken;
            if (securityToken == null)
            {
                throw new Exception("Invalid access token");
            }
        }

    }
}