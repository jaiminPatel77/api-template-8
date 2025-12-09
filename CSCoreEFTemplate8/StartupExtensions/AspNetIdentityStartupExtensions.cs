using ConfidoSoft.Data.Domain.Database;
using ConfidoSoft.Data.Domain.DBModels;
using ConfidoSoft.Data.Services.BLServices;
using CSCoreEFTemplate8;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CSCoreEFTemplate8.StartupExtensions
{
    internal static class AspNetIdentityStartupExtensions
    {

        internal static void AddAspNetIdentityServices(this Startup startup, IServiceCollection services)
        {
            //https://docs.microsoft.com/en-gb/aspnet/core/security/authentication/accconfirm
            //config => 
            //{
            //    config.SignIn.RequireConfirmedEmail = true; //user need to confirm first.
            //}
            services.AddIdentity<User, Role>(
                config =>
                {
                    // Password settings
                    //config.Password.RequireDigit = false;
                    //config.Password.RequiredLength = 4;
                    //config.Password.RequireNonAlphanumeric = false;
                    //config.Password.RequireUppercase = false;
                    //config.Password.RequireLowercase = false;

                    //// Lockout settings
                    //config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                    //config.Lockout.MaxFailedAccessAttempts = 10;

                    //// Cookie settings
                    //config.Cookies.ApplicationCookie.ExpireTimeSpan = TimeSpan.FromDays(150);
                    //config.Cookies.ApplicationCookie.LoginPath = "/Account/LogIn";
                    //config.Cookies.ApplicationCookie.LogoutPath = "/Account/LogOut";

                    // User settings
                    config.User.RequireUniqueEmail = true;

                    //Used same as phone number token for change email. i.e. sort code as user need to manually input it.
                    config.Tokens.ChangeEmailTokenProvider = config.Tokens.ChangePhoneNumberTokenProvider;

                    //config.SignIn.RequireConfirmedEmail = true; //user need to confirm first.
                    config.Stores.ProtectPersonalData = true;
                }
                )
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddPersonalDataProtection<AppDataProtector, AppDataKeyRing>()
                .AddDefaultTokenProviders();
                //.AddPasswordValidator<CustomPasswordValidator<User>>();

            //Added to handle null or empty value
            services.AddSingleton<IPersonalDataProtector, PersonalDataProtector>();
        }

        internal static void ConfigureIdentity(this Startup startup, IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
