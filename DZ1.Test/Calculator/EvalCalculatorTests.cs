using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace DZ1.Test.Calculator;

internal class EvalCalculator : ICalculator
{
    public double Calculate(string expression)
    {
        try {
            return CSharpScript.EvaluateAsync<double>(expression).Result;
        } catch (Microsoft.CodeAnalysis.Scripting.CompilationErrorException ex) {
            throw ex.Message.Contains("CS0020") ? new DivideByZeroException() : ex;
        }
    }
}

public class EvalCalculatorTests : CalculatorTests
{
    public EvalCalculatorTests() : base(new EvalCalculator()) {}
}