using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class IfStatementSyntax : StatementSyntax
    {
        internal IfStatementSyntax(SyntaxTree syntaxTree, SyntaxToken ifKeyword, ExpressionSyntax condition, StatementSyntax thenStatement, ElseClauseSyntax? elseClause)
            : base(syntaxTree)
        {
            IfKeyword = ifKeyword;
            Condition = condition;
            ThenStatement = thenStatement;
            ElseClause = elseClause;
        }

        public override SyntaxKind Kind => SyntaxKind.IfStatement;
        

        public SyntaxToken IfKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax ThenStatement { get; }
        public ElseClauseSyntax? ElseClause { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IfKeyword;
            yield return Condition;
            yield return ThenStatement;
            yield return ElseClause!;
        }
    }
}