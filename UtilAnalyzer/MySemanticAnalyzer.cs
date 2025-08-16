using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SampleAnalyzer
{

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MySemanticAnalyzer : DiagnosticAnalyzer
    {
        
        private static readonly DiagnosticDescriptor Rule = new(
            "IH0001",
            "Override method must call base method",
            "Override method must call base method",
            "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);


        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            //avoid generated code
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            //enable concurrent
            context.EnableConcurrentExecution();

            context.RegisterOperationBlockStartAction(AnalyzeOperation);
        }

        private void AnalyzeOperation(OperationBlockStartAnalysisContext context)
        {
            if (context.OwningSymbol is not IMethodSymbol methodSymbol)
            {
                return;
            }
            
            //check whether the method is an override method
            //check if the base method has the attribute
            //if yes, check if this method call the base method
            //if no, report error

            bool baseMethodHasAttribute(IMethodSymbol methodSymbol, string name)
            {
                var methodRef = methodSymbol;
                
                while (methodRef.IsOverride && methodRef.OverriddenMethod is not null)
                {
                    methodRef = methodRef.OverriddenMethod;
                }
                
                return hasAttributes(methodRef.GetAttributes(), name);
            }
            
            if (methodSymbol.MethodKind != MethodKind.Ordinary 
                || !methodSymbol.IsOverride
                || methodSymbol.OverriddenMethod is null
                || !baseMethodHasAttribute(methodSymbol, "MustCallBaseAttribute")
               )
                return;
            
            bool calledBase = false;

            context.RegisterOperationAction(analysisContext =>
            {
                if (SymbolEqualityComparer.Default.Equals(((IInvocationOperation)analysisContext.Operation).TargetMethod, methodSymbol.OverriddenMethod))
                {
                    calledBase = true;
                }

            }, OperationKind.Invocation);
            
            context.RegisterOperationBlockEndAction(endContext =>
            {
                if (!calledBase)
                {
                    if (methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is MethodDeclarationSyntax decl)
                    {
                        endContext.ReportDiagnostic(Diagnostic.Create(Rule, decl.Identifier.GetLocation(), methodSymbol.Name));
                    }
                }
            });

            bool hasAttributes(ImmutableArray<AttributeData> attributeData, string name)
            {
                foreach (var attr in attributeData)
                {
                    if (attr.AttributeClass is null) continue;
                    if (attr.AttributeClass.Name == name) return true;
                }

                return false;
            }
        }
        

    }
    
    

}