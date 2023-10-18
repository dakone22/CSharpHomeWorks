using DZ1.Tokenization;

namespace DZ1;

using State = OperatorType;

/// <summary>
/// Interface for a calculator.
/// </summary>
public interface ICalculator
{
    /// <summary>
    /// Calculates the result of an expression.
    /// </summary>
    /// <param name="expression">The expression to be calculated.</param>
    /// <returns>The result of the calculation.</returns>
    double Calculate(string expression);
}

/// <summary>
/// Base class for tokenized calculators.
/// </summary>
public abstract class TokenizedCalculator : ICalculator
{
    private readonly ITokenizer _tokenizer;

    /// <summary>
    /// Initializes a new instance of the TokenizedCalculator class.
    /// </summary>
    /// <param name="tokenizer">The tokenizer to be used for tokenization.</param>
    protected TokenizedCalculator(ITokenizer tokenizer) => _tokenizer = tokenizer;

    /// <inheritdoc/>
    public double Calculate(string expression) => ProcessTokens(_tokenizer.Tokenize(expression));

    /// <summary>
    /// Process an array of tokens to calculate the result.
    /// </summary>
    /// <param name="tokenArray">An array of tokens to be processed.</param>
    /// <returns>The result of the calculation.</returns>
    protected abstract double ProcessTokens(Token[] tokenArray);
}

/// <summary>
/// Reverse Polish Notation Calculator using the Bauer-Zamelson algorithm.
/// </summary>
public class BzaCalculator : TokenizedCalculator
{
    /// <summary>
    /// Initializes a new instance of the BzaCalculator class.
    /// </summary>
    public BzaCalculator() : base(new ArithmeticTokenizer()) { }

    // Enum for transition results during parsing
    private enum TransitionResult { TokenStay, NextToken, End }

    /// <inheritdoc/>
    protected override double ProcessTokens(Token[] tokenArray)
    {
        var rpnTokenList = ConstructReversePolishNotation(tokenArray);
        var value = CalculateReversePolishNotation(rpnTokenList);
        return Math.Round(value);
    }

    private static List<Token> ConstructReversePolishNotation(Token[] tokenArray)
    {
        Array.Reverse(tokenArray);
        var expressionTokens = new Stack<Token>(tokenArray);

        var operatorStack = new Stack<OperatorToken>();
        var rpnTokenList = new List<Token>();

        #region transition table

        TransitionResult PushOperatorToStack(OperatorToken token)
        {
            operatorStack.Push(token);
            return TransitionResult.NextToken;
        }

        TransitionResult MoveAndPush(OperatorToken token)
        {
            rpnTokenList.Add(operatorStack.Pop());
            operatorStack.Push(token);
            return TransitionResult.NextToken;
        }

        TransitionResult Discard(OperatorToken _)
        {
            operatorStack.Pop();
            return TransitionResult.NextToken;
        }

        TransitionResult MoveFromStackToList(OperatorToken _)
        {
            rpnTokenList.Add(operatorStack.Pop());
            return TransitionResult.TokenStay;
        }

        IReadOnlyDictionary<State, IReadOnlyDictionary<State, Func<OperatorToken, TransitionResult>>> transitions =
            new Dictionary<State, IReadOnlyDictionary<State, Func<OperatorToken, TransitionResult>>> {
                {
                    State.Begin, new Dictionary<State, Func<OperatorToken, TransitionResult>> {
                        { State.WeakOperation, PushOperatorToStack },
                        { State.StrongOperation, PushOperatorToStack },
                        { State.OpenBracket, PushOperatorToStack }, {
                            State.CloseBracket,
                            token => throw new Exception($"Unexpected closed bracket token ({token})!")
                        },
                        { State.End, _ => TransitionResult.End },
                    }
                }, {
                    State.WeakOperation, new Dictionary<State, Func<OperatorToken, TransitionResult>> {
                        { State.WeakOperation, MoveAndPush },
                        { State.StrongOperation, PushOperatorToStack },
                        { State.OpenBracket, PushOperatorToStack },
                        { State.CloseBracket, MoveFromStackToList },
                        { State.End, MoveFromStackToList },
                    }
                }, {
                    State.StrongOperation, new Dictionary<State, Func<OperatorToken, TransitionResult>> {
                        { State.WeakOperation, MoveFromStackToList },
                        { State.StrongOperation, MoveAndPush },
                        { State.OpenBracket, PushOperatorToStack },
                        { State.CloseBracket, MoveFromStackToList },
                        { State.End, MoveFromStackToList },
                    }
                }, {
                    State.OpenBracket, new Dictionary<State, Func<OperatorToken, TransitionResult>> {
                        { State.WeakOperation, PushOperatorToStack },
                        { State.StrongOperation, PushOperatorToStack },
                        { State.OpenBracket, PushOperatorToStack },
                        { State.CloseBracket, Discard },
                        { State.End, token => throw new Exception($"Unexpected end token ({token})!") },
                    }
                },
            };

        #endregion

        var firstToken = expressionTokens.Pop();
        if (firstToken.Type != TokenType.Operator || ((OperatorToken)firstToken).Operator != OperatorType.Begin) {
            throw new Exception($"First token is not Begin! {firstToken}");
        }

        operatorStack.Push((OperatorToken)firstToken);

        while (true) {
            var token = expressionTokens.Peek();

            if (token.Type == TokenType.Operand) {
                rpnTokenList.Add(expressionTokens.Pop());
                continue;
            }

            var operatorToken = (OperatorToken)token;
            var nextState = operatorToken.Operator;

            var currentState = operatorStack.Peek().Operator;

            if (!transitions[currentState].ContainsKey(nextState)) {
                throw new Exception($"No next state {nextState} for current state {currentState}!");
            }

            var result = transitions[currentState][nextState](operatorToken);

            if (result == TransitionResult.End)
                break;

            if (result == TransitionResult.NextToken)
                expressionTokens.Pop();
        }

        return rpnTokenList;
    }

    private static double CalculateReversePolishNotation(List<Token> rpnTokenList)
    {
        var operandStack = new Stack<OperandToken>();

        foreach (var token in rpnTokenList) {
            if (token.Type == TokenType.Operand) {
                operandStack.Push((OperandToken)token);
                continue;
            }

            var operationToken = (OperatorToken)token;

            if (operandStack.Count < 2)
                throw new Exception($"Not enough operands for operation {operationToken}!");

            var operand2 = operandStack.Pop();
            var operand1 = operandStack.Pop();

            double result;
            switch (operationToken.Operator) {
                case OperatorType.WeakOperation:
                {
                    var weakOperationToken = (WeakOperationToken)operationToken;
                    result = weakOperationToken.Operation switch {
                        WeakOperation.Add => operand1.Value + operand2.Value,
                        WeakOperation.Sub => operand1.Value - operand2.Value,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    break;
                }
                case OperatorType.StrongOperation:
                {
                    var strongOperationToken = (StrongOperationToken)operationToken;
                    result = strongOperationToken.Operation switch {
                        StrongOperation.Mul => operand1.Value * operand2.Value,
                        StrongOperation.Div when operand2.Value == 0 => throw new DivideByZeroException(
                            $"Second operand is zero! {operand1} {operationToken} {operand2}"),
                        StrongOperation.Div => operand1.Value / operand2.Value,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                    break;
                }
                case OperatorType.Begin:
                case OperatorType.End:
                case OperatorType.OpenBracket:
                case OperatorType.CloseBracket:
                default:
                    throw new Exception(
                        $"Can't process operation {operationToken} on operands {operand1} and {operand2}");
            }

            operandStack.Push((OperandToken)TokenFactory.CreateOperandToken(result));
        }

        if (operandStack.Count != 1) {
            throw new Exception($"operandStack.Count != 1! {operandStack}");
        }

        return operandStack.Pop().Value;
    }
}

internal static class Program
{
    public static void Main()
    {
        ICalculator calculator = new BzaCalculator();
        var expression = Console.ReadLine();
        if (expression == null) return;

        try {
            var result = calculator.Calculate(expression);
            Console.WriteLine($"Result: {Math.Round(result)}");
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}