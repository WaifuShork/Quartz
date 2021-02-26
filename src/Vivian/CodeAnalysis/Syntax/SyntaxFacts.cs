using System;
using System.Collections.Generic;

namespace Vivian.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
        {
            public static int GetPostfixOperatorPrecedence(this SyntaxKind kind)
            {
                return kind switch
                {
                    SyntaxKind.DotToken => 7,
                    _ => 0,
                };
            }
    
            public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
            {
                return kind switch
                {
                    SyntaxKind.PlusToken or SyntaxKind.MinusToken or SyntaxKind.BangToken or SyntaxKind.TildeToken => 6,
                    _ => 0,
                };
            }
    
            public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
            {
                return kind switch
                {
                    SyntaxKind.StarToken or SyntaxKind.SlashToken => 6,
                    SyntaxKind.PlusToken or SyntaxKind.MinusToken => 4,
                    SyntaxKind.EqualsEqualsToken or SyntaxKind.BangEqualsToken or SyntaxKind.LessToken or SyntaxKind.LessOrEqualsToken or SyntaxKind.GreaterToken or SyntaxKind.GreaterOrEqualsToken => 4,
                    SyntaxKind.AmpersandToken or SyntaxKind.AmpersandAmpersandToken => 3,
                    SyntaxKind.PipeToken or SyntaxKind.PipePipeToken or SyntaxKind.HatToken => 2,
                    SyntaxKind.PlusEqualsToken or SyntaxKind.MinusEqualsToken or SyntaxKind.StarEqualsToken or SyntaxKind.SlashEqualsToken or SyntaxKind.AmpersandEqualsToken or SyntaxKind.PipeEqualsToken or SyntaxKind.HatEqualsToken or SyntaxKind.EqualsToken => 1,
                    _ => 0,
                };
            }
    
            public static bool IsComment(this SyntaxKind kind)
            {
                return kind == SyntaxKind.SingleLineCommentTrivia ||
                       kind == SyntaxKind.MultiLineCommentTrivia;
            }
    
            public static SyntaxKind GetKeywordKind(string text)
            {
                return text switch
                {
                    "break" => SyntaxKind.BreakKeyword,
                    "continue" => SyntaxKind.ContinueKeyword,
                    "default" => SyntaxKind.DefaultKeyword,
                    "do" => SyntaxKind.DoKeyword,
                    "else" => SyntaxKind.ElseKeyword,
                    "false" => SyntaxKind.FalseKeyword,
                    "for" => SyntaxKind.ForKeyword,
                    "function" => SyntaxKind.FunctionKeyword,
                    "if" => SyntaxKind.IfKeyword,
                    "let" => SyntaxKind.LetKeyword,
                    "return" => SyntaxKind.ReturnKeyword,
                    "struct" => SyntaxKind.StructKeyword,
                    "this" => SyntaxKind.ThisKeyword,
                    "to" => SyntaxKind.ToKeyword,
                    "true" => SyntaxKind.TrueKeyword,
                    "var" => SyntaxKind.VarKeyword,
                    "while" => SyntaxKind.WhileKeyword,
                    _ => SyntaxKind.IdentifierToken,
                };
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
                return kind switch
                {
                    SyntaxKind.AmpersandAmpersandToken => "&&",
                    SyntaxKind.AmpersandEqualsToken => "&=",
                    SyntaxKind.AmpersandToken => "&",
                    SyntaxKind.BangEqualsToken => "!=",
                    SyntaxKind.BangToken => "!",
                    SyntaxKind.CloseBraceToken => "}",
                    SyntaxKind.CloseParenthesisToken => ")",
                    SyntaxKind.ColonToken => ":",
                    SyntaxKind.CommaToken => ",",
                    SyntaxKind.DotToken => ".",
                    SyntaxKind.EqualsEqualsToken => "==",
                    SyntaxKind.EqualsToken => "=",
                    SyntaxKind.GreaterOrEqualsToken => ">=",
                    SyntaxKind.GreaterToken => ">",
                    SyntaxKind.HatEqualsToken => "^=",
                    SyntaxKind.HatToken => "^",
                    SyntaxKind.LessOrEqualsToken => "<=",
                    SyntaxKind.LessToken => "<",
                    SyntaxKind.MinusEqualsToken => "-=",
                    SyntaxKind.MinusToken => "-",
                    SyntaxKind.OpenBraceToken => "{",
                    SyntaxKind.OpenParenthesisToken => "(",
                    SyntaxKind.PipeEqualsToken => "|=",
                    SyntaxKind.PipePipeToken => "||",
                    SyntaxKind.PipeToken => "|",
                    SyntaxKind.PlusEqualsToken => "+=",
                    SyntaxKind.PlusToken => "+",
                    SyntaxKind.SlashEqualsToken => "/=",
                    SyntaxKind.SlashToken => "/",
                    SyntaxKind.StarEqualsToken => "*=",
                    SyntaxKind.StarToken => "*",
                    SyntaxKind.TildeToken => "~",
    
                    SyntaxKind.BreakKeyword => "break",
                    SyntaxKind.ContinueKeyword => "continue",
                    SyntaxKind.DefaultKeyword => "default",
                    SyntaxKind.DoKeyword => "do",
                    SyntaxKind.ElseKeyword => "else",
                    SyntaxKind.FalseKeyword => "false",
                    SyntaxKind.ForKeyword => "for",
                    SyntaxKind.FunctionKeyword => "function",
                    SyntaxKind.IfKeyword => "if",
                    SyntaxKind.LetKeyword => "let",
                    SyntaxKind.ReturnKeyword => "return",
                    SyntaxKind.StructKeyword => "struct",
                    SyntaxKind.ThisKeyword => "this",
                    SyntaxKind.ToKeyword => "to",
                    SyntaxKind.TrueKeyword => "true",
                    SyntaxKind.VarKeyword => "var",
                    SyntaxKind.WhileKeyword => "while",
                    _ => null,
                };
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
                return kind == SyntaxKind.PlusEqualsToken
                    || kind == SyntaxKind.MinusEqualsToken
                    || kind == SyntaxKind.StarEqualsToken
                    || kind == SyntaxKind.SlashEqualsToken
                    || kind == SyntaxKind.AmpersandEqualsToken
                    || kind == SyntaxKind.PipeEqualsToken
                    || kind == SyntaxKind.HatEqualsToken
                    || kind == SyntaxKind.EqualsToken;
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
                return kind switch
                {
                    SyntaxKind.AmpersandEqualsToken => SyntaxKind.AmpersandToken,
                    SyntaxKind.HatEqualsToken => SyntaxKind.HatToken,
                    SyntaxKind.MinusEqualsToken => SyntaxKind.MinusToken,
                    SyntaxKind.PipeEqualsToken => SyntaxKind.PipeToken,
                    SyntaxKind.PlusEqualsToken => SyntaxKind.PlusToken,
                    SyntaxKind.SlashEqualsToken => SyntaxKind.SlashToken,
                    SyntaxKind.StarEqualsToken => SyntaxKind.StarToken,
                    _ => throw new Exception($"Unexpected syntax: '{kind}'"),
                };
            }
        }
}