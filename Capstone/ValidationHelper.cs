
namespace Capstone;

public class ValidationHelper
{
    public static bool IsValidXSSInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return true;
        }

        string lowerInput = input.ToLower();
        string[] xssIndicators = { "<script>", "</script>", "javascript:", "onerror=", "onload=", "<img", "<iframe", "<svg", "<body", "<div" };
        foreach (var indicator in xssIndicators)
        {
            if (lowerInput.Contains(indicator))
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsValidInput(string input, string allowedSpecialCharacters = "")
    {
        if (string.IsNullOrEmpty(input))

            return false;

        var validCharacters = allowedSpecialCharacters.ToHashSet();

        return input.All(c => char.IsLetterOrDigit(c) || validCharacters.Contains(c));
    }
}