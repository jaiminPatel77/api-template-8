using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConfidoSoft.Data.Services.BLServices
{
    #region IApplicationHubService interface
    /// <summary>
    /// Interface for IApplicationHubService.
    /// </summary>
    public interface IApplicationHubService
    {
        /// <summary>
        /// Send user Notification for notification count changed!
        /// </summary>
        /// <param name="userId">User id for which we need to SignalR if connected</param>
        /// <returns></returns>
        Task SendNotificationCountChanged(long userId);
    }
    #endregion

}
