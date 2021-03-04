using System;
using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetPostfixOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.DotToken:
                    return 7;
                default:
                    return 0;
            }
        }

        public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PlusToken or 
                     SyntaxKind.MinusToken or
                     SyntaxKind.BangToken or 
                     SyntaxKind.TildeToken:
                    return 6;
                default:
                    return 0;
            }
        }

        public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.StarToken or 
                     SyntaxKind.SlashToken or
                     SyntaxKind.PercentToken:
                    return 6;
                
                case SyntaxKind.PlusToken or 
                     SyntaxKind.MinusToken:
                    
                case SyntaxKind.EqualsEqualsToken or 
                     SyntaxKind.BangEqualsToken or 
                     SyntaxKind.LessToken or
                     SyntaxKind.LessOrEqualsToken or 
                     SyntaxKind.GreaterToken or 
                     SyntaxKind.GreaterOrEqualsToken:
                     return 4;
                
                case SyntaxKind.AmpersandToken or 
                     SyntaxKind.AmpersandAmpersandToken:
                    return 3;
                
                case SyntaxKind.PipeToken or 
                     SyntaxKind.PipePipeToken or 
                     SyntaxKind.HatToken:
                    return 2;
                
                case SyntaxKind.PlusEqualsToken or 
                     SyntaxKind.MinusEqualsToken or 
                     SyntaxKind.StarEqualsToken or
                     SyntaxKind.SlashEqualsToken or 
                     SyntaxKind.AmpersandEqualsToken or 
                     SyntaxKind.PipeEqualsToken or
                     SyntaxKind.HatEqualsToken or 
                     SyntaxKind.EqualsToken:
                    return 1;
                default:
                    return 0;
            }
        }

        public static bool IsComment(this SyntaxKind kind)
        {
            return kind == SyntaxKind.SingleLineCommentTrivia ||
                   kind == SyntaxKind.MultiLineCommentTrivia;
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "break":
                    return SyntaxKind.BreakKeyword;
                case "continue":
                    return SyntaxKind.ContinueKeyword;
                case "default":
                    return SyntaxKind.DefaultKeyword;
                case "do":
                    return SyntaxKind.DoKeyword;
                case "else":
                    return SyntaxKind.ElseKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                case "for":
                    return SyntaxKind.ForKeyword;
                case "new":
                    return SyntaxKind.FunctionKeyword;
                case "if":
                    return SyntaxKind.IfKeyword;
                case "const":
                    return SyntaxKind.ConstKeyword;
                case "return":
                    return SyntaxKind.ReturnKeyword;
                case "struct":
                    return SyntaxKind.StructKeyword;
                case "this":
                    return SyntaxKind.ThisKeyword;
                case "to":
                    return SyntaxKind.ToKeyword;
                case "true":
                    return SyntaxKind.TrueKeyword;
                case "var":
                    return SyntaxKind.VarKeyword;
                case "while":
                    return SyntaxKind.WhileKeyword;
                default:
                    return SyntaxKind.IdentifierToken;
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

        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            var kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));

            foreach (var kind in kinds)
            {
                if (GetBinaryOperatorPrecedence(kind) > 0)
                    yield return kind;
            }
        }

        public static string? GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.OpenBraceToken:
                    return "{";
                case SyntaxKind.CloseBraceToken:
                    return "}";
                case SyntaxKind.OpenParenthesisToken:
                    return "(";
                case SyntaxKind.CloseParenthesisToken:
                    return ")";
                
                case SyntaxKind.SemicolonToken:
                    return ";";
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.SlashToken:
                    return "/";
                case SyntaxKind.StarToken:
                    return "*";
                case SyntaxKind.TildeToken:
                    return "~";
                case SyntaxKind.BangToken:
                    return "!";
                case SyntaxKind.ColonToken:
                    return ":";
                case SyntaxKind.EqualsGreaterThanToken:
                    return "=>";
                case SyntaxKind.CommaToken:
                    return ",";
                case SyntaxKind.DotToken:
                    return ".";
                case SyntaxKind.EqualsToken:
                    return "=";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.HatToken:
                    return "^";
                case SyntaxKind.PercentToken:
                    return "%";
                
                case SyntaxKind.BangEqualsToken:
                    return "!=";
                case SyntaxKind.EqualsEqualsToken:
                    return "==";
                case SyntaxKind.GreaterOrEqualsToken:
                    return ">=";
                case SyntaxKind.GreaterToken:
                    return ">";
                case SyntaxKind.AmpersandAmpersandToken:
                    return "&&";
                case SyntaxKind.AmpersandToken:
                    return "&";
                case SyntaxKind.PipeEqualsToken:
                    return "|=";
                case SyntaxKind.PipePipeToken:
                    return "||";
                case SyntaxKind.PipeToken:
                    return "|";
                
                case SyntaxKind.AmpersandEqualsToken:
                    return "&=";
                case SyntaxKind.HatEqualsToken:
                    return "^=";
                case SyntaxKind.LessOrEqualsToken:
                    return "<=";
                case SyntaxKind.LessToken:
                    return "<";
                case SyntaxKind.MinusEqualsToken:
                    return "-=";
                case SyntaxKind.PlusEqualsToken:
                    return "+=";
                case SyntaxKind.SlashEqualsToken:
                    return "/=";
                case SyntaxKind.StarEqualsToken:
                    return "*=";
                
                case SyntaxKind.BreakKeyword:
                    return "break";
                case SyntaxKind.ContinueKeyword:
                    return "continue";
                
                case SyntaxKind.DoKeyword:
                    return "do";
                case SyntaxKind.WhileKeyword:
                    return "while";

                case SyntaxKind.IfKeyword:
                    return "if";
                case SyntaxKind.ElseKeyword:
                    return "else";
                
                case SyntaxKind.ReturnKeyword:
                    return "return";
                
                case SyntaxKind.StructKeyword:
                    return "struct";
                case SyntaxKind.ThisKeyword:
                    return "this";
                
                case SyntaxKind.ForKeyword:
                    return "for";
                case SyntaxKind.ToKeyword:
                    return "to";
                
                case SyntaxKind.TrueKeyword:
                    return "true";
                case SyntaxKind.FalseKeyword:
                    return "false";
                
                case SyntaxKind.VarKeyword:
                    return "var";
                case SyntaxKind.ConstKeyword:
                    return "const";
                
                case SyntaxKind.FunctionKeyword:
                    return "new";
                case SyntaxKind.DefaultKeyword:
                    return "default";
                
                default:
                    return null;
            }
        }

        public static bool IsTrivia(this SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.LineBreakTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.SkippedTextTrivia:
                case SyntaxKind.WhitespaceTrivia:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsAssignmentOperator(this SyntaxKind kind)
        {
            return kind == SyntaxKind.PlusEqualsToken || 
                   kind == SyntaxKind.MinusEqualsToken || 
                   kind == SyntaxKind.StarEqualsToken || 
                   kind == SyntaxKind.SlashEqualsToken || 
                   kind == SyntaxKind.AmpersandEqualsToken || 
                   kind == SyntaxKind.PipeEqualsToken ||
                   kind == SyntaxKind.HatEqualsToken || 
                   kind == SyntaxKind.EqualsToken;
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
        public static SyntaxKind GetBinaryOperatorOfAssignmentOperator(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.AmpersandEqualsToken:
                    return SyntaxKind.AmpersandToken;
                case SyntaxKind.HatEqualsToken:
                    return SyntaxKind.HatToken;
                case SyntaxKind.MinusEqualsToken:
                    return SyntaxKind.MinusToken;
                case SyntaxKind.PipeEqualsToken:
                    return SyntaxKind.PipeToken;
                case SyntaxKind.PlusEqualsToken:
                    return SyntaxKind.PlusToken;
                case SyntaxKind.SlashEqualsToken:
                    return SyntaxKind.SlashToken;
                case SyntaxKind.StarEqualsToken:
                    return SyntaxKind.StarToken;
                default:
                    throw new Exception($"Unexpected syntax: '{kind}'");
            }
        }
    }
}