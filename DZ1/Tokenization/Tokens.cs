namespace DZ1.Tokenization;

public enum TokenType { Operand, Operator }
public abstract record Token(TokenType Type);

public record OperandToken(double Value) : Token(TokenType.Operand);

public enum OperatorType { Begin, End, OpenBracket, CloseBracket, WeakOperation, StrongOperation }
public record OperatorToken(OperatorType Operator) : Token(TokenType.Operator);

public enum WeakOperation { Add, Sub }
public record WeakOperationToken(WeakOperation Operation) : OperatorToken(OperatorType.WeakOperation);

public enum StrongOperation { Mul, Div }
public record StrongOperationToken(StrongOperation Operation) : OperatorToken(OperatorType.StrongOperation);


public static class TokenFactory
{
    public static Token CreateOperandToken(double value) => new OperandToken(value);

    public static Token CreateBeginToken() => new OperatorToken(OperatorType.Begin);
    public static Token CreateEndToken() => new OperatorToken(OperatorType.End); 
        
    public static Token CreateOpenBracketToken() => new OperatorToken(OperatorType.OpenBracket);
    public static Token CreateCloseBracketToken() => new OperatorToken(OperatorType.CloseBracket);
        
    public static Token CreateWeakOperationToken(WeakOperation operation) => new WeakOperationToken(operation);
    public static Token CreateStrongOperationToken(StrongOperation operation) => new StrongOperationToken(operation);
}
