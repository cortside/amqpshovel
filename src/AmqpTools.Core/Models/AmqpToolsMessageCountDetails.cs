namespace AmqpTools.Core.Models {
    /// <summary>
    /// Response model for message count details
    /// </summary>
    public class AmqpToolsMessageCountDetails {
        /// <summary>
        /// ActiveMessageCount
        /// </summary>
        public long ActiveMessageCount { get; set; }
        /// <summary>
        /// DeadLetterMessageCount
        /// </summary>
        public long DeadLetterMessageCount { get; set; }
        /// <summary>
        /// ScheduledMessageCount
        /// </summary>
        public long ScheduledMessageCount { get; set; }
        /// <summary>
        /// TransferMessageCount
        /// </summary>
        public long TransferMessageCount { get; set; }
        /// <summary>
        /// TransferDeadLetterMessageCount
        /// </summary>
        public long TransferDeadLetterMessageCount { get; set; }
    }
}
