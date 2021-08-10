using System;
using System.IO;
using System.Linq;
using Messenger.Core.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Serilog.Context;


// Based on https://stackoverflow.com/questions/55118805/extract-called-method-information-using-roslyn
class Program
{
    static void Main(string[] args)
    {
        var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(args[0]));

        var references = new[]
                         {
                             MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                             MetadataReference.CreateFromFile(typeof(LogContext).Assembly
                                                                 .Location
                                                             ),
                             MetadataReference.CreateFromFile(typeof(MessageService).Assembly
                                                                 .Location
                                                             )
                         };

        var compilation = CSharpCompilation.Create("MyCompilation",
                                                   new[] {tree},
                                                   references
                                                  );

        var model = compilation.GetSemanticModel(tree);

        var invocationExpressionSyntaxes = tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>();

        // FIX: This does not return methods called on variables
        foreach (var invocation in invocationExpressionSyntaxes)
        {
            var invokedSymbol = model.GetSymbolInfo(invocation).Symbol;

            if (invokedSymbol is null)
            {
                var candidates = model.GetSymbolInfo(invocation).CandidateSymbols;

                if (candidates.Length > 0)
                {
                    invokedSymbol = candidates[0];
                }
            }

            if (invokedSymbol != null)
            {
                var name = invokedSymbol.ToString();
                Console.WriteLine(name);
            }
        }
    }
}