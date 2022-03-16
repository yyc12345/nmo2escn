using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace bmx2escn {

    public class CmdArg {
        [Option('b', "bmx", Required = true, HelpText = "Input BMX to be processed.")]
        public string InputBmx { get; set; }
        [Option('j', "json", Default = null, HelpText = "Output JSON file for group infomation.")]
        public string OutputJson { get; set; }
        [Option("jt", Default = JsonFormatType.OpenBallance, HelpText = "Output JSON file standard.")]
        public JsonFormatType OutputJsonType { get; set; }
        [Option('e', "escn", Default = null, HelpText = "Output ESCN file for scene.")]
        public string OutputEscn { get; set; }
        [Option("inpath", Default = "res://textures", HelpText = "Ballance internal textures path in ESCN file.")]
        public string EscnInternalPath { get; set; }
    }

    public class CmdArgParser {

        public CmdArg Result { get; private set; }

        public CmdArgParser(string[] argv) {
            Result = null;

            CommandLine.Parser.Default.ParseArguments<CmdArg>(argv)
                .WithParsed(ParsedFunc)
                .WithNotParsed(NotParsedFunc);
        }

        void ParsedFunc(CmdArg opts) {
            Result = opts;
        }

        void NotParsedFunc(IEnumerable<Error> errs) {
            errs.Output();
        }
    }
}
