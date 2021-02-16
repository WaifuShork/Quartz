using System;
using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.BangToken:    
                case SyntaxKind.TildeToken:    
                    return 6;

                default:
                    return 0;
            }
        }
        
        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                case SyntaxKind.ModuloToken:
                    return 5;
                
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 4;
                
                case SyntaxKind.EqualsEqualsToken:
                case SyntaxKind.BangEqualsToken:
                case SyntaxKind.LessToken:
                case SyntaxKind.GreaterToken:
                case SyntaxKind.LessOrEqualsToken:
                case SyntaxKind.GreaterOrEqualsToken:
                    return 3;
                
                case SyntaxKind.AmpersandToken:
                case SyntaxKind.AmpersandAmpersandToken:
                    return 2;
                
                case SyntaxKind.PipeToken:
                case SyntaxKind.PipePipeToken:
                case SyntaxKind.HatToken:
                    return 1;
                
                default:
                    return 0;
            }
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "const":
                    return SyntaxKind.ConstKeyword;
                case "var":
                    return SyntaxKind.VarKeyword;
                case "true":
                    return SyntaxKind.TrueKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                
                case "if":
                    return SyntaxKind.IfKeyword;
                case "else":
                    return SyntaxKind.ElseKeyword;
                case "do":
                    return SyntaxKind.DoKeyword;
                case "while":
                    return SyntaxKind.WhileKeyword;
                
                case "function":
                    return SyntaxKind.FunctionKeyword;
                
                case "for":
                    return SyntaxKind.ForKeyword;
                
                case "to":
                    return SyntaxKind.ToKeyword;
                
                case "break":
                    return SyntaxKind.BreakKeyword;
                
                case "continue":
                    return SyntaxKind.ContinueKeyword;
                
                case "return":
                    return SyntaxKind.ReturnKeyword;
                
                default:
                    return SyntaxKind.IdentifierToken;
            }
        }
        
        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                if (GetBinaryOperatorPrecedence(kind) > 0)
                    yield return kind;
            }
        }
        
        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
        {
            var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));
            foreach (var kind in kinds)
            {
                if (GetUnaryOperatorPrecedence(kind) > 0)
                    yield return kind;
            }
        }
        
        public static string? GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    return "+"; 
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.StarToken:
                    return "*";
                case SyntaxKind.ModuloToken:
                    return "%";
                case SyntaxKind.SlashToken:
                    return "/";
                case SyntaxKind.BangToken:
                    return "!";
                case SyntaxKind.AmpersandAmpersandToken:
                    return "&&";
                case SyntaxKind.PipePipeToken:
                    return "||";
                
                case SyntaxKind.AmpersandToken:
                    return "&";
                case SyntaxKind.PipeToken:
                    return "|";
                case SyntaxKind.TildeToken:
                    return "~";
                case SyntaxKind.HatToken:
                    return "^";

                case SyntaxKind.EqualsEqualsToken:
                    return "==";
                case SyntaxKind.BangEqualsToken:
                    return "!=";
                
                case SyntaxKind.GreaterOrEqualsToken:
                    return ">=";
                case SyntaxKind.GreaterToken:
                    return ">";
                
                case SyntaxKind.LessOrEqualsToken:
                    return "<=";
                case SyntaxKind.LessToken:
                    return "<";
                
                case SyntaxKind.OpenParenthesisToken:
                    return "(";
                case SyntaxKind.CloseParenthesisToken:
                    return ")";
                
                case SyntaxKind.ColonToken:
                    return ":"; 
                
                case SyntaxKind.OpenBraceToken:
                    return "{";
                case SyntaxKind.CloseBraceToken:
                    return "}";
                    
                case SyntaxKind.OpenBracketToken:
                    return "[";
                case SyntaxKind.CloseBracketToken:
                    return "]";
                
                case SyntaxKind.SemicolonToken:
                    return ";";

                case SyntaxKind.EqualsToken:
                    return "=";
                case SyntaxKind.FalseKeyword:
                    return "false";
                case SyntaxKind.TrueKeyword:
                    return "true";
                case SyntaxKind.ConstKeyword:
                    return "const";
                case SyntaxKind.VarKeyword:
                    return "var";
                
                case SyntaxKind.IfKeyword:
                    return "if";
                case SyntaxKind.ElseKeyword:
                    return "else";
                
                case SyntaxKind.FunctionKeyword:
                    return "function";
                
                case SyntaxKind.DoKeyword:
                    return "do";
                case SyntaxKind.WhileKeyword:
                    return "while";
                
                case SyntaxKind.ForKeyword:
                    return "for";
                
                case SyntaxKind.ToKeyword:
                    return "to";
                
                case SyntaxKind.CommaToken:
                    return ",";
                
                case SyntaxKind.BreakKeyword:
                    return "break";
                case SyntaxKind.ContinueKeyword:
                    return "continue";
                
                case SyntaxKind.ReturnKeyword:
                    return "return";

                default:
                    return null;
            }
        }

        public static bool IsKeyword(this SyntaxKind kind)
        {
            return kind.ToString().EndsWith("Keyword");
        }
        
        public static bool IsToken(this SyntaxKind kind)
        {
            return !kind.IsTrivia() &&
                   (kind.IsKeyword() || kind.ToString().EndsWith("Token"));
        }
        
        public static bool IsTrivia(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.WhitespaceTrivia: 
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.SkippedTextTrivia:
                case SyntaxKind.LineBreakTrivia:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsComment(this SyntaxKind kind)
        {
            return kind == SyntaxKind.SingleLineCommentTrivia ||
                   kind == SyntaxKind.MultiLineCommentTrivia;
        }
    }
}