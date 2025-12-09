using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CSCoreEFTemplate8.Helpers;
using CSCoreEFTemplate8.ViewModels;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.Database;
using ConfidoSoft.Data.Domain.Consts;
using CSCoreEFTemplate8.Services;
using Microsoft.AspNetCore.Authorization;
using ConfidoSoft.Data.Services.BLServices;
using System.Net;
using ConfidoSoft.Data.Services.DBServices;
using ConfidoSoft.Data.Domain.Dtos;
using ConfidoSoft.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;

namespace CSCoreEFTemplate8.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/Account")]
    public class AccountController : Controller
    {
        private readonly ILogger _logger;
        private readonly ReCaptchaService _reCaptchaService;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<User> userManager, 
            ReCaptchaService reCaptchaService,
            IUserService userService,
            IConfiguration configuration,
            ILogger<AccountController> logger)
        {
            _reCaptchaService = reCaptchaService;
            _logger = logger;
            _configuration = configuration;
            this._userService = userService;
        }

        #region Change password and request access functions

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest viewModel)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the ForgotPassword action, {viewModel.Email}");
                }

                if (this.ModelState.IsValid)
                {
                    var isValid = await this._reCaptchaService.Validate(viewModel.SecretCode);
                    if (!isValid)
                    {
                        return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_ACCESSREQUEST_FAILED, _logger);
                    }
                    var sucess = await this._userService.ForgotPassword(viewModel);
                    return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD, sucess);
                }
                else
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD_FAILED, ModelState, _logger);
                }
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD_FAILED, ex, _logger);
            }
        }

        [HttpPost("mobile-forgot-password")]
        public async Task<IActionResult> MobileForgotPassword([FromBody] MobileForgotPasswordRequest viewModel)
        {
            try
            {
                if (this.ModelState.IsValid)
                {
                    if (this._logger.IsInformationEnabled())
                    {
                        _logger.LogInformation($"Calling the MobileForgotPassword action, {viewModel.Code}");
                    }

                    var dataEncryptionService = new DataEncryptionService(DataEncryptionService.AppKey, DataEncryptionService.AppIV);
                    var email = dataEncryptionService.Decrypt(viewModel.Code);
                    ForgotPasswordRequest forgotPassword = new ForgotPasswordRequest
                    {
                        Email = email,
                        ReturnUrl = this._configuration["WebApplicationUrl"] + "auth/set-password"
                    };
                    var sucess = await this._userService.ForgotPassword(forgotPassword);
                    return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD, sucess);
                }
                else
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD_FAILED, ModelState, _logger);
                }
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD_FAILED, ex, _logger);
            }
        }

        [HttpGet("mobile-forgot-password-code/{email}")]
        public IActionResult MobileForgotPasswordCode(String email)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the MobileForgotPasswordCode action, {email}");
                }

                if (this.ModelState.IsValid)
                {
                    var dataEncryptionService = new DataEncryptionService(DataEncryptionService.AppKey, DataEncryptionService.AppIV);
                    var code = dataEncryptionService.Encrypt(email);
                    return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD, code);
                }
                else
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD_FAILED, ModelState, _logger);
                }
            }
            catch (Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_FORGOT_PASSWORD_FAILED, ex, _logger);
            }
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the ResetPassword action, {model.Email}");
                }
                if (this.ModelState.IsValid)
                {
                    var sucess = await this._userService.ResetPassword(model);
                    return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_RESET_PASSWORD, sucess);
                }
                else
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_CHANGE_PASSWORD_FAILED, ModelState, _logger);
                }
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_CHANGE_PASSWORD_FAILED, ex, _logger);
            }
        }
        
        [HttpPost("request-access")]
        public async Task<IActionResult> RequestAccess([FromBody] RequestAccessDto model)
        {
            try
            {
                if (this._logger.IsInformationEnabled())
                {
                    _logger.LogInformation($"Calling the RequestAccess action, {model.Email}");
                }

                if (this.ModelState.IsValid)
                {
                    var isValid = await this._reCaptchaService.Validate(model.SecretCode);
                    if (!isValid)
                    {
                        return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_ACCESSREQUEST_FAILED, _logger);
                    }
                    var sucess = await this._userService.RequestAccess(model);
                    return this.OkResponse(EnumEntityType.USER, EnumEntityEvents.USER_ACCESSREQUEST_SUCCESS, sucess);
                }
                else
                {
                    return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_ACCESSREQUEST_FAILED, ModelState, _logger);
                }
            }
            catch(Exception ex)
            {
                return this.CreateBadRequest(EnumEntityType.USER, EnumEntityEvents.USER_ACCESSREQUEST_FAILED, ex, _logger);
            }
        }

        #endregion
    }
}