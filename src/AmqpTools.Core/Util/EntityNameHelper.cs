// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace AmqpTools.Core.Util {
    /// <summary>
    /// This class can be used to format the path for different Service Bus entity types.
    /// </summary>
    /// <remarks>Local copy from deprecated Microsoft.Azure.ServiceBus package</remarks>
    /// <see href="https://github.com/Azure/azure-sdk-for-net/blob/c429da7ddcd3ecf1c6f500d24660c98620a5bc79/sdk/servicebus/Microsoft.Azure.ServiceBus/src/EntityNameHelper.cs"/>
    public static class EntityNameHelper {
        private const string PathDelimiter = @"/";
        private const string SubscriptionsSubPath = "Subscriptions";
        private const string RulesSubPath = "Rules";
        private const string SubQueuePrefix = "$";
        private const string DeadLetterQueueSuffix = "DeadLetterQueue";
        private const string DeadLetterQueueName = SubQueuePrefix + DeadLetterQueueSuffix;
        private const string Transfer = "Transfer";
        private const string TransferDeadLetterQueueName = SubQueuePrefix + Transfer + PathDelimiter + DeadLetterQueueName;

        public enum MessageType {
            Active,
            DeadLetter,
            Scheduled
        }

        public static string FormatQueue(string queue, string messageType) {
            Enum.TryParse(typeof(MessageType), messageType, true, out object type);
            switch (type) {
                case MessageType.Active:
                    return queue;
                case MessageType.DeadLetter:
                    return FormatDeadLetterPath(queue);
                default:
                    // Not supported
                    throw new InvalidOperationException($"{messageType} is not supported");
            }
        }

        /// <summary>
        /// Formats the dead letter path for either a queue, or a subscription.
        /// </summary>
        /// <param name="entityPath">The name of the queue, or path of the subscription.</param>
        /// <returns>The path as a string of the dead letter entity.</returns>
        public static string FormatDeadLetterPath(string entityPath) {
            return FormatSubQueuePath(entityPath, DeadLetterQueueName);
        }

        /// <summary>
        /// Formats the subqueue path for either a queue, or a subscription.
        /// </summary>
        /// <param name="entityPath">The name of the queue, or path of the subscription.</param>
        /// <returns>The path as a string of the subqueue entity.</returns>
        public static string FormatSubQueuePath(string entityPath, string subQueueName) {
            return string.Concat(entityPath, PathDelimiter, subQueueName);
        }

        /// <summary>
        /// Formats the subscription path, based on the topic path and subscription name.
        /// </summary>
        /// <param name="topicPath">The name of the topic, including slashes.</param>
        /// <param name="subscriptionName">The name of the subscription.</param>
        public static string FormatSubscriptionPath(string topicPath, string subscriptionName) {
            return string.Concat(topicPath, PathDelimiter, SubscriptionsSubPath, PathDelimiter, subscriptionName);
        }

        /// <summary>
        /// Formats the rule path, based on the topic path, subscription name and rule name.
        /// </summary>
        /// <param name="topicPath">The name of the topic, including slashes.</param>
        /// <param name="subscriptionName">The name of the subscription.</param>
        /// <param name="ruleName">The name of the rule</param>
        public static string FormatRulePath(string topicPath, string subscriptionName, string ruleName) {
            return string.Concat(
                topicPath, PathDelimiter,
                SubscriptionsSubPath, PathDelimiter,
                subscriptionName, PathDelimiter,
                RulesSubPath, PathDelimiter, ruleName);
        }

        /// <summary>
        /// Utility method that creates the name for the transfer dead letter receiver, specified by <paramref name="entityPath"/>
        /// </summary>
        public static string Format​Transfer​Dead​Letter​Path(string entityPath) {
            return string.Concat(entityPath, PathDelimiter, TransferDeadLetterQueueName);
        }



    }
}
