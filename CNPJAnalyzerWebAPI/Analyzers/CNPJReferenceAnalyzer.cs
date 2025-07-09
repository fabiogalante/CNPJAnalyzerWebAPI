using CNPJAnalyzerWebAPI.Entities;
using CNPJAnalyzerWebAPI.Services;

namespace CNPJAnalyzerWebAPI.Analyzers
{
    public class CNPJReferenceAnalyzer
    {
        private readonly IRecommendationService _recommendationService;

        public CNPJReferenceAnalyzer()
        {
            _recommendationService = new RecommendationService();
        }

        public CNPJAnalysisResult Analyze(string filePath, int lineNumber, string line)
        {
            var issues = new List<string>();
            var severity = "Info";
            var issueType = "CNPJReference";
            var needsCorrection = false;

            // Verificar problemas em ordem de prioridade
            if (PatternAnalyzer.HasNumericConversion(line))
            {
                issues.Add("Conversão numérica não funcionará com CNPJs alfanuméricos");
                severity = "High";
                needsCorrection = true;
                issueType = "NumericConversion";
            }
            else if (PatternAnalyzer.HasHardcodedFormatting(line))
            {
                issues.Add("Formatação hardcoded pode não funcionar com CNPJs alfanuméricos");
                severity = "High";
                needsCorrection = true;
                issueType = "HardcodedFormat";
            }
            else if (PatternAnalyzer.HasRigidLengthValidation(line))
            {
                issues.Add("Validação de tamanho muito rígida");
                severity = "Medium";
                needsCorrection = true;
                issueType = "LengthValidation";
            }
            else if (PatternAnalyzer.IsNumericOnlyValidation(line))
            {
                issues.Add("Validação assume apenas números");
                severity = "High";
                needsCorrection = true;
                issueType = "NumericValidation";
            }
            else if (PatternAnalyzer.HasNumericSQLType(line))
            {
                issues.Add("Tipo de dados SQL inadequado");
                severity = "High";
                needsCorrection = true;
                issueType = "SQLType";
            }
            else if (PatternAnalyzer.HasNumericCNPJType(line))
            {
                issues.Add("Declaração de variável CNPJ com tipo numérico");
                severity = "High";
                needsCorrection = true;
                issueType = "NumericCNPJType";
            }

            if (!issues.Any())
                return null;

            return new CNPJAnalysisResult
            {
                FilePath = filePath,
                LineNumber = lineNumber,
                LineContent = line.Trim(),
                DetectedCNPJ = "Referência a CNPJ detectada",
                NeedsCorrection = needsCorrection,
                RecommendedAction = _recommendationService.GetRecommendation(Path.GetExtension(filePath), issueType),
                IssueDescription = string.Join("; ", issues),
                IssueType = issueType,
                Severity = severity
            };
        }
    }
}