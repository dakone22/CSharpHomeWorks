namespace DZ1.Tokenization;

/// <summary>
/// Enum representing the type of a token (Operand or Operator).
/// </summary>
public enum TokenType { Operand, Operator }

/// <summary>
/// Base record representing a token.
/// </summary>
public abstract record Token(TokenType Type);

/// <summary>
/// Record representing an Operand token.
/// </summary>
public record OperandToken(double Value) : Token(TokenType.Operand);

/// <summary>
/// Enum representing the type of an operator token (Begin, End, OpenBracket, CloseBracket, WeakOperation, StrongOperation).
/// </summary>
public enum OperatorType { Begin, End, OpenBracket, CloseBracket, WeakOperation, StrongOperation }

/// <summary>
/// Record representing an Operator token.
/// </summary>
public record OperatorToken(OperatorType Operator) : Token(TokenType.Operator);

/// <summary>
/// Enum representing weak operation types (Add, Sub).
/// </summary>
public enum WeakOperation { Add, Sub }

/// <summary>
/// Record representing a WeakOperation token.
/// </summary>
public record WeakOperationToken(WeakOperation Operation) : OperatorToken(OperatorType.WeakOperation);

/// <summary>
/// Enum representing strong operation types (Mul, Div).
/// </summary>
public enum StrongOperation { Mul, Div }

/// <summary>
/// Record representing a StrongOperation token.
/// </summary>
public record StrongOperationToken(StrongOperation Operation) : OperatorToken(OperatorType.StrongOperation);

/// <summary>
/// A static class for creating various types of tokens.
/// </summary>
public static class TokenFactory
{
    /// <summary>
    /// Creates an Operand token with the specified value.
    /// </summary>
    /// <param name="value">The value of the Operand token.</param>
    /// <returns>An OperandToken instance.</returns>
    public static Token CreateOperandToken(double value) => new OperandToken(value);

    /// <summary>
    /// Creates a Begin token.
    /// </summary>
    /// <returns>An OperatorToken representing the Begin token.</returns>
    public static Token CreateBeginToken() => new OperatorToken(OperatorType.Begin);

    /// <summary>
    /// Creates an End token.
    /// </summary>
    /// <returns>An OperatorToken representing the End token.</returns>
    public static Token CreateEndToken() => new OperatorToken(OperatorType.End);

    /// <summary>
    /// Creates an OpenBracket token.
    /// </summary>
    /// <returns>An OperatorToken representing the OpenBracket token.</returns>
    public static Token CreateOpenBracketToken() => new OperatorToken(OperatorType.OpenBracket);

    /// <summary>
    /// Creates a CloseBracket token.
    /// </summary>
    /// <returns>An OperatorToken representing the CloseBracket token.</returns>
    public static Token CreateCloseBracketToken() => new OperatorToken(OperatorType.CloseBracket);

    /// <summary>
    /// Creates a WeakOperation token with the specified operation type.
    /// </summary>
    /// <param name="operation">The type of weak operation (Add or Sub).</param>
    /// <returns>A WeakOperationToken instance.</returns>
    public static Token CreateWeakOperationToken(WeakOperation operation) => new WeakOperationToken(operation);

    /// <summary>
    /// Creates a StrongOperation token with the specified operation type.
    /// </summary>
    /// <param name="operation">The type of strong operation (Mul or Div).</param>
    /// <returns>A StrongOperationToken instance.</returns>
    public static Token CreateStrongOperationToken(StrongOperation operation) => new StrongOperationToken(operation);
}
