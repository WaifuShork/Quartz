namespace Vivian.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Statements
        BlockStatement,
        ConditionalGotoStatement,
        DoWhileStatement,
        ExpressionStatement,
        ForStatement,
        GotoStatement,
        IfStatement,
        LabelStatement,
        MemberBlockStatement,
        NopStatement,
        ReturnStatement,
        SequencePointStatement,
        VariableDeclaration,
        WhileStatement,

        // Expressions
        AssignmentExpression,
        BinaryExpression,
        CallExpression,
        CompoundAssignmentExpression,
        CompoundFieldAssignmentExpression,
        ConversionExpression,
        ErrorExpression,
        FieldAccessExpression,
        FieldAssignmentExpression,
        LiteralExpression,
        ThisExpression,
        UnaryExpression,
        VariableExpression,
    }
}