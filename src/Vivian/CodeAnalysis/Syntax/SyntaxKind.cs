namespace Vivian.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        BadToken,

        // Trivia
        LineBreakTrivia,
        MultiLineCommentTrivia,
        SingleLineCommentTrivia,
        SkippedTextTrivia,
        WhitespaceTrivia,

        // Tokens
        PlusEqualsToken,
        MinusEqualsToken,
        AmpersandEqualsToken,
        SlashEqualsToken,
        StarEqualsToken,
        PercentEqualsToken,
        
        AmpersandAmpersandToken,
        AmpersandToken,
        EqualsEqualsToken,
        EqualsGreaterThanToken,
        BangEqualsToken,
        GreaterOrEqualsToken,
        GreaterToken,
        HatEqualsToken,
        HatToken,
        LessOrEqualsToken,
        LessToken,
        PipeEqualsToken,
        PipePipeToken,
        PipeToken, 
        BangToken,
        
        CharToken,
        ColonToken,
        CommaToken,
        DotToken,        
        NumberToken,
        SemicolonToken,
        EndOfFileToken,
        EqualsToken,
        IdentifierToken,
        
        OpenBraceToken,
        OpenParenthesisToken,
        CloseBraceToken,
        CloseParenthesisToken,
        
        PlusToken,
        MinusToken,
        PercentToken,
        SlashToken,
        StarToken,
        StringToken,
        TildeToken,

        // Keywords
        BreakKeyword,
        ContinueKeyword,
        DefaultKeyword,
        DoKeyword,
        ElseKeyword,
        FalseKeyword,
        ForKeyword,
        FunctionKeyword,
        IfKeyword,
        ConstKeyword,
        ReturnKeyword,
        ClassKeyword,
        ThisKeyword,
        ToKeyword,
        TrueKeyword,
        VarKeyword,
        WhileKeyword,        

        // Nodes
        CompilationUnit,
        ElseClause,
        FunctionDeclaration,
        GlobalStatement,
        Parameter,
        ClassDeclaration,
        TypeClause,

        // Statements
        BlockStatement,
        BreakStatement,
        ContinueStatement,
        DoWhileStatement,
        ExpressionStatement,
        ForStatement,
        IfStatement,
        MemberBlockStatement,
        ReturnStatement,
        VariableDeclaration,
        WhileStatement,

        // Expressions
        AssignmentExpression,
        BinaryExpression,
        CallExpression,
        CompoundAssignmentExpression,
        LiteralExpression,
        MemberAccessExpression,
        NameExpression,
        ParenthesizedExpression,
        UnaryExpression,
        NamespaceDeclaration,
    }
}