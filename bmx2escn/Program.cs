using System;

namespace bmx2escn {
    class Program {
        static void Main(string[] args) {
            var argParser = new CmdArgParser(args);
            if (argParser.Result is null)
                Environment.Exit(1);



        }
    }
}
