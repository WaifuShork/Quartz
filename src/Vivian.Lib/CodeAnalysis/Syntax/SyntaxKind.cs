namespace Vivian.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // Tokens 
        BadToken,
        EndOfFileToken,
        WhitespaceToken,        
        CommaToken,

        // Data Tokens
        StringToken,
        NumberToken,

        // Operator Tokens
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        
        BangToken,
        OpenParenthesisToken,
        CloseParenthesisToken,
        IdentifierToken,
        EqualsToken,
        OpenBraceToken,
        CloseBraceToken,
        
        // Bitwise Tokens
        TildeToken,
        AmpersandToken,
        PipeToken,
        HatToken,

        // Comparison Tokens
        AmpersandAmpersandToken,
        PipePipeToken,
        EqualsEqualsToken,
        BangEqualsToken,
        GreaterToken,
        LessOrEqualsToken,
        LessToken,
        GreaterOrEqualsToken,

        // Keywords 
        FalseKeyword,
        TrueKeyword,
        LetKeyword,
        ImplyKeyword,
        IfKeyword,
        ElseKeyword,        
        WhileKeyword,        
        ForKeyword,
        ToKeyword,

        // Expressions
        LiteralExpression,
        NameExpression,
        UnaryExpression,
        BinaryExpression,
        ParenthesizedExpression,
        AssignmentExpression,
        CallExpression,
        
        // Nodes
        CompilationUnit,
        
        // Statements
        BlockStatement,
        VariableDeclaration,
        ExpressionStatement,
        IfStatement,
        ElseClause,
        WhileStatement,
        ForStatement,
    }
}