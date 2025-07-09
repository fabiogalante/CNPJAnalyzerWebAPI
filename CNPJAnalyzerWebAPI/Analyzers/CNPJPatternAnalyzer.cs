using System.Text.RegularExpressions;
using CNPJAnalyzerWebAPI.Entities;
using CNPJAnalyzerWebAPI.Services;
using CNPJAnalyzerWebAPI.Validators;

namespace CNPJAnalyzerWebAPI.Analyzers
{
    public class CNPJPatternAnalyzer
    {
        private readonly IRecommendationService _recommendationService;

        public CNPJPatternAnalyzer()
        {
            _recommendationService = new RecommendationService();
        }

        public CNPJAnalysisResult Analyze(string cnpj, string filePath, int lineNumber, string line)
        {
            string cleanCNPJ = Regex.Replace(cnpj, @"[^\d]", "");

            // Se é apenas um padrão de formatação ou conversão, não processar aqui
            if (cnpj.Contains("00\\.000\\.000\\/0000\\-00") || 
                cnpj.Contains("Convert.ToUInt64") || 
                cnpj.Contains("ToString("))
            {
                return null; // Já foi processado pela análise de referência
            }

            // Verificar se a linha contém padrões problemáticos mesmo sem CNPJ válido
            var hasProblematicPattern = PatternAnalyzer.HasNumericConversion(line) ||
                                        PatternAnalyzer.HasHardcodedFormatting(line) ||
                                        PatternAnalyzer.HasRigidLengthValidation(line);

            if (cleanCNPJ.Length != 14 || !CnpjValidator.IsValid(cleanCNPJ))
            {
                return null; // Não processar CNPJs inválidos aqui
            }

            var result = new CNPJAnalysisResult
            {
                FilePath = filePath,
                LineNumber = lineNumber,
                LineContent = line.Trim(),
                DetectedCNPJ = cnpj,
                NeedsCorrection = false,
                RecommendedAction = "Nenhuma ação necessária",
                IssueDescription = "CNPJ válido detectado",
                IssueType = "None",
                Severity = "Info"
            };

            var issues = new List<string>();

            // Análise de potenciais problemas
            if (PatternAnalyzer.IsNumericOnlyValidation(line))
            {
                UpdateResultForIssue(result, issues, "NumericValidation", "High", 
                    "Validação assume apenas números", "Atualizar validação para aceitar caracteres alfanuméricos");
            }
            else if (PatternAnalyzer.HasRigidLengthValidation(line))
            {
                UpdateResultForIssue(result, issues, "LengthValidation", "Medium", 
                    "Validação de tamanho muito rígida", "Flexibilizar validação de tamanho");
            }
            else if (PatternAnalyzer.HasNumericSQLType(line))
            {
                UpdateResultForIssue(result, issues, "SQLType", "High", 
                    "Tipo de dados SQL inadequado", "Alterar tipo de coluna para VARCHAR/NVARCHAR");
            }
            else if (PatternAnalyzer.HasNumericCNPJType(line))
            {
                UpdateResultForIssue(result, issues, "NumericCNPJType", "High", 
                    "Declaração de variável CNPJ com tipo numérico", "Alterar tipo de variável para string ou VARCHAR");
            }
            else if (PatternAnalyzer.HasHardcodedFormatting(line))
            {
                UpdateResultForIssue(result, issues, "HardcodedFormat", "High", 
                    "Formatação hardcoded pode não funcionar com CNPJs alfanuméricos", 
                    "Remover formatação hardcoded e usar validação dinâmica");
            }
            else if (hasProblematicPattern)
            {
                UpdateResultForIssue(result, issues, "PotentialIssue", "Medium", 
                    "Linha contém padrões problemáticos que podem afetar a validação de CNPJ", 
                    "Revisar lógica de validação e conversão");
            }

            result.IssueDescription = issues.Any() ? string.Join("; ", issues) : "CNPJ válido detectado";
            return result;
        }

        private void UpdateResultForIssue(CNPJAnalysisResult result, List<string> issues, 
            string issueType, string severity, string issueDescription, string recommendedAction)
        {
            result.NeedsCorrection = true;
            result.IssueType = issueType;
            result.Severity = severity;
            result.RecommendedAction = recommendedAction;
            issues.Add(issueDescription);
        }
    }
}