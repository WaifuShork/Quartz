using Vivian.CodeAnalysis.Symbols;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken literalToken) : this(syntaxTree, literalToken, literalToken.Value, literalToken.Type)
        {
        }
        public LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken literalToken, object value, TypeSymbol type) : base(syntaxTree)
        {
            LiteralToken = literalToken;
            Value = value;
            Type = type;
        }
        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

        public SyntaxToken LiteralToken { get; }
        public object Value { get; }
        public TypeSymbol Type { get; }
    }
}