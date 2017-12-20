﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDBMSP.Entities.Article;
using DDBMSP.Entities.Query;
using DDBMSP.Entities.User;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DDBMSP.Common.QueryEngine
{
    public enum ScriptType
    {
        QuerySelector,
        QueryAggregator
    }

    public class QueryContext
    {
        public Dictionary<Guid, ArticleState> Articles = null;
        public Dictionary<Guid, UserState> Users = null;
        public IEnumerable<object> TaskResult = null;
    }

    public class QueryScript
    {
        public ScriptRunner<object> Selector { get; set; }
        public ScriptRunner<object> Aggregator { get; set; }
    }

    public static class QueryEngine
    {
        private static ScriptOptions ScriptOptions = ScriptOptions.Default
            .WithReferences(typeof(ArticleState).Assembly, typeof(System.Linq.IQueryable).Assembly,
                typeof(IEnumerable<>).Assembly, typeof(Guid).Assembly)
            .WithImports("DDBMSP.Entities.Article", "DDBMSP.Entities.User", "System.Linq", "System",
                "System.Collections.Generic").WithEmitDebugInformation(false);
        
        private static Dictionary<string, QueryScript> Queries = new Dictionary<string, QueryScript>();

        public static void CompileAndRegister(QueryDefinition queryDefinition) {
            if (Queries.ContainsKey(queryDefinition.Name))
                throw new Exception($"Query \"{queryDefinition.Name}\" already exists.");
            
            var selector = CSharpScript.Create(queryDefinition.SelectorLambda, ScriptOptions, typeof(QueryContext));
            selector.Compile();

            var aggregator = CSharpScript.Create($"var Selected = TaskResult.Select(i=>({queryDefinition.ReturnTypeName})i);",
                ScriptOptions, typeof(QueryContext)).ContinueWith<object>(queryDefinition.AggregationLambda, ScriptOptions);
            aggregator.Compile();
            
            Queries.Add(queryDefinition.Name, new QueryScript {
                Aggregator = aggregator.CreateDelegate(),
                Selector = selector.CreateDelegate()
            });
        }

        public static async Task<object> Execute(ScriptType type, QueryDefinition queryDefinition, QueryContext context) {
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: In thread");
            if (!Queries.ContainsKey(queryDefinition.Name))
                CompileAndRegister(queryDefinition);
            
            dynamic ret;
            if (type == ScriptType.QuerySelector) {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Executing Selector");
                ret = await Queries[queryDefinition.Name].Selector.Invoke(context);
            }
            else {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Executing Aggregator");
                ret = await Queries[queryDefinition.Name].Aggregator.Invoke(context);
            }
            
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Returning");
            return ret;
        }
        
    }
}