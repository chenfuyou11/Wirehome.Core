using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Wirehome.Core.Extensions;
using System.Net;
using System.Security;
using CSharpFunctionalExtensions;

namespace Wirehome.Core.Services.Roslyn
{
    public class RoslynAsseblyGenerator
    {
        public Result<string> GenerateAssembly(string assemblyName, string sourceDictionary, IEnumerable<string> dependencies)
        {
            var syntaxTrees = ParseSourceCode(sourceDictionary);
            var references = ParseDependencies(dependencies);

            var compilation = CSharpCompilation.Create(assemblyName)
                                               .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                                               .AddReferences(references)
                                               .AddSyntaxTrees(syntaxTrees);

            var path = Path.Combine(sourceDictionary, assemblyName);
            var compilationResult = compilation.Emit(path);

            return compilationResult.Success ? Result.Ok(path) : Result.Fail<string>(ReadCompilationErrors(compilationResult));
        }

        private string ReadCompilationErrors(Microsoft.CodeAnalysis.Emit.EmitResult compilationResult)
        {
            var sb = new StringBuilder();
            foreach (Diagnostic codeIssue in compilationResult.Diagnostics)
            {
                sb.AppendLine($"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}, Location: {codeIssue.Location.GetLineSpan().ToString()}, Severity: {codeIssue.Severity.ToString()}");
            }
            return sb.ToString();
        }

        private IEnumerable<PortableExecutableReference> ParseDependencies(IEnumerable<string> dependencies)
        {
            //Core references
            var references = new List<PortableExecutableReference>
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),             //System.Private.CoreLib.dll
                MetadataReference.CreateFromFile(typeof(FileAttributes).GetTypeInfo().Assembly.Location),     //System.Runtime.dll
                MetadataReference.CreateFromFile(typeof(NetworkCredential).GetTypeInfo().Assembly.Location),  //System.Net.Primitives.dll
                MetadataReference.CreateFromFile(typeof(SecureStringMarshal).GetTypeInfo().Assembly.Location) //System.Runtime.InteropServices.dll
            };

            dependencies.ForEach(dep => references.Add(MetadataReference.CreateFromFile(dep)));
            return references;
        }

        private IEnumerable<SyntaxTree> ParseSourceCode(string sourceDir, string filter = "*.cs") =>
        Directory.GetFiles(sourceDir, filter, SearchOption.AllDirectories).Select(file => SyntaxFactory.ParseSyntaxTree(File.ReadAllText(file)));






    }
}