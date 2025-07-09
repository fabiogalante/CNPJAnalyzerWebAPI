using System.Text.RegularExpressions;
using CNPJAnalyzerWebAPI.Entities;
using CNPJAnalyzerWebAPI.Services;

namespace CNPJAnalyzerWebAPI.Analyzers
{
    public class LanguageSpecificAnalyzer
    {
        private readonly IRecommendationService _recommendationService = new RecommendationService();

        public CNPJAnalysisResult AnalyzePattern(string filePath, int lineNumber, string line, 
            Match match, string extension)
        {
            var pattern = match.Value;
            var issues = new List<string>();
            var severity = "Medium";
            var issueType = "LanguageSpecific";

            switch (extension)
            {
                case ".cs":
                    AnalyzeCSharpPattern(pattern, issues, ref severity, ref issueType);
                    break;
                case ".java":
                    AnalyzeJavaPattern(pattern, issues, ref severity, ref issueType);
                    break;
                case ".js":
                case ".ts":
                    AnalyzeJavaScriptPattern(pattern, issues, ref severity, ref issueType);
                    break;
                case ".py":
                    AnalyzePythonPattern(pattern, issues, ref severity, ref issueType);
                    break;
                case ".php":
                    AnalyzePhpPattern(pattern, issues, ref severity, ref issueType);
                    break;
                case ".go":
                    AnalyzeGoPattern(pattern, issues, ref severity, ref issueType);
                    break;
                case ".rb":
                    AnalyzeRubyPattern(pattern, issues, ref severity, ref issueType);
                    break;
                case ".sql":
                    AnalyzeSqlPattern(pattern, issues, ref severity, ref issueType);
                    break;
            }

            if (!issues.Any())
                return null;

            return new CNPJAnalysisResult
            {
                FilePath = filePath,
                LineNumber = lineNumber,
                LineContent = line.Trim(),
                DetectedCNPJ = $"Padrão {extension}: {pattern}",
                NeedsCorrection = true,
                RecommendedAction = _recommendationService.GetRecommendation(extension, issueType),
                IssueDescription = string.Join("; ", issues),
                IssueType = issueType,
                Severity = severity
            };
        }

        private void AnalyzeCSharpPattern(string pattern, List<string> issues, 
            ref string severity, ref string issueType)
        {
            if (pattern.Contains("Convert.ToUInt64") || pattern.Contains("Convert.ToInt64"))
            {
                issues.Add("Conversão numérica não suportará CNPJs alfanuméricos");
                severity = "High";
                issueType = "NumericConversion";
            }
            else if (pattern.Contains("ToString") && pattern.Contains("00\\.000\\.000"))
            {
                issues.Add("Formatação hardcoded incompatível com formato alfanumérico");
                severity = "High";
                issueType = "HardcodedFormat";
            }
            else if (pattern.Contains("long.Parse") || pattern.Contains("int.Parse"))
            {
                issues.Add("Parse numérico falhará com CNPJs alfanuméricos");
                severity = "High";
                issueType = "NumericConversion";
            }
        }

        private void AnalyzeJavaPattern(string pattern, List<string> issues, 
            ref string severity, ref string issueType)
        {
            if (pattern.Contains("Integer.parseInt") || pattern.Contains("Long.parseLong"))
            {
                issues.Add("Parsing numérico falhará com CNPJs alfanuméricos");
                severity = "High";
                issueType = "NumericConversion";
            }
            else if (pattern.Contains("BigInteger"))
            {
                issues.Add("BigInteger não suportará caracteres alfanuméricos");
                severity = "High";
                issueType = "NumericConversion";
            }
            else if (pattern.Contains("String.format") && pattern.Contains("%02d"))
            {
                issues.Add("Formatação numérica não funcionará com caracteres alfanuméricos");
                severity = "High";
                issueType = "HardcodedFormat";
            }
        }

        private void AnalyzeJavaScriptPattern(string pattern, List<string> issues, 
            ref string severity, ref string issueType)
        {
            if (pattern.Contains("parseInt") || pattern.Contains("parseFloat") || pattern.Contains("Number("))
            {
                issues.Add("Conversão numérica JavaScript incompatível");
                severity = "High";
                issueType = "NumericConversion";
            }
            else if (pattern.Contains("type") && pattern.Contains("number"))
            {
                issues.Add("Tipo 'number' não suportará CNPJs alfanuméricos");
                severity = "High";
                issueType = "TypeDefinition";
            }
        }

        private void AnalyzePythonPattern(string pattern, List<string> issues, 
            ref string severity, ref string issueType)
        {
            if (pattern.Contains("int(") || pattern.Contains("float("))
            {
                issues.Add("Conversão numérica Python incompatível");
                severity = "High";
                issueType = "NumericConversion";
            }
            else if (pattern.Contains("format") && pattern.Contains("{:02d}"))
            {
                issues.Add("Formatação numérica não funcionará com caracteres alfanuméricos");
                severity = "High";
                issueType = "HardcodedFormat";
            }
        }

        private void AnalyzePhpPattern(string pattern, List<string> issues, 
            ref string severity, ref string issueType)
        {
            if (pattern.Contains("intval") || pattern.Contains("floatval"))
            {
                issues.Add("Conversão numérica PHP incompatível");
                severity = "High";
                issueType = "NumericConversion";
            }
            else if (pattern.Contains("sprintf") && pattern.Contains("%02d"))
            {
                issues.Add("Formatação numérica não funcionará com caracteres alfanuméricos");
                severity = "High";
                issueType = "HardcodedFormat";
            }
        }

        private void AnalyzeGoPattern(string pattern, List<string> issues, 
            ref string severity, ref string issueType)
        {
            if (pattern.Contains("strconv.Atoi") || pattern.Contains("strconv.ParseInt"))
            {
                issues.Add("Conversão numérica Go incompatível");
                severity = "High";
                issueType = "NumericConversion";
            }
            else if (pattern.Contains("fmt.Sprintf") && pattern.Contains("%02d"))
            {
                issues.Add("Formatação numérica não funcionará com caracteres alfanuméricos");
                severity = "High";
                issueType = "HardcodedFormat";
            }
        }

        private void AnalyzeRubyPattern(string pattern, List<string> issues, 
            ref string severity, ref string issueType)
        {
            if (pattern.Contains(".to_i") || pattern.Contains(".to_f") || pattern.Contains("Integer("))
            {
                issues.Add("Conversão numérica Ruby incompatível");
                severity = "High";
                issueType = "NumericConversion";
            }
        }

        private void AnalyzeSqlPattern(string pattern, List<string> issues, 
            ref string severity, ref string issueType)
        {
            if (pattern.Contains("CAST") && pattern.Contains("BIGINT"))
            {
                issues.Add("CAST para BIGINT incompatível com CNPJs alfanuméricos");
                severity = "High";
                issueType = "SQLType";
            }
            else if (pattern.Contains("CONVERT") && pattern.Contains("BIGINT"))
            {
                issues.Add("CONVERT para BIGINT incompatível com CNPJs alfanuméricos");
                severity = "High";
                issueType = "SQLType";
            }
        }
    }
}