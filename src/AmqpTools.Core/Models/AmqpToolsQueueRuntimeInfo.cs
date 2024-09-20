using System;

namespace AmqpTools.Core.Models {

    /// <summary>
    /// Queue runtime info response model
    /// </summary>
    public class AmqpToolsQueueRuntimeInfo {
        /// <summary>
        /// Path
        /// </summary>
        public string Path { get; internal set; }
        /// <summary>
        ///   The total number of messages in the queue.
        ///   /// </summary>
        public long MessageCount { get; internal set; }

        /// <summary>
        ///     Message count details of the sub-queues of the entity.
        /// </summary>
        public AmqpToolsMessageCountDetails MessageCountDetails { get; set; }

        /// <summary>
        /// 
        ///     Current size of the entity in bytes.
        /// </summary>
        public long SizeInBytes { get; internal set; }

        /// <summary>
        ///     The System.DateTime when the entity was created.
        /// </summary>
        public DateTime CreatedAt { get; internal set; }

        /// <summary>
        ///     The System.DateTime when the entity description was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; internal set; }

        /// <summary>
        ///     The System.DateTime when the entity was last accessed.
        /// </summary>
        public DateTime AccessedAt { get; internal set; }
    }
}
