using System;

namespace AmqpTools.Core.Models {
    /// <summary>
    /// Message Details
    /// </summary>
    public class AmqpToolsMessage {
        /// <summary>
        /// Body
        /// </summary>
        public object Body { get; set; }
        /// <summary>
        /// MessageId
        /// </summary>
        public string MessageId { get; set; }
        /// <summary>
        /// CorrelationId
        /// </summary>
        public string CorrelationId { get; set; }
        /// <summary>
        /// PartitionKey
        /// </summary>
        public string PartitionKey { get; set; }
        /// <summary>
        /// ExpiresAtUtc
        /// </summary>
        public DateTime? ExpiresAtUtc { get; set; }
        /// <summary>
        /// ContentType
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// ScheduledEnqueueTimeUtc
        /// </summary>
        public DateTime? ScheduledEnqueueTimeUtc { get; set; }
        /// <summary>
        /// UserProperties
        /// </summary>
        public object UserProperties { get; set; }
        /// <summary>
        /// SystemProperties
        /// </summary>
        public object SystemProperties { get; set; }
    }
}
