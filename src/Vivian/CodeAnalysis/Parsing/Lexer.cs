using System.Collections.Immutable;
using System.Text;
using Vivian.CodeAnalysis.Symbols;
using Vivian.CodeAnalysis.Text;

namespace Vivian.CodeAnalysis.Syntax
{
    internal sealed class Lexer
    {
        private readonly SyntaxTree _syntaxTree;
        private SyntaxKind _kind;
        
        private readonly SourceText _text;
        private int _position;
        private int _start;
        private object? _value;
        
        private char _current => Peek(0);
        private char _lookahead => Peek(1);
        
        private readonly ImmutableArray<SyntaxTrivia>.Builder _triviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia>();

        public Lexer(SyntaxTree syntaxTree)
        {
            _syntaxTree = syntaxTree;
            _text = syntaxTree.Text;
        }

        public DiagnosticBag Diagnostics { get; } = new();
        
        private char Peek(int offset)
        {
            var index = _position + offset;

            if (index >= _text.Length)
            {
                return '\0';
            }

            return _text[index];
        }

        public SyntaxToken Lex()
        {
            ReadTrivia(leading: true);

            var leadingTrivia = _triviaBuilder.ToImmutable();
            var tokenStart = _position;

            ReadToken();

            var tokenKind = _kind;
            var tokenValue = _value;
            var tokenLength = _position - _start;

            ReadTrivia(leading: false);

            var trailingTrivia = _triviaBuilder.ToImmutable();

            var tokenText = SyntaxFacts.GetText(tokenKind);
            if (tokenText == null)
            {
                tokenText = _text.ToString(tokenStart, tokenLength);
            }

            return new SyntaxToken(_syntaxTree, tokenKind, tokenStart, tokenText, tokenValue, leadingTrivia, trailingTrivia);
        }

        private void ReadTrivia(bool leading)
        {
            _triviaBuilder.Clear();

            var done = false;

            while (!done)
            {
                _start = _position;
                _kind = SyntaxKind.BadToken;
                _value = null;

                switch (_current)
                {
                    case '\0':
                        done = true;
                        break;
                    case '/':
                        if (_lookahead == '/')
                        {
                            ReadSingleLineComment();
                        }
                        else if (_lookahead == '*')
                        {
                            ReadMultiLineComment();
                        }
                        else
                        {
                            done = true;
                        }
                        break;
                    case '\n':
                    case '\r':
                        if (!leading)
                        {
                            done = true;
                        }
                        ReadLineBreak();
                        break;
                    case ' ':
                    case '\t':
                        ReadWhiteSpace();
                        break;
                    default:
                        if (char.IsWhiteSpace(_current))
                        {
                            ReadWhiteSpace();
                        }
                        else
                        {
                            done = true;
                        }
                        break;
                }

                var length = _position - _start;
                if (length > 0)
                {
                    var text = _text.ToString(_start, length);
                    var trivia = new SyntaxTrivia(_syntaxTree, _kind, _start, text);
                    _triviaBuilder.Add(trivia);
                }
            }
        }

        private void ReadLineBreak()
        {
            if (_current == '\r' && _lookahead == '\n')
            {
                _position += 2;
            }
            else
            {
                _position++;
            }

            _kind = SyntaxKind.LineBreakTrivia;
        }

        private void ReadWhiteSpace()
        {
            var done = false;

            while (!done)
            {
                switch (_current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        done = true;
                        break;
                    default:
                        if (!char.IsWhiteSpace(_current))
                        {
                            done = true;
                        }
                        else
                        {
                            _position++;
                        }
                        break;
                }
            }

            _kind = SyntaxKind.WhitespaceTrivia;
        }

        private void ReadSingleLineComment()
        {
            _position += 2;
            var done = false;

            while (!done)
            {
                switch (_current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        done = true;
                        break;
                    default:
                        _position++;
                        break;
                }
            }

            _kind = SyntaxKind.SingleLineCommentTrivia;
        }

        private void ReadMultiLineComment()
        {
            _position += 2;
            var done = false;

            while (!done)
            {
                switch (_current)
                {
                    case '\0':
                        var span = new TextSpan(_start, 2);
                        var location = new TextLocation(_text, span);
                        Diagnostics.ReportUnterminatedMultiLineComment(location);
                        done = true;
                        break;
                    case '*':
                        if (_lookahead == '/')
                        {
                            _position++;
                            done = true;
                        }
                        _position++;
                        break;
                    default:
                        _position++;
                        break;
                }
            }

            _kind = SyntaxKind.MultiLineCommentTrivia;
        }

        private void ReadToken()
        {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;

            switch (_current)
            {
                case '\0':
                    _kind = SyntaxKind.EndOfFileToken;
                    break;
                case '.':
                    _kind = SyntaxKind.DotToken;
                    _position++;
                    break;
                case '+':
                    _position++;
                    if (_current != '=')
                    {
                        _kind = SyntaxKind.PlusToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.PlusEqualsToken;
                        _position++;
                    }
                    break;
                case '-':
                    _position++;
                    if (_current != '=')
                    {
                        _kind = SyntaxKind.MinusToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.MinusEqualsToken;
                        _position++;
                    }
                    break;
                case '*':
                    _position++;
                    if (_current != '=')
                    {
                        _kind = SyntaxKind.StarToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.StarEqualsToken;
                        _position++;
                    }
                    break;
                case '/':
                    _position++;
                    if (_current != '=')
                    {
                        _kind = SyntaxKind.SlashToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.SlashEqualsToken;
                        _position++;
                    }
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
                case '}':
                    _kind = SyntaxKind.CloseBraceToken;
                    _position++;
                    break;
                case ':':
                    _kind = SyntaxKind.ColonToken;
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
                case '%':
                    _kind = SyntaxKind.PercentToken;
                    _position++;
                    break;
                case ';':
                    _kind = SyntaxKind.SemicolonToken;
                    _position++;
                    break;
                case '^':
                    _position++;
                    if (_current != '=')
                    {
                        _kind = SyntaxKind.HatToken;
                    }
                    else
                    {
                        _kind = SyntaxKind.HatEqualsToken;
                        _position++;
                    }
                    break;
                case '&':
                    _position++;
                    if (_current == '&')
                    {
                        _kind = SyntaxKind.AmpersandAmpersandToken;
                        _position++;
                    }
                    else if (_current == '=')
                    {
                        _kind = SyntaxKind.AmpersandEqualsToken;
                        _position++;
                    }
                    else
                    {
                        _kind = SyntaxKind.AmpersandToken;
                    }
                    break;
                case '|':
                    _position++;
                    if (_current == '|')
                    {
                        _kind = SyntaxKind.PipePipeToken;
                        _position++;
                    }
                    else if (_current == '=')
                    {
                        _kind = SyntaxKind.PipeEqualsToken;
                        _position++;
                    }
                    else
                    {
                        _kind = SyntaxKind.PipeToken;
                    }
                    break;
                case '=':
                    _position++;
                    if (_current != '=' && _current != '>')
                    {
                        _kind = SyntaxKind.EqualsToken;
                    }
                    else switch (_current)
                    {
                        case '>':
                            _kind = SyntaxKind.EqualsGreaterThanToken;
                            _position++;
                            break;
                        case '=':
                            _kind = SyntaxKind.EqualsEqualsToken; 
                            _position++; 
                            break;
                    }
                    break;
                case '!':
                    _position++;
                    if (_current != '=')
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
                    if (_current != '=')
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
                    if (_current != '=')
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
                case '\'':
                    ReadString();
                    break;
                case '0': 
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    ReadNumber();
                    break;
                case '_':
                    ReadIdentifierOrKeyword();
                    break;
                default:
                    if (char.IsLetter(_current))
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else
                    {
                        var span = new TextSpan(_position, 1);
                        var location = new TextLocation(_text, span);
                        Diagnostics.ReportBadCharacter(location, _current);
                        _position++;
                    }
                    break;
            }
        }

        private void ReadString()
        {
            // Skip the current quote
            var quoteChar = _current;
            _position++;

            var sb = new StringBuilder();
            var done = false;

            while (!done)
            {
                switch (_current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        var span = new TextSpan(_start, 1);
                        var location = new TextLocation(_text, span);
                        Diagnostics.ReportUnterminatedString(location);
                        done = true;
                        break;
                    case '\'':
                    case '"':
                        if (_lookahead == quoteChar)
                        {
                            sb.Append(_current);
                            _position += 2;
                        }
                        else
                        {
                            _position++;
                            done = true;
                        }
                        break;
                    default:
                        sb.Append(_current);
                        _position++;
                        break;
                }
            }

            _kind = quoteChar == '"' ? SyntaxKind.StringToken : SyntaxKind.CharToken;

            if (quoteChar == '"')
            {
                _value = sb.ToString();
            }
            else
            {
                if (sb.Length > 1)
                {
                    var span = new TextSpan(_start, 1);
                    var location = new TextLocation(_text, span);

                    if (sb.Length == 0)
                    {
                        Diagnostics.ReportEmptyCharConst(location);
                    }
                    else
                    {
                        Diagnostics.ReportInvalidCharConst(location);
                    }
                }

                _value = sb[0];
            }
        }

        private void ReadNumber()
        {
            var hasSeparator = false;
            var hasDecimal = false;
            var hasMultipleDecimals = false;

            // Allow numbers and underscores as long as there are more digits following them.
            // This allows _ to act as separators for numeric literals (e.g. 1_000_000)
            while (char.IsDigit(_current) || 
                   _current == '_' && char.IsDigit(Peek(1)) || 
                   _current == '.' && char.IsDigit(Peek(1)))
            {
                if (!hasSeparator && _current == '_')
                {
                    hasSeparator = true;
                }

                if (_current == '.')
                {
                    if (hasDecimal)
                    {
                        hasMultipleDecimals = true;
                    }

                    hasDecimal = true;
                }

                _position++;
            }

            var length = _position - _start;
            var text = _text.ToString(_start, length).Replace("_", string.Empty);

            var span = new TextSpan(_start, length);
            var location = new TextLocation(_text, span);

            // Underscores followed by a number are valid identifiers
            if (text.StartsWith('_'))
            {
                Diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Decimal);
            }

            if (hasMultipleDecimals)
            {
                Diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Decimal);
            }

            if (hasDecimal)
            {
                if (!double.TryParse(text, out var floatValue))
                {
                    Diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Decimal);
                }
                else
                {
                    if (floatValue >= float.MinValue && floatValue <= float.MaxValue)
                    {
                        _value = (float)floatValue;
                    }
                    else if (floatValue >= double.MinValue && floatValue <= double.MaxValue)
                    {
                        _value = floatValue;
                    }
                }
            }
            else
            {
                if (!ulong.TryParse(text, out var integerValue))
                {
                    Diagnostics.ReportInvalidNumber(location, text, TypeSymbol.Int32);
                }
                else
                {
                    if (integerValue <= int.MaxValue)
                    {
                        _value = (int)integerValue;
                    }
                    else if (integerValue <= uint.MaxValue)
                    {
                        _value = (uint)integerValue;
                    }
                    else if (integerValue <= long.MaxValue)
                    {
                        _value = integerValue;
                    }
                    else if (integerValue <= ulong.MaxValue)
                    {
                        _value = integerValue;
                    }
                }
            }

            _kind = SyntaxKind.NumberToken;
        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetterOrDigit(_current) || _current == '_')
            {
                _position++;
            }

            var length = _position - _start;
            var text = _text.ToString(_start, length);
            _kind = SyntaxFacts.GetKeywordKind(text);
        }
    }
}