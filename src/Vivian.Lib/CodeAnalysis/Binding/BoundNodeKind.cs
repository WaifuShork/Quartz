namespace wsc.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Expressions
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression,
        ErrorExpression,

        // Statements 
        BlockStatement,
        ExpressionStatement,
        IfStatement,
        WhileStatement,
        ForStatement,
        VariableDeclaration,
        GotoStatement,
        LabelStatement,
        ConditionalGotoStatement,
    }
}