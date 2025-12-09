using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCoreEFTemplate8.AppSettings;
using CSCoreEFTemplate8.Auth;

namespace CSCoreEFTemplate8.StartupExtensions
{
    public static class AuthStartupExtensions
    {
        //https://fullstackmark.com/post/10/user-authentication-with-angular-and-asp-net-core

        internal static void AddAuthServices(this Startup startup, IServiceCollection services)
        {
            string secretKey = "cNovDmHLpUAn23sqfihqGbMRsRo1PVkt"; // todo: get this from somewhere secure
            var keyFromConfig = startup.Configuration["SecurityKey"];
            if(false == String.IsNullOrEmpty(keyFromConfig))
            {
                secretKey = keyFromConfig;
            }
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

            //get Google ReCaptcha setting
            services.Configure<ReCaptchaSettings>(startup.Configuration.GetSection(nameof(ReCaptchaSettings)));

            // Register the ConfigurationBuilder instance of FacebookAuthSettings
            services.Configure<FacebookAuthSettings>(startup.Configuration.GetSection(nameof(FacebookAuthSettings)));

            // Register the ConfigurationBuilder instance of AzureAdAuthSettings
            services.Configure<AzureAdAuthSettings>(startup.Configuration.GetSection(nameof(AzureAdAuthSettings)));

            //services.TryAddTransient<IHttpContextAccessor, HttpContextAccessor>();

            // jwt wire up
            // Get options from app settings
            var jwtAppSettingOptions = startup.Configuration.GetSection(nameof(JwtIssuerOptions));

            services.AddScoped<IAuthService, AuthService>();

            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                //https://www.jerriepelser.com/blog/aspnetcore-jwt-saving-bearer-token-as-claim/ not very sure whether we need this!
                //configureOptions.SaveToken = true;
            });

            // api user claim policy
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("ApiUser", policy => policy.RequireClaim(Constants.Strings.JwtClaimIdentifiers.Rol, Constants.Strings.JwtClaims.ApiAccess));
            //});
        }
    }
}
