using System.Text.RegularExpressions;
using CNPJAnalyzerWebAPI.Configuration;
using CNPJAnalyzerWebAPI.Entities;

namespace CNPJAnalyzerWebAPI.Analyzers
{
    public class LineAnalyzer
    {
        public List<CNPJAnalysisResult> AnalyzeLine(string filePath, int lineNumber, string line)
        {
            var results = new List<CNPJAnalysisResult>();
            var foundMatches = new HashSet<string>();
            
            var extension = Path.GetExtension(filePath).ToLower();
            
            // 1. Análise específica por linguagem
            if (AnalysisConfiguration.LanguageSpecificPatterns.ContainsKey(extension))
            {
                var languageResults = AnalyzeLanguageSpecificPatterns(filePath, lineNumber, line, extension);
                AddUniqueResults(results, languageResults, foundMatches);
            }
            
            // 2. Análise de referências CNPJ
            if (ContainsCNPJReference(line, filePath))
            {
                var referenceResult = AnalyzeCNPJReferenceLine(filePath, lineNumber, line);
                if (referenceResult != null && !foundMatches.Contains(referenceResult.DetectedCNPJ))
                {
                    results.Add(referenceResult);
                    foundMatches.Add(referenceResult.DetectedCNPJ);
                }
            }
            
            // 3. Análise de padrões CNPJ
            var patternResults = AnalyzeCNPJPatterns(filePath, lineNumber, line);
            AddUniqueResults(results, patternResults, foundMatches);
            
            return results;
        }

        private void AddUniqueResults(List<CNPJAnalysisResult> results, 
            List<CNPJAnalysisResult> newResults, HashSet<string> foundMatches)
        {
            foreach (var result in newResults.Where(r => !foundMatches.Contains(r.DetectedCNPJ)))
            {
                results.Add(result);
                foundMatches.Add(result.DetectedCNPJ);
            }
        }

        private List<CNPJAnalysisResult> AnalyzeLanguageSpecificPatterns(string filePath, 
            int lineNumber, string line, string extension)
        {
            var results = new List<CNPJAnalysisResult>();
            
            if (!AnalysisConfiguration.LanguageSpecificPatterns.ContainsKey(extension))
                return results;

            foreach (var pattern in AnalysisConfiguration.LanguageSpecificPatterns[extension])
            {
                var matches = pattern.Matches(line);
                foreach (Match match in matches)
                {
                    var analysis = new LanguageSpecificAnalyzer()
                        .AnalyzePattern(filePath, lineNumber, line, match, extension);
                    if (analysis != null)
                        results.Add(analysis);
                }
            }
            
            return results;
        }

        private bool ContainsCNPJReference(string line, string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            
            if (AnalysisConfiguration.LanguageKeywords.ContainsKey(extension))
            {
                return AnalysisConfiguration.LanguageKeywords[extension]
                    .Any(keyword => line.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }
            
            var keywords = new[] { "cnpj", "CNPJ", "FormatCnpj", "formatCnpj", "cnpj_format", "cnpjFormat" };
            return keywords.Any(keyword => line.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private CNPJAnalysisResult AnalyzeCNPJReferenceLine(string filePath, int lineNumber, string line)
        {
            var analyzer = new CNPJReferenceAnalyzer();
            return analyzer.Analyze(filePath, lineNumber, line);
        }

        private List<CNPJAnalysisResult> AnalyzeCNPJPatterns(string filePath, int lineNumber, string line)
        {
            var results = new List<CNPJAnalysisResult>();
            
            foreach (var pattern in AnalysisConfiguration.CNPJPatterns)
            {
                var matches = pattern.Matches(line);
                foreach (Match match in matches)
                {
                    string cnpj = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
                    var analysis = new CNPJPatternAnalyzer().Analyze(cnpj, filePath, lineNumber, line);
                    if (analysis != null)
                        results.Add(analysis);
                }
            }
            
            return results;
        }
    }
}