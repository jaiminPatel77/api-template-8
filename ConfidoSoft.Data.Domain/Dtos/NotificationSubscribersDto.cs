using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.Dtos
{
    /// <summary>
    /// SignalR Hun interface object to notify user with unread count changed.
    /// </summary>
    public class NotificationCounts
    {
        /// <summary>
        /// Indicate that unread count changed.
        /// </summary>
        public bool UnreadCountChanged { get; set; }
    }
}
