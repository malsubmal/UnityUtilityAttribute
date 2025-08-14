using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SampleAnalyzer
{
    /// <summary>
    /// A sample analyzer that reports invalid values being used for the 'speed' parameter of the 'SetSpeed' function.
    /// To make sure that we analyze the method of the specific class, we use semantic analysis instead of the syntax tree, so this analyzer will not work if the project is not compilable.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MySemanticAnalyzer : DiagnosticAnalyzer
    {
        
        private static readonly DiagnosticDescriptor Rule = new(
            "CS1001",
            "Override method must call base method",
            "Override method must call base method",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);


        // Keep in mind: you have to list your rules here.
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            // You must call this method to avoid analyzing generated code.
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // You must call this method to enable the Concurrent Execution.
            context.EnableConcurrentExecution();

            context.RegisterOperationBlockStartAction(AnalyzeOperation);


            // Check other 'context.Register...' methods that might be helpful for your purposes.
        }

        /// <summary>
        /// Executed on the completion of the semantic analysis associated with the Invocation operation.
        /// </summary>
        /// <param name="context">Operation context.</param>
        private void AnalyzeOperation(OperationBlockStartAnalysisContext context)
        {
            var methodSymbol = context.OwningSymbol as IMethodSymbol;

            if (methodSymbol is null)
            {
                return;
            }
            //check whether the method is an override method
            //check if the base method has the attribute
            //if yes, check if this method call the base method
            //if no, report error
            
            bool hasAttributes(ImmutableArray<AttributeData> attributeData, string name)
            {
                foreach (var attr in attributeData)
                {
                    if (attr.AttributeClass is null) continue;
                    if (attr.AttributeClass.Name == name) return true;
                }

                return false;
            }

            bool baseMethodHasAttribute(IMethodSymbol methodSymbol, string name)
            {
                var methodRef = methodSymbol;
                
                while (methodRef.IsOverride && methodRef.OverriddenMethod is not null)
                {
                    methodRef = methodRef.OverriddenMethod;
                }
                
                return hasAttributes(methodRef.GetAttributes(), name);
            }
            
            if (methodSymbol.MethodKind != MethodKind.Ordinary ||
                !methodSymbol.IsOverride
                || methodSymbol.OverriddenMethod is null
                //|| //get the base method of the method
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
                    var decl = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
                    if (decl != null)
                    {
                        endContext.ReportDiagnostic(Diagnostic.Create(Rule, decl.Identifier.GetLocation(), methodSymbol.Name));
                    }
                }
            });
        }
        

    }
    
    

}