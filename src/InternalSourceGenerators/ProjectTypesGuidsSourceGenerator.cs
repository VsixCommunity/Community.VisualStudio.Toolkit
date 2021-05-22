﻿using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace InternalSourceGenerators
{
    [Generator]
    public class ProjectTypesGuidsSourceGenerator : ISourceGenerator
    {
        private const string _projecTypesTypeName = "EnvDTE.ProjectTypes";

        private static readonly DiagnosticDescriptor _projectTypesNotFoundDiagnostic = new(
            "ISG001",
            $"{_projecTypesTypeName} was not found",
            $"Project type Guids could not be generated because the type '{_projecTypesTypeName}' was not found.",
            "CodeGeneration",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        public void Initialize(GeneratorInitializationContext context)
        {
            // Nothing to do here.
        }

        public void Execute(GeneratorExecutionContext context)
        {
            INamedTypeSymbol? projectTypesClass = context.Compilation.GetTypeByMetadataName(_projecTypesTypeName);

            if (projectTypesClass is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(_projectTypesNotFoundDiagnostic, null));
                return;
            }

            StringBuilder source = new();

            source.AppendLine("// <auto-generated/>");
            source.AppendLine("using System;");
            source.AppendLine("namespace EnvDTE {");
            source.AppendLine("partial class ProjectTypes {");

            // Use the same XML documentation that's on the `ProjectTypes` class.
            source.AppendLine(projectTypesClass.GetLeadingTriviaFrom<ClassDeclarationSyntax>(context.CancellationToken));
            source.AppendLine("public static class Guids {");

            foreach (IFieldSymbol field in projectTypesClass.GetMembers().OfType<IFieldSymbol>().Where(x => x.HasConstantValue))
            {
                // Use the same XML documentation that's on the corresponding constant.
                source.AppendLine(field.GetLeadingTriviaFrom<FieldDeclarationSyntax>(context.CancellationToken));
                source.AppendLine($"public static readonly Guid {field.Name} = new Guid(\"{field.ConstantValue}\");");
            }

            source.AppendLine("}"); // class Guids.
            source.AppendLine("}"); // class ProjecTypes.
            source.AppendLine("}"); // namespace EnvDte.

            context.AddSource("ProjectTypes.Guids.cs", SourceText.From(source.ToString(), encoding: Encoding.UTF8));
        }
    }
}
