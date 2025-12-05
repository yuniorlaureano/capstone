using Capstone;

namespace Capstone.Test;

public class TestInputValidation
{
    [Fact]
    public void TestForSQLInjection() 
    {
        string maliciousInput = "admin'--";
        bool isValid = ValidationHelper.IsValidInput(maliciousInput, "_@.-");
        Assert.False(isValid);
    }

    [Fact]
    public void TestForXSS() 
    {
        string maliciousInput = "<script>alert('XSS');</script>";
        bool isValid = ValidationHelper.IsValidXSSInput(maliciousInput);
        Assert.False(isValid);        
    }
}
