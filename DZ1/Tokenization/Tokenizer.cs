namespace DZ1.Tokenization;

public interface ITokenizer
{
    Token[] Tokenize(string expression);
}

public class ArithmeticTokenizer : ITokenizer
{
    public Token[] Tokenize(string expression)
    {
        var tokens = new List<Token> { TokenFactory.CreateBeginToken() };

        var value = 0;
        var isParsingValue = false;
        foreach (var ch in expression + " ") {
            if (char.IsNumber(ch)) {
                if (isParsingValue) {
                    value = value * 10 + (ch - '0');
                } else {
                    isParsingValue = true;
                    value = (ch - '0');
                }

                continue;
            }

            if (isParsingValue) {
                tokens.Add(TokenFactory.CreateOperandToken(value));
                isParsingValue = false;
                value = 0;
            }

            if (char.IsWhiteSpace(ch)) {
                continue;
            }

            tokens.Add(ch switch {
                '(' => TokenFactory.CreateOpenBracketToken(),
                ')' => TokenFactory.CreateCloseBracketToken(),
                '+' => TokenFactory.CreateWeakOperationToken(WeakOperation.Add),
                '-' => TokenFactory.CreateWeakOperationToken(WeakOperation.Sub),
                '*' => TokenFactory.CreateStrongOperationToken(StrongOperation.Mul),
                '/' => TokenFactory.CreateStrongOperationToken(StrongOperation.Div),
                _ => throw new Exception($"Unknown symbol '{ch}' while parsing string \"{expression}\"")
            });
        }
        
        tokens.Add(TokenFactory.CreateEndToken());

        return tokens.ToArray();
    }
}