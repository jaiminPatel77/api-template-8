using ConfidoSoft.Data.Domain.Dtos;
using ConfidoSoft.Data.Services.BLServices;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfidoHotelApi.Hub
{
   

    #region Implementation of IApplicationHubService

    public class ApplicationHubService : IApplicationHubService
    {
        //locking object while accessing counter.
        private static readonly object _lockObject = new object();

        private ILogger _logger;
        private readonly IHubContext<ApplicationHub, IApplicationHub> _hub;

        #region Fix Client subject text/prefix and associated current live connection.
        /// <summary>
        /// subject for user notification count changed.
        /// after _ we will append user id or session.
        /// </summary>
        public const string NotificationCountChanged = "NotificationCountChanged_";
        //List of the client connection for notification count.
        private static Dictionary<String, String> _notificationCountClient = new Dictionary<string, string>();

        #endregion

        public ApplicationHubService(ILogger<ApplicationHubService> logger, 
            IHubContext<ApplicationHub, IApplicationHub> hubContext)
        {
            this._logger = logger;
            this._hub = hubContext;
        }        

        #region Notification Count Handling

        /// <summary>
        /// Send user Notification for notification count changed!
        /// </summary>
        /// <param name="userId">User id for which we need to SignalR if connected</param>
        /// <returns></returns>
        public async Task SendNotificationCountChanged(long userId)
        {
            String userNotification = null;
            String userNotificationTopic = (NotificationCountChanged + userId);
            lock (_lockObject)
            {
                userNotification = _notificationCountClient.Values.FirstOrDefault(e => e == userNotificationTopic);
            }
            try
            {
                if (false == String.IsNullOrEmpty(userNotification))
                {
                    var retVal = new NotificationCounts
                    {
                        UnreadCountChanged = true
                    };
                    await _hub.Clients.Group(userNotification).NotificationCountChanged(retVal);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        #endregion

        #region Connected/Disconnect Client handling

        /// <summary>
        /// On any client Subscribe to Hub. Add the topic
        /// </summary>
        public static void OnClientSubscribe(String connectionId, String topic)
        {
            lock (_lockObject)
            {
                if (topic.StartsWith(NotificationCountChanged))
                {
                    if (!_notificationCountClient.ContainsKey(connectionId))
                    {
                        _notificationCountClient.Add(connectionId, topic);
                    }
                }
               
            }
        }

        /// <summary>
        /// On any client Unsubscripted to Hub. remove the topic
        /// </summary>
        public static void OnClientUnsubscribe(String connectionId, String topic)
        {
            lock (_lockObject)
            {
                if (topic.StartsWith(NotificationCountChanged))
                {
                    if (_notificationCountClient.ContainsKey(connectionId))
                    {
                        _notificationCountClient.Remove(connectionId);
                    }
                }               
            }
        }


        /// <summary>
        /// On any client Disconnected to Hub. remove the topic list entry
        /// </summary>
        public static void OnClientDisconnected(String connectionId)
        {
            lock (_lockObject)
            {   
                if (_notificationCountClient.ContainsKey(connectionId))
                {
                    _notificationCountClient.Remove(connectionId);
                }
            }
        }

        #endregion
        
    }

    #endregion
}
