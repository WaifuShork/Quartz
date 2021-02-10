using System;
using System.Text;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Text;

namespace Vivian.CodeAnalysis.Syntax
{
    internal sealed class Lexer
    {
        private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SyntaxTree _syntaxTree;
        private readonly SourceText _text;
        private int _position;
        
        private int _start;
        private SyntaxKind _kind;
        private object _value;
        private TypeSymbol _type;
        
        public Lexer(SyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;
            _text = syntaxTree.Text;
        }

        public DiagnosticBag Diagnostics => _diagnostics;
        private char Current => Peak(0);
        private char Lookahead => Peak(1);

        private char Peak(int offset)
        {
            var index = _position + offset;
            
            if (index >= _text.Length)
                return '\0';
            
            return _text[index];
        }

        public SyntaxToken Lex()
        {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;
            _type = null;
            
            switch (Current)
            {
                case '\0':
                    _kind = SyntaxKind.EndOfFileToken;
                    break;
                case '+':
                    _kind = SyntaxKind.PlusToken;
                    _position++;
                    break;
                case '%':
                    _kind = SyntaxKind.ModuloToken;
                    _position++;
                    break;
                case '-':
                    _kind = SyntaxKind.MinusToken;
                    _position++;
                    break;
                case '*':
                    _kind = SyntaxKind.StarToken;
                    _position++;
                    break;
                case '/':
                    _kind = SyntaxKind.SlashToken;
                    _position++;
                    break;
                case '(':
                    _kind = SyntaxKind.OpenParenthesisToken;
                    _position++;
                    break;
                case ')':
                    _kind = SyntaxKind.CloseParenthesisToken;
                    _position++;
                    break;
                case '{':
                    _kind = SyntaxKind.OpenBraceToken;
                    _position++;
                    break;
                case ':':
                    _kind = SyntaxKind.ColonToken;
                    _position++;
                    break;
                case '}':
                    _kind = SyntaxKind.CloseBraceToken;
                    _position++;
                    break;
                case ',':
                    _kind = SyntaxKind.CommaToken;
                    _position++;
                    break;
                case '~':
                    _kind = SyntaxKind.TildeToken;
                    _position++;
                    break;
                case '^':
                    _kind = SyntaxKind.HatToken;
                    _position++;
                    break;
                case ';':
                    _kind = SyntaxKind.SemicolonToken;
                    _position++;
                    break;
                case '&':
                    _position++;
                    if (Current != '&')
                    {
                        _kind = SyntaxKind.AmpersandToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.AmpersandAmpersandToken;
                        _position++;
                    }
                    break;
                case '|':
                    _position++;
                    if (Current != '|')
                    {
                        _kind = SyntaxKind.PipeToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.PipePipeToken;
                        _position++;
                    }
                    break;

                case '=':
                    _position++;
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.EqualsToken;
                    }
                    else
                    {
                        _position++;
                        _kind = SyntaxKind.EqualsEqualsToken;
                    }
                    break;

                case '!':
                    _position++;
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.BangToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.BangEqualsToken;
                        _position++;
                    }
                    break;
                case '<':
                    _position++;
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.LessToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.LessOrEqualsToken;
                        _position++;
                    }
                    break;
                case '>':
                    _position++;
                    if (Current != '=')
                    {
                        _kind = SyntaxKind.GreaterToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.GreaterOrEqualsToken;
                        _position++;
                    }
                    break;
                case '"':
                    ReadString();
                    break;
                case'0': case'1': case'2': case'3': case'4': 
                case'5': case'6': case'7': case'8': case'9':
                case'.':
                    ReadNumberToken();
                    break;
                
                case ' ': case '\t': case '\n':
                    ReadWhiteSpace();
                    break;

                default:
                    if (char.IsLetter(Current))
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else if (char.IsWhiteSpace(Current))
                    {
                        ReadWhiteSpace();
                    }
                    else
                    {
                        var span = new TextSpan(_position, 1);
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportBadCharacter(location, Current);
                        _position++;
                    }
                    break;
            }

            var length = _position - _start;
            var text = SyntaxFacts.GetText(_kind);
            if (text == null)
                text = _text.ToString(_start, length);

            return new SyntaxToken(_syntaxTree, _kind, _start, text, _value, _type);
        }

        private void ReadString()
        {
            _position++;
            
            var sb = new StringBuilder();
            var done = false;
            
            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        var span = new TextSpan(_start, 1);
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportUnterminatedString(location);
                        done = true;
                        break;
                    case '"':
                        if (Lookahead == '"')
                        {
                            sb.Append(Current);
                            _position += 2;
                        }
                        else
                        {
                            _position++;
                            done = true;
                        }
                        break;
                    default:
                        sb.Append(Current);
                        _position++;
                        break;
                }
            }

            _kind = SyntaxKind.StringToken;
            _value = sb.ToString();
            _type = TypeSymbol.String;
        }

        private void ReadWhiteSpace()
        {
            while (char.IsWhiteSpace(Current))
                _position++;

            _kind = SyntaxKind.WhitespaceToken;
        }

        private void ReadNumberToken()
        {
            var decimalPointFound = false;
            while (char.IsDigit(Current) || (Current == '.' && !decimalPointFound))
            {
                if (Current == '.')
                    decimalPointFound = true;
                _position++;
            }

            var length = _position - _start;
            var text = _text.ToString(_start, length);

            var type = '\0';
            if (Current is 'l' or 'f' or 'm') {
                type = Current;
                _position++;
            }

            _type = TypeSymbol.Int;

            switch (type)
            {
                case '\0':
                    if (int.TryParse(text, out var intValue))
                    {
                        _value = intValue;
                        _type = TypeSymbol.Int;
                    }
                    else if (double.TryParse(text, out var doubleValue))
                    {
                        _value = doubleValue;
                        _type = TypeSymbol.Double;
                    }
                    else
                    {
                        var span = new TextSpan(_start, length);
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportInvalidNumber(location, text, decimalPointFound ? TypeSymbol.Double : TypeSymbol.Int);
                    }
                    break;
                case 'l':
                    if (decimalPointFound)
                        throw new Exception($"Invalid type specifier {type} for fixed point number");
                    if (long.TryParse(text, out var longValue))
                    {
                        _value = longValue;
                        _type = TypeSymbol.Long;
                    }
                    else
                    {
                        var span = new TextSpan(_start, length);
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Long);
                    }
                    break;
                case 'f':
                    if (!decimalPointFound)
                        throw new Exception($"Invalid type specifier {type} for floating point number");
                    if (float.TryParse(text, out var floatValue))
                    {
                        _value = floatValue;
                        _type = TypeSymbol.Float;
                    }
                    else
                    {
                        var span = new TextSpan(_start, length);
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Float);
                    }
                    break;
                case 'm':
                    if (decimal.TryParse(text, out var decimalValue))
                    {
                        _value = decimalValue;
                        _type = TypeSymbol.Decimal;
                    }
                    else
                    {
                        var span = new TextSpan(_start, length);
                        var location = new TextLocation(_text, span);
                        _diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Decimal);
                    }
                    break;
                default:
                    throw new Exception($"Unknown type specifier {type}");
            }

            _kind = SyntaxKind.NumberToken;
        }
        
        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
                _position++;

            var length = _position - _start;
            var text = _text.ToString(_start, length);
            _kind = SyntaxFacts.GetKeywordKind(text);
        }
    }
}