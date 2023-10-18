namespace DZ1.Test.Calculator;

public abstract class CalculatorTests
{
    private readonly ICalculator _calculator;

    protected CalculatorTests(ICalculator calculator) => _calculator = calculator;

    [Theory]
    [InlineData("1+1", 2)]
    [InlineData("2+1", 3)]
    [InlineData("1+2", 3)]
    [InlineData("0+1", 1)]
    [InlineData("60+5", 65)]
    [InlineData("100+20+3", 123)]
    public void AddTest(string expression, double expected) => Assert.Equal(expected, _calculator.Calculate(expression));

    [Theory]
    [InlineData("1-1", 0)]
    [InlineData("5-4", 1)]
    [InlineData("1-9", -8)]
    [InlineData("1-0", 1)]
    [InlineData("60-5", 55)]
    [InlineData("100-20+3", 83)]
    public void SubTest(string expression, double expected) => Assert.Equal(expected, _calculator.Calculate(expression));
    
    [Theory]
    [InlineData("1*1", 1)]
    [InlineData("5*4", 20)]
    [InlineData("20*9", 180)]
    [InlineData("1*0", 0)]
    [InlineData("60*5", 300)]
    [InlineData("100*20*3", 6000)]
    public void MulTest(string expression, double expected) => Assert.Equal(expected, _calculator.Calculate(expression));

    [Theory]
    [InlineData("1/1", 1)]
    [InlineData("5/4", 1)]
    [InlineData("20/9", 2)]
    [InlineData("0/5", 0)]
    [InlineData("60/5", 12)]
    [InlineData("100/20", 5)]
    public void DivTest(string expression, double expected) => Assert.Equal(expected, _calculator.Calculate(expression));

    [Theory]
    [InlineData("(2+3)-4", 1)]
    [InlineData("4-(2*3)", -2)]
    [InlineData("1+2", 3)]
    [InlineData("(234-11)*34", 7582)]
    [InlineData("6*6/6", 6)]
    public void TestBasicOperations(string expression, double expected) => Assert.Equal(expected, _calculator.Calculate(expression));

    [Theory]
    [InlineData("2+3*4", 14)]
    [InlineData("10/2-1", 4)]
    public void TestOperatorPrecedence(string expression, double expected) => Assert.Equal(expected, _calculator.Calculate(expression));

    [Theory]
    [InlineData("2*(3+4)", 14)] 
    [InlineData("2*(3+4)/2", 7)]
    public void TestBrackets(string expression, double expected) => Assert.Equal(expected, _calculator.Calculate(expression));

    [Theory]
    [InlineData("2+3*4-1/2", 14)]
    [InlineData("(5+3)*(7-2)/2", 20)]
    public void TestComplexExpressions(string expression, double expected) => Assert.Equal(expected, _calculator.Calculate(expression));

    /// <summary>
    /// Ensure that attempting to divide by zero throws a DivideByZeroException
    /// </summary>
    [Fact]
    public void TestDivideByZero() => Assert.Throws<DivideByZeroException>(() => _calculator.Calculate("1/0"));

    /// <summary>
    /// Ensure that a complex expression involving divide by zero throws a DivideByZeroException
    /// </summary>
    [Fact]
    public void TestComplexExpressionWithDivideByZero() => Assert.Throws<DivideByZeroException>(() => _calculator.Calculate("(5+3)*(7-2)/(1-1)"));
}