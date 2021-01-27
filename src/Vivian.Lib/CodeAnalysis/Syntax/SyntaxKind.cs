namespace Vivian.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // Tokens 
        BadToken,
        EndOfFileToken,
        WhitespaceToken,        
        CommaToken,
        ColonToken,
        SemicolonToken,

        // Data Tokens
        StringToken,
        NumberToken,

        // Operator Tokens
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        ModuloToken,

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
        ConstKeyword,
        VarKeyword,
        IfKeyword,
        ElseKeyword,        
        WhileKeyword,        
        ForKeyword,
        ToKeyword,
        DoKeyword,
        ContinueKeyword,
        BreakKeyword,
        ReturnKeyword,
        FunctionKeyword,

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
        TypeClause,

        // Statements
        BlockStatement,
        VariableDeclaration,
        ExpressionStatement,
        IfStatement,
        ElseClause,
        WhileStatement,
        ForStatement,
        DoWhileStatement,
        GlobalStatement,
        FunctionDeclaration,
        Parameter,
        ContinueStatement,
        BreakStatement,
        ReturnStatement
    }
}