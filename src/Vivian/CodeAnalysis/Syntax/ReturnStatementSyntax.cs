using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class ReturnStatementSyntax : StatementSyntax
    {
        internal ReturnStatementSyntax(SyntaxTree syntaxTree, SyntaxToken returnKeyword, ExpressionSyntax? expression)
            : base(syntaxTree)
        {
            ReturnKeyword = returnKeyword;
            Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ReturnKeyword;
            yield return Expression;
        }

        public SyntaxToken ReturnKeyword { get; }
        public ExpressionSyntax? Expression { get; }
    }
}