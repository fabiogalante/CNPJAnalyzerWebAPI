using System.Text.RegularExpressions;

namespace CNPJAnalyzerWebAPI.Analyzers;

public static class PatternAnalyzer
{
    public static bool HasNumericConversion(string line)
    {
        var patterns = new[]
        {
            @"long\.Parse\s*\(", @"Convert\.ToInt64\s*\(", @"parseInt\s*\(",
            @"parseFloat\s*\(", @"Number\s*\(", @"BigInteger\s*\(",
            @"Convert\.ToUInt64\s*\(\s*cnpj\s*\)", @"Convert\.ToInt64\s*\(\s*cnpj\s*\)",
            @"long\.Parse\s*\(\s*cnpj\s*\)", @"int\.Parse\s*\(\s*cnpj\s*\)"
        };
        return patterns.Any(pattern => Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
    }

    public static bool HasHardcodedFormatting(string line)
    {
        var patterns = new[]
        {
            @"\.Substring\s*\(\s*\d+\s*,\s*\d+\s*\)", @"\.substring\s*\(\s*\d+\s*,\s*\d+\s*\)",
            @"String\.Format.*\{0:00\.000\.000\/0000-00\}", @"format.*00\.000\.000\/0000-00",
            @"ToString\s*\(\s*@?['""].*00\\\.000\\\.000\\\/0000\\-00.*['""]",
            @"\.ToString\s*\(\s*@?['""].*\{0:.*\}.*['""].*cnpj",
            @"00\\\.000\\\.000\\\/0000\\-00"
        };
        return patterns.Any(pattern => Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
    }

    public static bool HasRigidLengthValidation(string line)
    {
        var patterns = new[]
        {
            @"Length\s*==\s*14", @"len\s*==\s*14", @"size\s*==\s*14",
            @"maxlength\s*=\s*['""]?14['""]?", @"minlength\s*=\s*['""]?14['""]?"
        };
        return patterns.Any(pattern => Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
    }

    public static bool IsNumericOnlyValidation(string line)
    {
        var patterns = new[]
        {
            @"IsNumeric\s*\(", @"int\.Parse\s*\(", @"Convert\.ToInt",
            @"[Tt]ype\s*==?\s*['""]?number['""]?", @"[Dd]ataType\s*==?\s*['""]?[Nn]umber['""]?",
            @"[Rr]egex.*\[\^\\d\]", @"[Oo]nly.*[Nn]umber", @"[Nn]umeric.*[Oo]nly"
        };
        return patterns.Any(pattern => Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
    }

    public static bool HasNumericSQLType(string line)
    {
        var patterns = new[]
        {
            @"BIGINT", @"NUMERIC\s*\(\s*14", @"DECIMAL\s*\(\s*14",
            @"INT\s+cnpj", @"BIGINT\s+cnpj", @"cnpj\s+BIGINT", @"cnpj\s+INT"
        };
        return patterns.Any(pattern => Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
    }

    public static bool HasNumericCNPJType(string line)
    {
        var problematicPatterns = new[]
        {
            @"\b(long|int|short|byte|ulong|uint|ushort|sbyte)\s+[a-zA-Z0-9_]*[Cc]npj[a-zA-Z0-9_]*\b",
            @"\b(long|int|short|byte|ulong|uint|ushort|sbyte)\s+\w*[Cc]npj\w*\s*\{",
            @"\b(long|int|short|byte|ulong|uint|ushort|sbyte)\s+\w*[Cc]npj\w*\s*;",
            @"\b(long|int|short|byte|ulong|uint|ushort|sbyte)\s+\w*[Cc]npj\w*\s*=",
            @"\(\s*(long|int|short|byte|ulong|uint|ushort|sbyte)\s+\w*[Cc]npj\w*\s*[\),]",
            @"\w*[Cc]npj\w*\s*=\s*\d+L?\s*;",
            @"Convert\.To(Int32|Int64|UInt32|UInt64)\s*\(\s*\w*[Cc]npj\w*\s*\)"
        };
        return problematicPatterns.Any(pattern => Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
    }
}