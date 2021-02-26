using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class ElseClauseSyntax : SyntaxNode
        {
            internal ElseClauseSyntax(SyntaxTree syntaxTree, SyntaxToken elseKeyword, StatementSyntax elseStatement)
                : base(syntaxTree)
            {
                ElseKeyword = elseKeyword;
                ElseStatement = elseStatement;
            }
    
            public override SyntaxKind Kind => SyntaxKind.ElseClause;
            public override IEnumerable<SyntaxNode> GetChildren()
            {
                yield return ElseKeyword;
                yield return ElseStatement;
            }

            public SyntaxToken ElseKeyword { get; }
            public StatementSyntax ElseStatement { get; }
        }
}