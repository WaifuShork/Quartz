using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax(SyntaxTree syntaxTree,
                                  SyntaxToken keyword, 
                                  SyntaxToken openParenthesisToken,
                                  SyntaxToken identifier, 
                                  SyntaxToken equalsToken, 
                                  ExpressionSyntax lowerBound, 
                                  SyntaxToken toKeyword, 
                                  ExpressionSyntax upperBound, 
                                  SyntaxToken closeParenthesisToken,
                                  StatementSyntax body)
                                  : base(syntaxTree)
        {
            Keyword = keyword;
            OpenParenthesisToken = openParenthesisToken;
            Identifier = identifier;
            EqualsToken = equalsToken;
            LowerBound = lowerBound;
            ToKeyword = toKeyword;
            UpperBound = upperBound;
            CloseParenthesisToken = closeParenthesisToken;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.ForStatement;
        
        public SyntaxToken Keyword { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax LowerBound { get; }
        public SyntaxToken ToKeyword { get; }
        public ExpressionSyntax UpperBound { get; }
        public SyntaxToken CloseParenthesisToken { get; }
        public StatementSyntax Body { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
            yield return OpenParenthesisToken;
            yield return Identifier;
            yield return EqualsToken;
            yield return LowerBound;
            yield return ToKeyword;
            yield return UpperBound;
            yield return CloseParenthesisToken;
            yield return Body;
        }
    }
}