using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class ReturnStatementSyntax : StatementSyntax
    {
        public ReturnStatementSyntax(SyntaxTree syntaxTree, SyntaxToken returnKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken) : base(syntaxTree)
        {
            ReturnKeyword = returnKeyword;
            Expression = expression;
            SemicolonToken = semicolonToken;
        }

        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
        
        public SyntaxToken ReturnKeyword { get; }
        public ExpressionSyntax? Expression { get; }
        public SyntaxToken SemicolonToken { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ReturnKeyword;
            yield return Expression!;
            yield return SemicolonToken;
        }
    }
}