using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class ForStatementSyntax : StatementSyntax
    {
        internal ForStatementSyntax(SyntaxTree syntaxTree, SyntaxToken keyword, SyntaxToken identifier, SyntaxToken equalsToken, ExpressionSyntax lowerBound, SyntaxToken toKeyword, ExpressionSyntax upperBound, StatementSyntax body)
            : base(syntaxTree)
        {
            Keyword = keyword;
            Identifier = identifier;
            EqualsToken = equalsToken;
            LowerBound = lowerBound;
            ToKeyword = toKeyword;
            UpperBound = upperBound;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.ForStatement;
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
            yield return Identifier;
            yield return EqualsToken;
            yield return LowerBound;
            yield return ToKeyword;
            yield return UpperBound;
            yield return Body;
        }

        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax LowerBound { get; }
        public SyntaxToken ToKeyword { get; }
        public ExpressionSyntax UpperBound { get; }
        public StatementSyntax Body { get; }
    }
}