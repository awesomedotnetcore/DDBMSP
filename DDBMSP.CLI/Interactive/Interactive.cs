﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using DDBMSP.CLI.Core;
using DDBMSP.CLI.Interactive.Query;
using DDBMSP.Common;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Serialization;

namespace DDBMSP.CLI.Interactive
{
    [Verb("interact", HelpText = "Run queries against your cluster")]
    public class Interactive : ConnectedTool
    {
        public CSharpRepl Repl { get; set; } = new CSharpRepl();
        
        [Verb("quit", HelpText = "Exit the CLI interactive mode")]
        public class Quit
        {
            public int Run() {
                Environment.Exit(0);
                return 0;
            }
        }

        public async Task<int> Run() {
            var parser = new Parser(settings => {
                settings.IgnoreUnknownArguments = true;
                settings.HelpWriter = null;
                settings.CaseSensitive = false;
            });
            
            while (true) {
                var shouldInterpret = true;
                var line = ReadLine.Read("127.0.0.1> ");
                if (string.IsNullOrWhiteSpace(line)) {
                    Console.ReadKey(true);
                    continue;
                }
                
                ReadLine.AddHistory(line);
                
                if (line == "query commit" || line == "query commit") {
                    line = line.Insert(0, "\"");
                    line = line.Insert(line.Length, "\"");
                }
                
                var result = parser.ParseArguments<CommitQuery, ExecuteQuery, Quit>(SplitCommandLine(line));
                result.WithNotParsed(errors => {
                    if (errors.Any(error => error.Tag == ErrorType.BadVerbSelectedError)) return;
                    var message = HelpText.AutoBuild(result);
                    message.Copyright = "";
                    message.Heading = "";
                    Console.WriteLine(message.ToString());
                    shouldInterpret = false;
                });
                result.WithParsed(o => shouldInterpret = false);
                
                result.MapResult(
                        (CommitQuery opts) => opts.Run(ClusterClient).Result,
                        (ExecuteQuery o) => o.Run(Repl, ClusterClient).Result,
                        (Quit o) => o.Run(),
                        errs => -1);

                if (shouldInterpret) {
                    try {
                        var res = await Repl.Evaluate(line);
                        if (res != null)
                            Console.WriteLine(res);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e.Message);
                    }
                }
                
                Console.ReadKey(true);
            }
        }

        private static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            var inQuotes = false;
            return commandLine.Split(c =>
                {
                    if (c == '\"')
                        inQuotes = !inQuotes;

                    return !inQuotes && c == ' ';
                })
                .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
                .Where(arg => !string.IsNullOrEmpty(arg));
        }
    }
}