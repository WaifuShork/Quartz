namespace Vivian.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // Trivia
        WhitespaceTrivia,        
        SingleLineCommentTrivia,
        MultiLineCommentTrivia,        
        BadTokenTrivia,
        
        // Tokens 
        EndOfFileToken,
        CommaToken,
        ColonToken,
        SemicolonToken,

        StringToken,
        NumberToken,

        // Operator Tokens
        PlusToken,
        MinusToken,
        StarToken,
        SlashToken,
        ModuloToken,

        BangToken,
        IdentifierToken,

        OpenParenthesisToken,
        CloseParenthesisToken,

        EqualsToken,
        OpenBraceToken,
        CloseBraceToken,
        
        OpenBracketToken,
        CloseBracketToken,
        
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
        ArrayKeyword,

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