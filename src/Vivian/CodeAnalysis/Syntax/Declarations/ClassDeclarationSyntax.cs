using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    // TODO: Classes need to be able to hold functions, properties, and fields
    public sealed class ClassDeclarationSyntax : MemberSyntax
    {
        internal ClassDeclarationSyntax(SyntaxTree syntaxTree, SyntaxToken classKeyword, SyntaxToken identifier, MemberBlockStatementSyntax body)
        : base(syntaxTree)
        {
            ClassKeyword = classKeyword;
            Identifier = identifier;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.ClassDeclaration;
        
        public SyntaxToken ClassKeyword { get; }
        public SyntaxToken Identifier { get; }
        public MemberBlockStatementSyntax Body { get; }
        
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ClassKeyword;
            yield return Identifier;
            yield return Body;
        }
    }
}