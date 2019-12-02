using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Microsoft.Azure.ServiceBus.Management;

namespace AmqpShovel {
    class SBMessageHandler {
        static IQueueClient receiver;
        static IQueueClient sender;
        static int max;
        private static int current;

        public static async Task ResendDeadLetters(ShovelOptions opts) {
            Console.Out.WriteLine($"Connecting to {opts.Queue} to shovel maximum of {opts.Max} messages");
            var managementClient = new ManagementClient(opts.ConnectionString);
            var queue = await managementClient.GetQueueRuntimeInfoAsync(opts.Queue);
            var messageCount = queue.MessageCountDetails.DeadLetterMessageCount;
            Console.Out.WriteLine($"Message queue {EntityNameHelper.FormatDeadLetterPath(opts.Queue)} has {messageCount} messages");

            receiver = new QueueClient(opts.ConnectionString, EntityNameHelper.FormatDeadLetterPath(opts.Queue));
            sender = new QueueClient(opts.ConnectionString, opts.Queue);
            max = opts.Max;
            if (messageCount < opts.Max) {
                Console.Out.WriteLine($"resetting max messages to {messageCount}");
                max = Convert.ToInt32(messageCount);
            }

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            // Register QueueClient's MessageHandler and receive messages in a loop
            RegisterOnMessageHandlerAndReceiveMessages(opts);

            Console.ReadKey();

            await receiver.CloseAsync();
        }


        static void RegisterOnMessageHandlerAndReceiveMessages(ShovelOptions opts) {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler) {
                MaxConcurrentCalls = opts.MaxConcurrentCalls,
                AutoComplete = false
            };

            // Register the function that will process messages
            receiver.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token) {

            var body = message.Body;
            var clone = message.Clone();
            clone.Body = message.Body;

            var b = message.GetBody<byte[]>();
            var foo = Encoding.UTF8.GetString(b);

            var bdy = message.GetBody<Stream>();

            if (body == null) {
                Console.Out.WriteLine("ABORT: empty body");
                await receiver.CloseAsync();
            }

            current++;
            var s = body == null ? "<null>" : Encoding.UTF8.GetString(body);
            Console.WriteLine($"{current}. Received message: MessageId:{message.MessageId} SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{s}");

            Console.Out.WriteLine($"resubmitting message {clone.MessageId}");
            await sender.SendAsync(clone);
            Console.Out.WriteLine($"completeing message {message.MessageId}");
            await receiver.CompleteAsync(message.SystemProperties.LockToken);
            Console.Out.WriteLine($"completed message {message.MessageId}");

            if (current >= max) {
                Console.Out.WriteLine("max messages processed, closing connectiong");
                await receiver.CloseAsync();
                Console.Out.WriteLine("closed");
            }
        }

        // Use this handler to examine the exceptions received on the message pump.
        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs) {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
