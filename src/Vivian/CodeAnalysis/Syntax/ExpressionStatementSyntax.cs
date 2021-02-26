using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class ExpressionStatementSyntax : StatementSyntax
        {
            internal ExpressionStatementSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression)
                : base(syntaxTree)
            {
                Expression = expression;
            }
    
            public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
            public override IEnumerable<SyntaxNode> GetChildren()
            {
                yield return Expression;
            }

            public ExpressionSyntax Expression { get; }
        }
}