using System;

namespace bmx2escn {
    class Program {
        static void Main(string[] args) {
            var argParser = new CmdArgParser(args);
            if (argParser.Result is null)
                Environment.Exit(1);

            ConvCore core = new ConvCore(argParser.Result);
            core.DoConv();
            core.Dispose();

            Console.WriteLine("Done.");
        }
    }
}
