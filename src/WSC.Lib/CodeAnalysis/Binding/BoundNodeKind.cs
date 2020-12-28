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
        VariableDeclaration,
        
        // Statements 
        BlockStatement,
        ExpressionStatement,
        IfStatement,
        WhileStatement
    }
}