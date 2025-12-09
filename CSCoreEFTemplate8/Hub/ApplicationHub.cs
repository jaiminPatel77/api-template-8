using ConfidoSoft.Data.Domain.Dtos;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfidoHotelApi.Hub
{

    #region IApplicationHub Interface to SignalR

    /// <summary>
    /// Interface of client methods.
    /// </summary>
    public interface IApplicationHub
    {
        Task NotificationCountChanged(NotificationCounts notificationCounts);
    }

    #endregion

    #region Application Hub Implementation

    public class ApplicationHub : Hub<IApplicationHub>
    {

        #region List of the Service methods which client call

        /// <summary>
        /// Server side method to which client subscribe to particular topic to get the update.
        /// </summary>
        /// <param name="topic"> name of the topic for which client get update from server</param>
        /// <returns></returns>
        public Task Subscribe(string topic)
        {
            var retVal = Groups.AddToGroupAsync(Context.ConnectionId, topic);
            ApplicationHubService.OnClientSubscribe(this.Context.ConnectionId, topic);
            //this.Clients.Caller.SendConnectMessage("Well come to Group Notification for : " + topic);
            //this.Clients.Group("test").
            return retVal;
        }

        /// <summary>
        /// Server side method to which client un-subscribe to particular topic to stop getting the update.
        /// </summary>
        /// <param name="topic">topic name</param>
        /// <returns></returns>
        public Task Unsubscribe(string topic)
        {
            ApplicationHubService.OnClientUnsubscribe(this.Context.ConnectionId, topic);
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, topic);
        }

        #endregion

        #region OnConnect disconnect handing

        /// <summary>
        /// Update counter on new client connected.
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// Update counter on client Disconnected.
        /// </summary>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            ApplicationHubService.OnClientDisconnected(this.Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
        
        #endregion
    }
    #endregion
}
