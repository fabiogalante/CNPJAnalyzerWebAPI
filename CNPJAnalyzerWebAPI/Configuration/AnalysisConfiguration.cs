using System.Text.RegularExpressions;

namespace CNPJAnalyzerWebAPI.Configuration
{
    public static class AnalysisConfiguration
    {
        public static readonly Dictionary<string, string[]> LanguageKeywords = new()
        {
            [".cs"] = new[] { "cnpj", "CNPJ", "FormatCnpj", "formatCnpj", "cnpjFormat", "CnpjFormat" },
            [".java"] = new[] { "cnpj", "CNPJ", "formatCnpj", "cnpjFormat", "CnpjFormat" },
            [".js"] = new[] { "cnpj", "CNPJ", "formatCnpj", "cnpjFormat", "format_cnpj" },
            [".ts"] = new[] { "cnpj", "CNPJ", "formatCnpj", "cnpjFormat", "format_cnpj" },
            [".py"] = new[] { "cnpj", "CNPJ", "format_cnpj", "cnpj_format", "formatCnpj" },
            [".php"] = new[] { "cnpj", "CNPJ", "format_cnpj", "$cnpj", "formatCnpj" },
            [".rb"] = new[] { "cnpj", "CNPJ", "format_cnpj", "cnpj_format", "formatCnpj" },
            [".go"] = new[] { "cnpj", "CNPJ", "FormatCnpj", "formatCnpj", "cnpjFormat" },
            [".sql"] = new[] { "cnpj", "CNPJ", "cnpj_column", "cnpj_field" }
        };

        public static readonly string[] SupportedExtensions =
        {
            ".cs", ".js", ".ts", ".json", ".xml", ".config", ".txt", ".sql",
            ".html", ".htm", ".css", ".java", ".py", ".php", ".rb", ".go",
            ".cpp", ".c", ".h", ".hpp", ".yaml", ".yml", ".properties", ".ini",
            ".razor", ".cshtml", ".vb", ".fs", ".scala", ".kt", ".swift", ".dart",
            ".pl", ".sh", ".bat", ".ps1", ".vue", ".jsx", ".tsx", ".scss", ".less"
        };

        public static readonly string[] IgnorePaths =
        {
            "node_modules", "bin/", "obj/", "packages/", ".git/", "vendor/",
            ".vscode/", ".idea/", "target/", "build/", "dist/", "out/",
            "__pycache__/", ".pytest_cache/", "venv/", "env/", ".env/",
            "coverage/", ".nyc_output/", ".sass-cache/", "bower_components/"
        };

        public static readonly Dictionary<string, Regex[]> LanguageSpecificPatterns = new()
        {
            // C# e .NET
            [".cs"] = new[]
            {
                new Regex(@"Convert\.ToUInt64\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"Convert\.ToInt64\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"long\.Parse\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"int\.Parse\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"ToString\s*\(\s*@?['""]([^'""]*00\\\.000\\\.000\\\/0000\\-00[^'""]*)['""]",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },

            // Java
            [".java"] = new[]
            {
                new Regex(@"Integer\.parseInt\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"Long\.parseLong\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"BigInteger\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\.length\s*\(\s*\)\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"String\.format\s*\(\s*['""]([^'""]*%02d\.%03d\.%03d\/%04d-%02d[^'""]*)['""]",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },

            // JavaScript/TypeScript
            [".js"] = new[]
            {
                new Regex(@"parseInt\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"parseFloat\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"Number\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\.length\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"type\s*:\s*['""]number['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },

            // Python
            [".py"] = new[]
            {
                new Regex(@"int\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"float\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"len\s*\(\s*\w*cnpj\w*\s*\)\s*[!=<>]+\s*14",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\.format\s*\(\s*['""]([^'""]*\{:02d\}\.\{:03d\}\.\{:03d\}\/\{:04d\}-\{:02d\}[^'""]*)['""]",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },

            // PHP
            [".php"] = new[]
            {
                new Regex(@"intval\s*\(\s*\$\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"floatval\s*\(\s*\$\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"strlen\s*\(\s*\$\w*cnpj\w*\s*\)\s*[!=<>]+\s*14",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"sprintf\s*\(\s*['""]([^'""]*%02d\.%03d\.%03d\/%04d-%02d[^'""]*)['""]",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },

            // Go
            [".go"] = new[]
            {
                new Regex(@"strconv\.Atoi\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"strconv\.ParseInt\s*\(\s*\w*cnpj\w*\s*,", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"len\s*\(\s*\w*cnpj\w*\s*\)\s*[!=<>]+\s*14",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"fmt\.Sprintf\s*\(\s*['""]([^'""]*%02d\.%03d\.%03d\/%04d-%02d[^'""]*)['""]",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },

            // Ruby
            [".rb"] = new[]
            {
                new Regex(@"\.to_i\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\.to_f\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"Integer\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\.length\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },

            // SQL
            [".sql"] = new[]
            {
                new Regex(@"CAST\s*\(\s*\w*cnpj\w*\s+AS\s+BIGINT\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"CONVERT\s*\(\s*BIGINT\s*,\s*\w*cnpj\w*\s*\)",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"LEN\s*\(\s*\w*cnpj\w*\s*\)\s*[!=<>]+\s*14",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"LENGTH\s*\(\s*\w*cnpj\w*\s*\)\s*[!=<>]+\s*14",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase),
            },

            // TypeScript
            [".ts"] = new[]
            {
                new Regex(@"parseInt\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"parseFloat\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"Number\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\.length\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"type\s*:\s*['""]number['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            }
        };

        public static readonly Regex[] CNPJPatterns =
        {
            // Padrões básicos de CNPJ
            new Regex(@"\b\d{2}\.?\d{3}\.?\d{3}\/?\d{4}-?\d{2}\b", RegexOptions.Compiled),
            new Regex(@"\b\d{14}\b", RegexOptions.Compiled),
        
            // Padrões em atribuições (multilinguagem)
            new Regex(@"(?:cnpj|document|registro|taxId|companyId)\s*[:=]\s*['""]?([0-9./\-]{14,18})['""]?", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"\$(?:cnpj|document|registro|taxId|companyId)\s*=\s*['""]?([0-9./\-]{14,18})['""]?", RegexOptions.Compiled | RegexOptions.IgnoreCase), // PHP
            new Regex(@"(?:cnpj|document|registro|taxId|companyId)\s*=\s*['""]?([0-9./\-]{14,18})['""]?", RegexOptions.Compiled | RegexOptions.IgnoreCase), // Python/Ruby
        
            // Padrões para validação de tamanho (multilinguagem)
            new Regex(@"(?:cnpj|document)\.(?:length|Length|len|size)\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(?:len|length|strlen|LEN|LENGTH)\s*\(\s*\$?\w*cnpj\w*\s*\)\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        };
    }
}