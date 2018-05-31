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
using Wirehome.Core.Utils;
using Wirehome.ComponentModel.Adapters;
using Wirehome.Model.Core;
using Microsoft.CodeAnalysis.Emit;

namespace Wirehome.Core.Services.Roslyn
{
    public class RoslynAsseblyGenerator
    {
        public void CompileAssemblies(string sourceDictionary, bool generatePdb = false)
        {
            var assemblies = new List<Result<string>>();
            var modelAssemblies = AssemblyHelper.GetReferencedAssemblies(typeof(Adapter));
            var servicesAssemblies = AssemblyHelper.GetReferencedAssemblies(typeof(WirehomeController));
            var references = modelAssemblies.Union(servicesAssemblies).Distinct();

            foreach (string adapterDictionary in Directory.GetDirectories(sourceDictionary))
            {
                GenerateAssembly(Path.GetFileName(adapterDictionary), adapterDictionary, references, generatePdb);
            }
        }

        public Result<string> GenerateAssembly(string adapterName, string sourceDictionary, IEnumerable<string> dependencies, bool generatePdb = false)
        {
            var syntaxTrees = ParseSourceCode(sourceDictionary);
            var references = ParseDependencies(dependencies);
            var assemblyName = $"{adapterName}.dll";

            var compilation = CSharpCompilation.Create(assemblyName)
                                               .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                                               .AddReferences(references)
                                               .AddSyntaxTrees(syntaxTrees);

            var path = Path.Combine(sourceDictionary, assemblyName);
            var pdbPath = generatePdb ? Path.Combine(sourceDictionary, assemblyName) : null;

            var compilationResult = compilation.Emit(path, pdbPath: pdbPath);

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