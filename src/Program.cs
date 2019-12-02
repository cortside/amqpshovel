using System;
using System.Collections.Generic;
using CommandLine;

namespace AmqpShovel {

    public class Program {

        static void Main(string[] args) {
            CommandLine.Parser.Default.ParseArguments<ShovelOptions>(args)
              .WithParsed<ShovelOptions>(opts => RunOptionsAndReturnExitCode(opts))
              .WithNotParsed<ShovelOptions>((errs) => HandleParseError(errs));
        }


        private static void HandleParseError(IEnumerable<Error> errs) {
            foreach (var err in errs) {
                Console.Out.WriteLine(err.ToString());
            }
        }

        private static void RunOptionsAndReturnExitCode(ShovelOptions opts) {
            //SBMessageHandler.ResendDeadLetters(opts).GetAwaiter().GetResult();
            AmqpMessageHandler.ResendDeadLetters(opts);
        }
    }
}
