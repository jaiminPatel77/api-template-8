using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConfidoSoft.Data.Domain.Database;
using ConfidoSoft.Data.Domain.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ConfidoSoft.Data.Services.DBServices;
using ConfidoSoft.Data.Services.BLServices;
using ConfidoSoft.Data.Domain.DBModels.Settings;
using ConfidoSoft.Data.Domain.Consts;

namespace CSCoreEFTemplate8.StartupExtensions
{
    public static class DatabaseStartupExtensions
    {
        internal static void InitializeDatabase(this Startup startup, IApplicationBuilder app)
        {

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var startupLogger = serviceScope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
                try
                {                    
                    startupLogger.LogInformation("Application startup data handing started!");
                    var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.Migrate();
                    startupLogger.LogInformation("Database Migration check done.");
                    SetApplicaitonDataKeyRing(serviceScope, startupLogger).Wait();
                    startupLogger.LogInformation("Loading encryption key done.");
                    var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
                    var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                    InitalDataSeed.InitializeDatabase(dbContext, userManager, roleManager).Wait();
                    startupLogger.LogInformation("Application startup data handing Done!");
                }
                catch(Exception ex)
                {
                    startupLogger.LogError(ex, "Error while InitializeDatabase at startup! ");
                }
            }
        }

        /// <summary>
        /// Load the application encryption key details.
        /// </summary>
        /// <param name="serviceScope"></param>
        /// <param name="startupLogger"></param>
        /// <returns></returns>
        private static async Task SetApplicaitonDataKeyRing(IServiceScope serviceScope, ILogger startupLogger)
        {
            try
            {
                ISettingService settingService = serviceScope.ServiceProvider.GetRequiredService<ISettingService>();
                ILookupProtectorKeyRing keyRing = serviceScope.ServiceProvider.GetRequiredService<ILookupProtectorKeyRing>();
                ISetLookupProtectorKeyRing setKeyRing = keyRing as ISetLookupProtectorKeyRing;
                if (setKeyRing != null)
                {
                    if (settingService != null)
                    {
                        var currentSetting = await settingService.GetSetting<EncryptionKeySetting>(EnumSettingType.EncryptionKeySettings);
                        if (currentSetting.Value.Key != null)
                        {
                            startupLogger.LogInformation("Loading encryption key");
                            var previousSetting = await settingService.GetSetting<EncryptionKeySetting>(EnumSettingType.PreviousEncryptionKeySettings);
                            if (previousSetting.Value.Key != null)
                            {
                                startupLogger.LogInformation("Loading previous encryption key");
                                setKeyRing.SetKey(previousSetting.Value.KeyId, previousSetting.Value.Key, false);
                            }
                            setKeyRing.SetKey(currentSetting.Value.KeyId, currentSetting.Value.Key, true);
                        }
                        else
                        {
                            //set random key if not already set!
                            currentSetting.Value = EncryptionKeySetting.GenerateNewKey(null);
                            await settingService.UpdateSetting<EncryptionKeySetting>(currentSetting);
                            setKeyRing.SetKey(currentSetting.Value.KeyId, currentSetting.Value.Key, true);
                            //set random encryption key.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                startupLogger.LogError(ex, "Error while setting encryption key at start-up!");
                //do nothing if not loadded key in case of migration for first run.. we do load from portal after login.
            }
        }
    }
}
