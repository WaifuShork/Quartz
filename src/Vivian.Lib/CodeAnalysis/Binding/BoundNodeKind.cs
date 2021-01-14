namespace Vivian.CodeAnalysis.Binding
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
        CallExpression,
        ConversionExpression,

        // Statements 
        BlockStatement,
        ExpressionStatement,
        IfStatement,
        DoWhileStatement,
        WhileStatement,
        ForStatement,
        VariableDeclaration,
        GotoStatement,
        LabelStatement,
        ConditionalGotoStatement,
        ReturnStatement
    }
}