using ConfidoSoft.Data.Domain.Consts;
using ConfidoSoft.Data.Domain.Database;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Domain.DBModels.Settings;
using ConfidoSoft.Data.Domain.Dtos;
using ConfidoSoft.Data.Services.DBServices;
using ConfidoSoft.Infrastructure.Extensions;
using ConfidoSoft.Infrastructure.Models;
using System.Net.Mail;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mime;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace ConfidoSoft.Data.Services.BLServices
{

    #region IEmailSender interface

    /// <summary>
    /// Email sender service interface!
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Send Invitation Email to given user with link to callback URL.
        /// </summary>
        /// <param name="toUser">Email to be send to user.</param>
        /// <param name="fromUser">Invitation send from User</param>
        /// <param name="callbackUrl">callback URL for Invited user to access application.</param>
        /// <returns></returns>
        Task SendUserInvitationEmailAsync(User toUser, User fromUser, string callbackUrl);

        /// <summary>
        /// Send reset password request email to User from Administrator.
        /// </summary>
        /// <param name="toUser">User to which reset password request email to be sent.</param>
        /// <param name="fromUser">Password reset by user.</param>
        /// <param name="callbackUrl">Callback URL where user can set his new password.</param>
        /// <returns></returns>
        Task SendUserAdminResetPasswordEmailAsync(User toUser, User fromUser, string callbackUrl);

        /// <summary>
        /// Send reset password request email to user as user forgot his password.
        /// </summary>
        /// <param name="toUser">User for which reset password request email to be sent.</param>
        /// <param name="callbackUrl">Callback URL where user can set his new password.</param>
        /// <returns></returns>
        Task SendUserResetPasswordEmailAsync(User toUser, string callbackUrl);

        /// <summary>
        /// Send Authentication code email for 2-factor login if enabled.
        /// </summary>
        /// <param name="toUser">User to send the code.</param>
        /// <param name="code">Authentication code.</param>
        /// <returns></returns>
        Task SendAuthenticationCodeEmailAsync(User toUser, string code);

        /// <summary>
        /// Send change email address verification email to user.
        /// </summary>
        /// <param name="user"> User of which email to be changed.</param>
        /// <param name="newEmail"> New email address of user.</param>
        /// <param name="code">Verification code to allow change email.</param>
        /// <returns></returns>
        Task SendChangeEmailAddressEmailAsync(User user, string newEmail, string code);

        /// <summary>
        /// Send access request email to administrator from new user.
        /// </summary>
        /// <param name="model"> User information and reason for need access to application.</param>
        /// <returns></returns>
        Task SendAccessRequestEmailAsync(RequestAccessDto model);

        /// <summary>
        /// Send ad-hock email to given email address using given subject and text message. 
        /// </summary>
        /// <param name="email">email address where email to be sent.</param>
        /// <param name="subject">subject of email</param>
        /// <param name="textMessage">text message for email</param>
        /// <returns></returns>
        Task SendEmailAsync(string email, string subject, string textMessage);

        /// <summary>
        /// Send alert email to given user.
        /// </summary>
        /// <param name="toUser">User to which email need to be sent.</param>
        /// <param name="subject">subject of email</param>
        /// <param name="message">Alert message to be set in email.</param>
        /// <returns></returns>
        Task SendAlertEmailAsync(User toUser, string subject, string message);
    }

    #endregion

    #region IEmailSender implementation

    /// <summary>
    /// Email message sender service.
    /// </summary>
    public class EmailMessageService : IEmailSender
    {
        #region members and static values
        public const string AppLogoLink = "AppLogo";
        public const string AppTitleKey = "${appTitle}";
        public const string UserFullNameKey = "${userFullName}";
        public const string FromUserFullNameKey = "${fromUserFullName}";
        public const string CallbackUrlKey = "${callbackUrl}";
        public const string FromUserEmailKey = "${fromUserEmail}";
        public const string AppTitle = "CSCoreEFTemplate8 Application";

        private readonly ILogger _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly ISettingService _settingService;
        private readonly IHostEnvironment _hostingEnvironment;
        private SMTPSetting _smtpSetting;
        private string _webRootPath;

        #endregion

        #region ctor

        public EmailMessageService(ILogger<EmailMessageService> logger,
                   ApplicationDbContext dbContext,
                   IHostEnvironment hostingEnvironment,
                   ISettingService settingService)
        {
            this._logger = logger;
            //this._configuration = configuration;
            this._dbContext = dbContext;
            this._settingService = settingService;
            this._hostingEnvironment = hostingEnvironment;
        }

        #endregion
        
        #region Get root www path
        private string WebRootPath
        {
            get
            {
                if (this._webRootPath == null)
                {
                    this._webRootPath = Path.Combine(this._hostingEnvironment.ContentRootPath, "wwwroot");
                }
                return this._webRootPath;
            }
        }
        #endregion
        
        #region User authentication related emails


        /// <summary>
        /// Send Invitation Email to given user with link to callback URL.
        /// </summary>
        /// <param name="toUser">Email to be send to user.</param>
        /// <param name="fromUser">Invitation send from User</param>
        /// <param name="callbackUrl">callback URL for Invited user to access application.</param>
        /// <returns></returns>
        public async Task SendUserInvitationEmailAsync(User toUser, User fromUser, string callbackUrl)
        {
            if (this._logger.IsInformationEnabled())
            {
                this._logger.LogInformation($"Send Invitation email to {toUser.Email}");
            }
            await this.ValidateEmailSetting();
            string emailTemplate = null;
            emailTemplate = Path.Combine(this.WebRootPath, "EmailTemplate/user-invitation.html");


            string appLogoImage = Path.Combine(this.WebRootPath, "EmailTemplate/AppLogo.png");
            string htmlMessage = await FileUtile.ReadAllTextAsync(emailTemplate);
            string userFullName = toUser.FullName;
            string fromUserFullName = fromUser.FullName;
            string subject = $"{fromUserFullName} invited you to {AppTitle}";


            string fromUserEmail = String.Empty;
            if (false == String.IsNullOrWhiteSpace(this._smtpSetting.ServerAddress))
            {
                fromUserEmail = this._smtpSetting.FromEmail;
            }

            //Format the Dynamic Values
            Dictionary<String, String> messageBodyKeyValue = new Dictionary<string, string>();
            messageBodyKeyValue.Add(UserFullNameKey, userFullName);
            messageBodyKeyValue.Add(FromUserFullNameKey, fromUserFullName);
            messageBodyKeyValue.Add(CallbackUrlKey, callbackUrl);
            messageBodyKeyValue.Add(FromUserEmailKey, fromUserEmail);
            messageBodyKeyValue.Add(AppTitleKey, AppTitle);


            foreach (var keyValue in messageBodyKeyValue)
            {
                htmlMessage = htmlMessage.Replace(keyValue.Key, keyValue.Value);
            }


            //send using send grid or normally
            await SendGridEmailAsync(toUser.Email, subject, htmlMessage, true, userFullName);
        }


        /// <summary>
        /// Send reset password request email to User from Administrator.
        /// </summary>
        /// <param name="toUser">User to which reset password request email to be sent.</param>
        /// <param name="fromUser">Password reset by user.</param>
        /// <param name="callbackUrl">Callback URL where user can set his new password.</param>
        /// <returns></returns>
        public async Task SendUserAdminResetPasswordEmailAsync(User toUser, User fromUser, string callbackUrl)
        {
            if (this._logger.IsInformationEnabled())
            {
                this._logger.LogInformation($"Sending Admin Reset password email to {toUser.Email}");
            }
            await this.ValidateEmailSetting();
            string emailTemplate = Path.Combine(this.WebRootPath, "EmailTemplate/user-resetpassword.html");
            string appLogoImage = Path.Combine(this.WebRootPath, "EmailTemplate/AppLogo.png");
            string htmlMessage = await FileUtile.ReadAllTextAsync(emailTemplate);
            string userFullName = toUser.FullName;
            //string fromUserFullName = $"{fromUser.FirstName} {fromUser.LastName}";
            string subject = $"Request to reset your password for your {AppTitle} account";

            //Format the Dynamic Values
            Dictionary<String, String> messageBodyKeyValue = new Dictionary<string, string>();
            messageBodyKeyValue.Add(UserFullNameKey, userFullName);
            messageBodyKeyValue.Add(CallbackUrlKey, callbackUrl);
            messageBodyKeyValue.Add(FromUserEmailKey, fromUser.Email);
            messageBodyKeyValue.Add(AppTitleKey, AppTitle);

            foreach (var keyValue in messageBodyKeyValue)
            {
                htmlMessage = htmlMessage.Replace(keyValue.Key, keyValue.Value);
            }


            //send using send grid or normally
            await SendGridEmailAsync(toUser.Email, subject, htmlMessage, true, userFullName);

        }

        /// <summary>
        /// Send reset password request email to user as user forgot his password.
        /// </summary>
        /// <param name="toUser">User for which reset password request email to be sent.</param>
        /// <param name="callbackUrl">Callback URL where user can set his new password.</param>
        /// <returns></returns>
        public async Task SendUserResetPasswordEmailAsync(User toUser, string callbackUrl)
        {
            if (this._logger.IsInformationEnabled())
            {
                this._logger.LogInformation($"Send Reset password email to {toUser.Email}");
            }
            await this.ValidateEmailSetting();
            string emailTemplate = Path.Combine(this.WebRootPath, "EmailTemplate/user-resetpassword.html");
            string appLogoImage = Path.Combine(this.WebRootPath, "EmailTemplate/AppLogo.png");
            string htmlMessage = await FileUtile.ReadAllTextAsync(emailTemplate);
            string userFullName = toUser.FullName;            
            string subject = $"Request to reset your password for your {AppTitle} account";
            string fromUserEmail = String.Empty;
            if (false == String.IsNullOrWhiteSpace(this._smtpSetting.ServerAddress))
            {
                fromUserEmail = this._smtpSetting.FromEmail;
            }

            //Format the Dynamlic Values
            Dictionary<String, String> messageBodyKeyValue = new Dictionary<string, string>();
            messageBodyKeyValue.Add(UserFullNameKey, userFullName);
            //messageBodyKeyValue.Add(EmailSMSAlertStringConstants.FromUserFullNameKey, fromUserFullName);
            messageBodyKeyValue.Add(CallbackUrlKey, callbackUrl);
            messageBodyKeyValue.Add(FromUserEmailKey, fromUserEmail);
            messageBodyKeyValue.Add(AppTitleKey, AppTitle);

            foreach (var keyValue in messageBodyKeyValue)
            {
                htmlMessage = htmlMessage.Replace(keyValue.Key, keyValue.Value);
            }


            //send using send grid or normally
            await SendGridEmailAsync(toUser.Email, subject, htmlMessage, true, userFullName);

        }

        /// <summary>
        /// Send Authentication code email for 2-factor login if enabled.
        /// </summary>
        /// <param name="toUser">User to send the code.</param>
        /// <param name="code">Authentication code.</param>
        /// <returns></returns>
        public async Task SendAuthenticationCodeEmailAsync(User toUser, string code)
        {
            if (this._logger.IsInformationEnabled())
            {
                this._logger.LogInformation($"Send Authentication code email to {toUser.Email}");
            }
            await this.ValidateEmailSetting();
            string emailTemplate = Path.Combine(this.WebRootPath, "EmailTemplate/user-authentication-code.html");
            string appLogoImage = Path.Combine(this.WebRootPath, "EmailTemplate/AppLogo.png");
            string htmlMessage = await FileUtile.ReadAllTextAsync(emailTemplate);
            string userFullName = toUser.FullName;            
            string subject = $"Authentication code for your {AppTitle} account login";
            string fromUserEmail = String.Empty;
            if (false == String.IsNullOrWhiteSpace(this._smtpSetting.ServerAddress))
            {
                fromUserEmail = this._smtpSetting.FromEmail;
            }

            //Format the Dynamlic Values
            Dictionary<String, String> messageBodyKeyValue = new Dictionary<string, string>();
            messageBodyKeyValue.Add(UserFullNameKey, userFullName);
            messageBodyKeyValue.Add("${code}", code);
            messageBodyKeyValue.Add(FromUserEmailKey, fromUserEmail);
            messageBodyKeyValue.Add(AppTitleKey, AppTitle);

            foreach (var keyValue in messageBodyKeyValue)
            {
                htmlMessage = htmlMessage.Replace(keyValue.Key, keyValue.Value);
            }

            //send using send grid or normally
            await SendGridEmailAsync(toUser.Email, subject, htmlMessage, true, userFullName);


        }

        /// <summary>
        /// Send change email address verification email to user.
        /// </summary>
        /// <param name="user"> User of which email to be changed.</param>
        /// <param name="newEmail"> New email address of user.</param>
        /// <param name="code">Verification code to allow change email.</param>
        /// <returns></returns>
        public async Task SendChangeEmailAddressEmailAsync(User toUser, string newEmail, string code)
        {
            if (this._logger.IsInformationEnabled())
            {
                this._logger.LogInformation($"Send Change email address of {toUser.FullName} from {toUser.Email} to {newEmail}");
            }
            await this.ValidateEmailSetting();
            string emailTemplate = Path.Combine(this.WebRootPath, "EmailTemplate/user-change-email.html");
            string appLogoImage = Path.Combine(this.WebRootPath, "EmailTemplate/AppLogo.png");
            string htmlMessage = await FileUtile.ReadAllTextAsync(emailTemplate);
            string userFullName = toUser.FullName;            
            string subject = $"Change email address request verification for your {AppTitle} account";
            string fromUserEmail = String.Empty;
            if (false == String.IsNullOrWhiteSpace(this._smtpSetting.ServerAddress))
            {
                fromUserEmail = this._smtpSetting.FromEmail;
            }

            //Format the Dynamlic Values
            Dictionary<String, String> messageBodyKeyValue = new Dictionary<string, string>();
            messageBodyKeyValue.Add(UserFullNameKey, userFullName);
            messageBodyKeyValue.Add("${code}", code);
            messageBodyKeyValue.Add(FromUserEmailKey, fromUserEmail);
            messageBodyKeyValue.Add(AppTitleKey, AppTitle);


            foreach (var keyValue in messageBodyKeyValue)
            {
                htmlMessage = htmlMessage.Replace(keyValue.Key, keyValue.Value);
            }

            //send using send grid or normally
            await SendGridEmailAsync(toUser.Email, subject, htmlMessage, true, userFullName);

        }

        /// <summary>
        /// Send access request email to administrator from new user.
        /// </summary>
        /// <param name="model"> User information and reason for need access to application.</param>
        /// <returns></returns>
        public async Task SendAccessRequestEmailAsync(RequestAccessDto model)
        {
            if (this._logger.IsInformationEnabled())
            {
                this._logger.LogInformation($"Send Access Request email from {model.Email}");
            }
            await this.ValidateEmailSetting();
            string emailTemplate = Path.Combine(this.WebRootPath, "EmailTemplate/user-access-request.html");
            string appLogoImage = Path.Combine(this.WebRootPath, "EmailTemplate/AppLogo.png");
            string htmlMessage = await FileUtile.ReadAllTextAsync(emailTemplate);
            string subject = $"Access request received from {model.FullName} for {AppTitle}";

            //Format the Dynamlic Values
            Dictionary<String, String> messageBodyKeyValue = new Dictionary<string, string>();
            messageBodyKeyValue.Add(UserFullNameKey, model.FullName);
            messageBodyKeyValue.Add("${userEmail}", model.Email);
            messageBodyKeyValue.Add("${phoneNumber}", model.PhoneNumber);
            messageBodyKeyValue.Add("${reason}", model.ReasonForAccess);
            messageBodyKeyValue.Add("${callbackUrl}", model.CallbackUrl);
            messageBodyKeyValue.Add(AppTitleKey, AppTitle);


            foreach (var keyValue in messageBodyKeyValue)
            {
                htmlMessage = htmlMessage.Replace(keyValue.Key, keyValue.Value);
            }

            //send using send grid or normally
            await SendGridEmailAsync(model.Email, subject, htmlMessage, true, model.FullName);


        }


        #endregion

        #region User Aler Email 

        /// <summary>
        /// Send alert email to given user.
        /// </summary>
        /// <param name="toUser">User to which email need to be sent.</param>
        /// <param name="subject">subject of email</param>
        /// <param name="message">Alert message to be set in email.</param>
        /// <returns></returns>
        public async Task SendAlertEmailAsync(User toUser, string subject, string message)
        {
            if (this._logger.IsInformationEnabled())
            {
                this._logger.LogInformation($"Send Alert email to {toUser.Email}");
            }
            await this.ValidateEmailSetting();
            string emailTemplate = Path.Combine(this.WebRootPath, "EmailTemplate/user-alert-email.html");
            string appLogoImage = Path.Combine(this.WebRootPath, "EmailTemplate/AppLogo.png");
            string htmlMessage = await FileUtile.ReadAllTextAsync(emailTemplate);
            //string userFullName = $"{toUser.FirstName} {toUser.LastName}";
            string userFullName = toUser.FullName;

            //Format the Dynamlic Values
            Dictionary<String, String> messageBodyKeyValue = new Dictionary<string, string>();
            messageBodyKeyValue.Add(UserFullNameKey, userFullName);
            messageBodyKeyValue.Add("${messageBody}", message);
            messageBodyKeyValue.Add(AppTitleKey, AppTitle);

            foreach (var keyValue in messageBodyKeyValue)
            {
                htmlMessage = htmlMessage.Replace(keyValue.Key, keyValue.Value);
            }

            //send using send grid or normally
            await SendGridEmailAsync(toUser.Email, subject, htmlMessage, true, userFullName);


        }
        #endregion

        #region Text Email message

        /// <summary>
        /// Send ad-hock email to given email address using given subject and text message. 
        /// </summary>
        /// <param name="email">email address where email to be sent.</param>
        /// <param name="subject">subject of email</param>
        /// <param name="textMessage">text message for email</param>
        /// <returns></returns>
        public async Task SendEmailAsync(string email, string subject, string textMessage)
        {
            if (this._logger.IsInformationEnabled())
            {
                this._logger.LogInformation("Send email to {0} with subject \"{1}\": {2}", email, subject, textMessage);
            }
            await this.SendEmailAsync(email, subject, textMessage, false, this._smtpSetting);
        }
        #endregion

        #region  Email private methods

        private async Task ValidateEmailSetting()
        {
            bool isValid = true;
            try
            {
                if (this._smtpSetting == null)
                {
                    var smtpSetting = await _settingService.GetSetting<SMTPSetting>(EnumSettingType.SMTPSettings);
                    this._smtpSetting = smtpSetting.Value;
                }
                if (String.IsNullOrWhiteSpace(this._smtpSetting.ServerAddress))
                {
                    isValid = false;
                }
            }
            catch
            {
                isValid = false;
            }
            if (!isValid)
            {
                throw new CSApplicationException(EnumEntityEvents.SETTING_NOSMTP_SETTING.ToString(), "Unable to send e-mail notification because of Mail Settings are not configured.");
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtmlBody, SMTPSetting smtpSetting)
        {
            MailMessage mail = new MailMessage();
            if (isHtmlBody)
            {
                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                mail.AlternateViews.Add(avHtml);
            }
            else
            {
                mail.Body = body;
            }
            mail.IsBodyHtml = isHtmlBody;
            await SendEmail(toEmail, subject, mail, smtpSetting);
        }

        private async Task SendEmailWithAttchmentAsync(string toEmail, string subject, string body, bool isHtmlBody, Dictionary<String, String> attchmentKeyValue)
        {
            await SendEmailWithAttchmentAsync(toEmail, subject, body, isHtmlBody, attchmentKeyValue, this._smtpSetting);
        }

        private async Task SendEmailWithAttchmentAsync(string toEmail, string subject, string body, bool isHtmlBody, Dictionary<String, String> attchmentKeyValue, SMTPSetting smtpSetting)
        {
            MailMessage mail = new MailMessage();

            if (isHtmlBody)
            {
                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                foreach (var keyValue in attchmentKeyValue)
                {
                    LinkedResource inline = new LinkedResource(keyValue.Value, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = keyValue.Key;
                    avHtml.LinkedResources.Add(inline);
                    //Attachment att = new Attachment(keyValue.Value);
                    //att.ContentDisposition.Inline = true;
                    //mail.Attachments.Add(att);
                }
                mail.AlternateViews.Add(avHtml);
            }
            else
            {
                mail.Body = body;
            }
            mail.IsBodyHtml = isHtmlBody;
            await SendEmail(toEmail, subject, mail, smtpSetting);
        }


        private async Task SendEmail(String toEmail, String subject, MailMessage message, SMTPSetting smtpSetting)
        {
            try
            {
                string userName = smtpSetting.UserName;
                string password = smtpSetting.Password;
                string emailServerAddress = smtpSetting.ServerAddress;
                int port = smtpSetting.ServerPort;
                string emailForm = smtpSetting.FromEmail;
                bool useSSL = smtpSetting.IsSSL;
                //toEmail = smtpSetting.AdminEmail; // fatch for time being.
                if (String.IsNullOrEmpty(emailForm))
                {
                    emailForm = userName;
                }
                message.From = new MailAddress(emailForm);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                using (var client = new SmtpClient())
                {
                    String address = $"{emailServerAddress}:{port}";
                    SmtpClient smtp = new SmtpClient(emailServerAddress, port);
                    smtp.EnableSsl = useSSL;
                    smtp.DeliveryMethod = smtpSetting.DeliveryMethod;
                    smtp.DeliveryFormat = smtpSetting.DeliveryFormat;
                    smtp.UseDefaultCredentials = smtpSetting.UseDefaultCredentials;
                    if (!smtpSetting.UseDefaultCredentials)
                    {
                        System.Net.NetworkCredential cred = new System.Net.NetworkCredential(userName, password);
                        smtp.Credentials = cred;
                    }
                    if (smtpSetting.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
                    {
                        smtp.PickupDirectoryLocation = smtpSetting.PickupDirectoryLocation;
                    }
                    //send the email
                    await smtp.SendMailAsync(message);
                }

                //using (var client = new SmtpClient())
                //{
                //    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                //    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                //    client.Connect(emailServerAddress, port, useSSL);

                //    // Note: since we don't have an OAuth2 token, disable
                //    // the XOAUTH2 authentication mechanism.  
                //    client.AuthenticationMechanisms.Remove("XOAUTH2");

                //    // Note: only needed if the SMTP server requires authentication
                //    client.Authenticate(userName, password);

                //    await client.SendAsync(message);
                //    client.Disconnect(true);
                //}
            }
            catch (Exception ex)
            {
                this._logger.LogCritical(ex.Message);
                throw new Exception($"Error while sending email to {toEmail}", ex);
            }
        }

        #endregion


        private async Task SendGridEmailAsync(string toEmail, string subject, string body, bool isHtmlBody, string toUserName)
        {
            if (this._smtpSetting.IsUsingSendGrid == true) // send using SendGrid
            {
                SendGridEmailWithAttachmentAsync(toEmail, toUserName, subject, body).Wait();
            }
            else // send normally
            {
                string appLogoImage = Path.Combine(this.WebRootPath, "EmailTemplate/AppLogo.png");

                //attachment key value
                Dictionary<String, String> attachmentKeyValue = new Dictionary<string, string>();
                attachmentKeyValue.Add(AppLogoLink, appLogoImage);

                await SendEmailWithAttchmentAsync(toEmail, subject, body, isHtmlBody, attachmentKeyValue);
            }
        }

        #region send email using sendGrid

        private async Task SendGridEmailWithAttachmentAsync(string toEmail, string toUserName, string subject, string htmlMessage)
        {
            try
            {
                string apiKey = this._smtpSetting.Password; ;
                string fromEmail = this._smtpSetting.FromEmail;
                string fromName = this._smtpSetting.UserName;
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress();
                from.Email = fromEmail;
                from.Name = fromName;

                var to = new EmailAddress(toEmail, toUserName);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, " ", htmlMessage);
                string appLogoImage = Path.Combine(this.WebRootPath, "EmailTemplate/AppLogo.png");

                //attachment key value
                using (var fileStream = File.OpenRead(appLogoImage))
                {
                    await msg.AddAttachmentAsync("AppLogo.png", fileStream, "image/png", "inline", AppLogoLink);
                    var response = await client.SendEmailAsync(msg);
                }

            }
            catch (Exception ex)
            {
                this._logger.LogCritical(ex.Message);
                throw new CSApplicationException(EnumEntityEvents.SETTING_SENDEMAIL_FAILD.ToString(), $"Error while sending email with sendGrid to {toEmail}", ex);
            }
        }


        #endregion

    }

    #endregion

}
