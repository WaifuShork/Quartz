﻿using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed class ExpressionStatementSyntax : StatementSyntax
    {
        internal ExpressionStatementSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression)
            : base(syntaxTree)
        {
            Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
        
        public ExpressionSyntax Expression { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
        }
    }
}