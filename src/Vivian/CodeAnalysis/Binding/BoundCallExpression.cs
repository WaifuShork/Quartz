using System.Collections.Immutable;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Syntax;

namespace Vivian.CodeAnalysis.Binding
{
    internal sealed class BoundCallExpression : BoundExpression
    {
        public BoundCallExpression(SyntaxNode syntax, FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
            : base(syntax)
        {
            Function = function;
            Arguments = arguments;
        }

        public BoundCallExpression(SyntaxNode syntax, BoundVariableExpression instance, FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
            : this (syntax, function, arguments)
        {
            Instance = instance;
        }

        public BoundCallExpression(SyntaxNode syntax, BoundFieldAccessExpression instance, FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
            : this (syntax, function, arguments)
        {
            Instance = instance;
        }

        public BoundCallExpression(SyntaxNode syntax, BoundThisExpression instance, FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
            :this (syntax, function, arguments)
        {
            Instance = instance;
        }

        public override BoundNodeKind Kind => BoundNodeKind.CallExpression;
        public override TypeSymbol Type => Function.ReturnType;
        public BoundExpression? Instance { get; }
        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
    }
}