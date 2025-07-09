namespace CNPJAnalyzerWebAPI.Services
{
    public interface IRecommendationService
    {
        string GetRecommendation(string extension, string issueType);
    }

    public class RecommendationService : IRecommendationService
    {
        public string GetRecommendation(string extension, string issueType)
        {
            return extension switch
            {
                ".cs" => GetCSharpRecommendation(issueType),
                ".java" => GetJavaRecommendation(issueType),
                ".js" => GetJavaScriptRecommendation(issueType),
                ".ts" => GetTypeScriptRecommendation(issueType),
                ".py" => GetPythonRecommendation(issueType),
                ".php" => GetPhpRecommendation(issueType),
                ".go" => GetGoRecommendation(issueType),
                ".rb" => GetRubyRecommendation(issueType),
                ".sql" => GetSqlRecommendation(issueType),
                _ => GetGenericRecommendation(issueType)
            };
        }

        private string GetCSharpRecommendation(string issueType)
        {
            return issueType switch
            {
                "NumericConversion" => "Usar string.IsNullOrEmpty() ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com StringBuilder ou Regex",
                "LengthValidation" => "Usar validação flexível: cnpj?.Length >= 11 && cnpj.Length <= 18",
                "SQLType" => "Alterar para NVARCHAR(18) ou VARCHAR(18)",
                _ => "Revisar código C# para compatibilidade"
            };
        }

        private string GetJavaRecommendation(string issueType)
        {
            return issueType switch
            {
                "NumericConversion" => "Usar String.isEmpty() ao invés de parsing numérico",
                "HardcodedFormat" => "Implementar formatação dinâmica com StringBuilder",
                "LengthValidation" => "Usar validação flexível: cnpj.length() >= 11 && cnpj.length() <= 18",
                _ => "Revisar código Java para compatibilidade"
            };
        }

        private string GetJavaScriptRecommendation(string issueType)
        {
            return issueType switch
            {
                "NumericConversion" => "Usar typeof cnpj === 'string' ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com template strings",
                "TypeDefinition" => "Alterar tipo para string",
                _ => "Revisar código JavaScript para compatibilidade"
            };
        }

        private string GetTypeScriptRecommendation(string issueType)
        {
            return issueType switch
            {
                "NumericConversion" => "Usar typeof cnpj === 'string' ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com template strings",
                "TypeDefinition" => "Alterar tipo para string",
                _ => "Revisar código TypeScript para compatibilidade"
            };
        }

        private string GetPythonRecommendation(string issueType)
        {
            return issueType switch
            {
                "NumericConversion" => "Usar isinstance(cnpj, str) ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com f-strings",
                _ => "Revisar código Python para compatibilidade"
            };
        }

        private string GetPhpRecommendation(string issueType)
        {
            return issueType switch
            {
                "NumericConversion" => "Usar is_string($cnpj) ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com substr()",
                _ => "Revisar código PHP para compatibilidade"
            };
        }

        private string GetGoRecommendation(string issueType)
        {
            return issueType switch
            {
                "NumericConversion" => "Usar validação de string ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com strings.Builder",
                _ => "Revisar código Go para compatibilidade"
            };
        }

        private string GetRubyRecommendation(string issueType)
        {
            return issueType switch
            {
                "NumericConversion" => "Usar cnpj.is_a?(String) ao invés de conversões numéricas",
                _ => "Revisar código Ruby para compatibilidade"
            };
        }

        private string GetSqlRecommendation(string issueType)
        {
            return issueType switch
            {
                "SQLType" => "Alterar para VARCHAR(18) ou NVARCHAR(18)",
                _ => "Revisar queries SQL para compatibilidade"
            };
        }

        private string GetGenericRecommendation(string issueType)
        {
            return issueType switch
            {
                "NumericConversion" => "Remover conversões numéricas e tratar CNPJ como string",
                "HardcodedFormat" => "Implementar formatação dinâmica que suporte caracteres alfanuméricos",
                "LengthValidation" => "Flexibilizar validação de tamanho para aceitar variações",
                "SQLType" => "Alterar tipo de coluna para VARCHAR/NVARCHAR",
                _ => "Revisar código para compatibilidade com CNPJs alfanuméricos"
            };
        }
    }
}