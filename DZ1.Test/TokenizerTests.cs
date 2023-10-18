using DZ1.Tokenization;

namespace DZ1.Test;

public class TokenizerTests
{
    private readonly ITokenizer _tokenizer = new ArithmeticTokenizer();

    [Fact]
    public void TokenizeEmpty()
    {
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("")
        );
    }
    
    [Fact]
    public void TokenizeInt()
    {
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateOperandToken(0),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("0")
        );
        
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateOperandToken(1),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("1")
        );
        
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateOperandToken(2),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("2")
        );
        
                
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateOperandToken(10),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("10")
        );
        
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateOperandToken(9874),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("9874")
        );
    }
    
    [Fact]
    public void TokenizeAdd()
    {
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateOperandToken(1),
                TokenFactory.CreateWeakOperationToken(WeakOperation.Add),
                TokenFactory.CreateOperandToken(2),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("1+2")
        );
    }
    
    [Fact]
    public void TokenizeSub()
    {
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateOperandToken(1),
                TokenFactory.CreateWeakOperationToken(WeakOperation.Sub),
                TokenFactory.CreateOperandToken(2),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("1-2")
        );
    }
    
    [Fact]
    public void TokenizeMul()
    {
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateOperandToken(1),
                TokenFactory.CreateStrongOperationToken(StrongOperation.Mul),
                TokenFactory.CreateOperandToken(2),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("1*2")
        );
    }
    
    [Fact]
    public void TokenizeDiv()
    {
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateOperandToken(1),
                TokenFactory.CreateStrongOperationToken(StrongOperation.Div),
                TokenFactory.CreateOperandToken(2),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("1/2")
        );
    }
    
    [Fact]
    public void TokenizeBrackets()
    {
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateOpenBracketToken(),
                TokenFactory.CreateCloseBracketToken(),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("()")
        );
    }
    
    [Fact]
    public void TokenizeIgnoreWhitespace()
    {
        Assert.Equal(new[] {
                TokenFactory.CreateBeginToken(),
                TokenFactory.CreateOperandToken(12),
                TokenFactory.CreateWeakOperationToken(WeakOperation.Add),
                TokenFactory.CreateOperandToken(45),
                TokenFactory.CreateStrongOperationToken(StrongOperation.Mul),
                TokenFactory.CreateOperandToken(9),
                TokenFactory.CreateWeakOperationToken(WeakOperation.Sub),
                TokenFactory.CreateOpenBracketToken(),
                TokenFactory.CreateOperandToken(1),
                TokenFactory.CreateWeakOperationToken(WeakOperation.Add),
                TokenFactory.CreateOperandToken(2),
                TokenFactory.CreateCloseBracketToken(),
                TokenFactory.CreateStrongOperationToken(StrongOperation.Div),
                TokenFactory.CreateOperandToken(454),
                TokenFactory.CreateEndToken()
            },
            _tokenizer.Tokenize("   12 + \n  45 * 9 - (1 +   2) /  \t 454  ")
        );
    }
    
}