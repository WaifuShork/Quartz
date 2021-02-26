using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public sealed partial class StructDeclarationSyntax : MemberSyntax
        {
            internal StructDeclarationSyntax(SyntaxTree syntaxTree, SyntaxToken structKeyword, SyntaxToken identifier, MemberBlockStatementSyntax body)
            : base(syntaxTree)
            {
                StructKeyword = structKeyword;
                Identifier = identifier;
                Body = body;
            }
    
            public override SyntaxKind Kind => SyntaxKind.StructDeclaration;
            public override IEnumerable<SyntaxNode> GetChildren()
            {
                yield return StructKeyword;
                yield return Identifier;
                yield return Body;
            }

            public SyntaxToken StructKeyword { get; }
            public SyntaxToken Identifier { get; }
            public MemberBlockStatementSyntax Body { get; }
        }
}