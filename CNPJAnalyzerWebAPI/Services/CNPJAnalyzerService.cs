using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using CNPJAnalyzerWebAPI.Entities;

namespace CNPJAnalyzerWebAPI.Services;

public class CNPJAnalyzerService
{
    private static readonly Dictionary<string, Regex[]> LanguageSpecificPatterns = new()
    {
        // C# e .NET
        [".cs"] = new[]
        {
            new Regex(@"Convert\.ToUInt64\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"Convert\.ToInt64\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"long\.Parse\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"int\.Parse\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"ToString\s*\(\s*@?['""]([^'""]*00\\\.000\\\.000\\\/0000\\-00[^'""]*)['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        },
        
        // Java
        [".java"] = new[]
        {
            new Regex(@"Integer\.parseInt\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"Long\.parseLong\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"BigInteger\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"\.length\s*\(\s*\)\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"String\.format\s*\(\s*['""]([^'""]*%02d\.%03d\.%03d\/%04d-%02d[^'""]*)['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase),
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
            new Regex(@"len\s*\(\s*\w*cnpj\w*\s*\)\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"\.format\s*\(\s*['""]([^'""]*\{:02d\}\.\{:03d\}\.\{:03d\}\/\{:04d\}-\{:02d\}[^'""]*)['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        },
        
        // PHP
        [".php"] = new[]
        {
            new Regex(@"intval\s*\(\s*\$\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"floatval\s*\(\s*\$\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"strlen\s*\(\s*\$\w*cnpj\w*\s*\)\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"sprintf\s*\(\s*['""]([^'""]*%02d\.%03d\.%03d\/%04d-%02d[^'""]*)['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        },
        
        // Go
        [".go"] = new[]
        {
            new Regex(@"strconv\.Atoi\s*\(\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"strconv\.ParseInt\s*\(\s*\w*cnpj\w*\s*,", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"len\s*\(\s*\w*cnpj\w*\s*\)\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"fmt\.Sprintf\s*\(\s*['""]([^'""]*%02d\.%03d\.%03d\/%04d-%02d[^'""]*)['""]", RegexOptions.Compiled | RegexOptions.IgnoreCase),
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
            new Regex(@"CONVERT\s*\(\s*BIGINT\s*,\s*\w*cnpj\w*\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"LEN\s*\(\s*\w*cnpj\w*\s*\)\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"LENGTH\s*\(\s*\w*cnpj\w*\s*\)\s*[!=<>]+\s*14", RegexOptions.Compiled | RegexOptions.IgnoreCase),
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

    private static readonly Regex[] CNPJPatterns =
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

    private static readonly Dictionary<string, string[]> LanguageKeywords = new()
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

    private static readonly string[] SupportedExtensions =
    {
        ".cs", ".js", ".ts", ".json", ".xml", ".config", ".txt", ".sql",
        ".html", ".htm", ".css", ".java", ".py", ".php", ".rb", ".go",
        ".cpp", ".c", ".h", ".hpp", ".yaml", ".yml", ".properties", ".ini",
        ".razor", ".cshtml", ".vb", ".fs", ".scala", ".kt", ".swift", ".dart",
        ".pl", ".sh", ".bat", ".ps1", ".vue", ".jsx", ".tsx", ".scss", ".less"
    };
    
    private static readonly string[] IgnorePaths =
    {
        "node_modules", "bin/", "obj/", "packages/", ".git/", "vendor/",
        ".vscode/", ".idea/", "target/", "build/", "dist/", "out/",
        "__pycache__/", ".pytest_cache/", "venv/", "env/", ".env/",
        "coverage/", ".nyc_output/", ".sass-cache/", "bower_components/"
    };

    public async Task<AnalysisResponse> AnalyzeZipFileAsync(Stream zipStream)
    {
        var results = new List<CNPJAnalysisResult>();
        int totalFiles = 0;

        try
        {
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                foreach (var entry in archive.Entries)
                {
                    if (ShouldAnalyzeFile(entry.FullName))
                    {
                        totalFiles++;
                        var entryResults = await AnalyzeFileEntryAsync(entry);
                        results.AddRange(entryResults);
                    }
                }
            }

            var stats = GenerateStats(totalFiles, results);
            var reportHtml = GenerateHtmlReport(results, stats);

            return new AnalysisResponse
            {
                Success = true,
                Message = "Análise concluída com sucesso",
                Stats = stats,
                Results = results,
                ReportHtml = reportHtml
            };
        }
        catch (Exception ex)
        {
            return new AnalysisResponse
            {
                Success = false,
                Message = $"Erro ao processar arquivo ZIP: {ex.Message}",
                Results = new List<CNPJAnalysisResult>()
            };
        }
    }

    private bool ShouldAnalyzeFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        
        // Verificar se a extensão é suportada
        if (!SupportedExtensions.Contains(extension))
            return false;
        
        // Usar o array IgnorePaths para verificação mais robusta
        return !IgnorePaths.Any(ignorePath => 
            fileName.Contains(ignorePath, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<List<CNPJAnalysisResult>> AnalyzeFileEntryAsync(ZipArchiveEntry entry)
    {
        var results = new List<CNPJAnalysisResult>();

        try
        {
            using (var stream = entry.Open())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string content = await reader.ReadToEndAsync();
                string[] lines = content.Split('\n');

                for (int i = 0; i < lines.Length; i++)
                {
                    var lineResults = AnalyzeLine(entry.FullName, i + 1, lines[i]);
                    results.AddRange(lineResults);
                }
            }
        }
        catch (Exception ex)
        {
            results.Add(new CNPJAnalysisResult
            {
                FilePath = entry.FullName,
                LineNumber = 0,
                LineContent = "Erro ao ler arquivo",
                DetectedCNPJ = "",
                NeedsCorrection = true,
                RecommendedAction = "Verificar manualmente",
                IssueDescription = $"Erro ao ler arquivo: {ex.Message}",
                IssueType = "ReadError",
                Severity = "High"
            });
        }

        return RemoveDuplicates(results);
    }

    private List<CNPJAnalysisResult> RemoveDuplicates(List<CNPJAnalysisResult> results)
    {
        return results
            .GroupBy(r => new { r.FilePath, r.LineNumber, r.IssueType })
            .Select(g => g.First())
            .ToList();
    }

    private List<CNPJAnalysisResult> AnalyzeLine(string filePath, int lineNumber, string line)
    {
        var results = new List<CNPJAnalysisResult>();
        var foundMatches = new HashSet<string>();
        
        // Obter extensão do arquivo
        var extension = Path.GetExtension(filePath).ToLower();
        
        // 1. Usar padrões específicos da linguagem SE existirem
        if (LanguageSpecificPatterns.ContainsKey(extension))
        {
            foreach (var pattern in LanguageSpecificPatterns[extension])
            {
                var matches = pattern.Matches(line);
                foreach (Match match in matches)
                {
                    var analysis = AnalyzeLanguageSpecificPattern(filePath, lineNumber, line, match, extension);
                    if (analysis != null && !foundMatches.Contains(analysis.DetectedCNPJ))
                    {
                        results.Add(analysis);
                        foundMatches.Add(analysis.DetectedCNPJ);
                    }
                }
            }
        }
        
        // 2. Verificar se a linha contém referências a CNPJ
        if (ContainsCNPJReference(line, filePath))
        {
            var potentialIssue = AnalyzeCNPJReferenceLine(filePath, lineNumber, line);
            if (potentialIssue != null && !foundMatches.Contains(potentialIssue.DetectedCNPJ))
            {
                results.Add(potentialIssue);
                foundMatches.Add(potentialIssue.DetectedCNPJ);
            }
        }

        // 3. Procurar por padrões de CNPJ específicos
        foreach (var pattern in CNPJPatterns)
        {
            var matches = pattern.Matches(line);
            foreach (Match match in matches)
            {
                string cnpj = match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
            
                if (!foundMatches.Contains(cnpj))
                {
                    var analysis = AnalyzeCNPJ(cnpj, filePath, lineNumber, line);
                    if (analysis != null)
                    {
                        results.Add(analysis);
                        foundMatches.Add(cnpj);
                    }
                }
            }
        }

        return results;
    }

    private CNPJAnalysisResult AnalyzeLanguageSpecificPattern(string filePath, int lineNumber, string line, Match match, string extension)
    {
        var pattern = match.Value;
        var issues = new List<string>();
        var severity = "Medium";
        var issueType = "LanguageSpecific";
        
        // Análise específica por linguagem
        switch (extension)
        {
            case ".cs":
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
                break;
                
            case ".java":
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
                break;
                
            case ".js":
            case ".ts":
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
                break;
                
            case ".py":
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
                break;
                
            case ".php":
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
                break;
                
            case ".go":
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
                break;
                
            case ".rb":
                if (pattern.Contains(".to_i") || pattern.Contains(".to_f") || pattern.Contains("Integer("))
                {
                    issues.Add("Conversão numérica Ruby incompatível");
                    severity = "High";
                    issueType = "NumericConversion";
                }
                break;
                
            case ".sql":
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
                break;
        }
        
        if (issues.Any())
        {
            return new CNPJAnalysisResult
            {
                FilePath = filePath,
                LineNumber = lineNumber,
                LineContent = line.Trim(),
                DetectedCNPJ = $"Padrão {extension}: {pattern}",
                NeedsCorrection = true,
                RecommendedAction = GetRecommendationForLanguage(extension, issueType),
                IssueDescription = string.Join("; ", issues),
                IssueType = issueType,
                Severity = severity
            };
        }
        
        return null;
    }

    private bool ContainsCNPJReference(string line, string filePath = "")
    {
        var extension = Path.GetExtension(filePath).ToLower();
        
        // Usar keywords específicas da linguagem se disponíveis
        if (LanguageKeywords.ContainsKey(extension))
        {
            return LanguageKeywords[extension].Any(keyword => 
                line.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        
        // Fallback para keywords gerais
        var keywords = new[] { "cnpj", "CNPJ", "FormatCnpj", "formatCnpj", "cnpj_format", "cnpjFormat" };
        return keywords.Any(keyword => line.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private CNPJAnalysisResult AnalyzeCNPJReferenceLine(string filePath, int lineNumber, string line)
    {
        var issues = new List<string>();
        var severity = "Info";
        var issueType = "CNPJReference";
        var needsCorrection = false;

        // Verificar problemas em ordem de prioridade
        if (HasNumericConversion(line))
        {
            issues.Add("Conversão numérica não funcionará com CNPJs alfanuméricos");
            severity = "High";
            needsCorrection = true;
            issueType = "NumericConversion";
        }
        else if (HasHardcodedFormatting(line))
        {
            issues.Add("Formatação hardcoded pode não funcionar com CNPJs alfanuméricos");
            severity = "High";
            needsCorrection = true;
            issueType = "HardcodedFormat";
        }
        else if (HasRigidLengthValidation(line))
        {
            issues.Add("Validação de tamanho muito rígida");
            severity = "Medium";
            needsCorrection = true;
            issueType = "LengthValidation";
        }
        else if (IsNumericOnlyValidation(line))
        {
            issues.Add("Validação assume apenas números");
            severity = "High";
            needsCorrection = true;
            issueType = "NumericValidation";
        }
        else if (HasNumericSQLType(line))
        {
            issues.Add("Tipo de dados SQL inadequado");
            severity = "High";
            needsCorrection = true;
            issueType = "SQLType";
        }

        // Só retornar resultado se há problemas reais
        if (issues.Any())
        {
            return new CNPJAnalysisResult
            {
                FilePath = filePath,
                LineNumber = lineNumber,
                LineContent = line.Trim(),
                DetectedCNPJ = "Referência a CNPJ detectada",
                NeedsCorrection = needsCorrection,
                RecommendedAction = GetRecommendationForLanguage(Path.GetExtension(filePath), issueType),
                IssueDescription = string.Join("; ", issues),
                IssueType = issueType,
                Severity = severity
            };
        }

        return null;
    }

    private CNPJAnalysisResult AnalyzeCNPJ(string cnpj, string filePath, int lineNumber, string line)
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
        var hasProblematicPattern = HasNumericConversion(line) ||
                                    HasHardcodedFormatting(line) ||
                                    HasRigidLengthValidation(line);

        if (cleanCNPJ.Length != 14 || !IsValidCNPJ(cleanCNPJ))
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
        if (IsNumericOnlyValidation(line))
        {
            result.NeedsCorrection = true;
            result.IssueType = "NumericValidation";
            result.Severity = "High";
            issues.Add("Validação assume apenas números");
            result.RecommendedAction = "Atualizar validação para aceitar caracteres alfanuméricos";
        }
        else if (HasRigidLengthValidation(line))
        {
            result.NeedsCorrection = true;
            result.IssueType = "LengthValidation";
            result.Severity = "Medium";
            issues.Add("Validação de tamanho muito rígida");
            result.RecommendedAction = "Flexibilizar validação de tamanho";
        }
        else if (HasNumericSQLType(line))
        {
            result.NeedsCorrection = true;
            result.IssueType = "SQLType";
            result.Severity = "High";
            issues.Add("Tipo de dados SQL inadequado");
            result.RecommendedAction = "Alterar tipo de coluna para VARCHAR/NVARCHAR";
        }

        result.IssueDescription = issues.Any() ? string.Join("; ", issues) : "CNPJ válido detectado";
        return result;
    }

    private bool IsNumericOnlyValidation(string line)
    {
        var patterns = new[]
        {
            @"IsNumeric\s*\(", @"int\.Parse\s*\(", @"Convert\.ToInt",
            @"[Tt]ype\s*==?\s*['""]?number['""]?", @"[Dd]ataType\s*==?\s*['""]?[Nn]umber['""]?",
            @"[Rr]egex.*\[\^\\d\]", @"[Oo]nly.*[Nn]umber", @"[Nn]umeric.*[Oo]nly"
        };
        return patterns.Any(pattern => Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
    }

    private bool HasRigidLengthValidation(string line)
    {
        var patterns = new[]
        {
            @"Length\s*==\s*14", @"len\s*==\s*14", @"size\s*==\s*14",
            @"maxlength\s*=\s*['""]?14['""]?", @"minlength\s*=\s*['""]?14['""]?"
        };
        return patterns.Any(pattern => Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
    }

    private bool HasHardcodedFormatting(string line)
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

    private bool HasNumericConversion(string line)
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

    private bool HasNumericSQLType(string line)
    {
        var patterns = new[]
        {
            @"BIGINT", @"NUMERIC\s*\(\s*14", @"DECIMAL\s*\(\s*14",
            @"INT\s+cnpj", @"BIGINT\s+cnpj", @"cnpj\s+BIGINT", @"cnpj\s+INT"
        };
        return patterns.Any(pattern => Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase));
    }

    private bool IsValidCNPJ(string cnpj)
    {
        if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0])) return false;

        int[] weights1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] weights2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        int sum1 = 0, sum2 = 0;
        for (int i = 0; i < 12; i++)
        {
            sum1 += int.Parse(cnpj[i].ToString()) * weights1[i];
            sum2 += int.Parse(cnpj[i].ToString()) * weights2[i];
        }

        int digit1 = sum1 % 11 < 2 ? 0 : 11 - (sum1 % 11);
        sum2 += digit1 * weights2[12];
        int digit2 = sum2 % 11 < 2 ? 0 : 11 - (sum2 % 11);

        return digit1 == int.Parse(cnpj[12].ToString()) && digit2 == int.Parse(cnpj[13].ToString());
    }

    private string GetRecommendationForLanguage(string extension, string issueType)
    {
        return extension switch
        {
            ".cs" => issueType switch
            {
                "NumericConversion" => "Usar string.IsNullOrEmpty() ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com StringBuilder ou Regex",
                "LengthValidation" => "Usar validação flexível: cnpj?.Length >= 11 && cnpj.Length <= 18",
                "SQLType" => "Alterar para NVARCHAR(18) ou VARCHAR(18)",
                _ => "Revisar código C# para compatibilidade"
            },
            ".java" => issueType switch
            {
                "NumericConversion" => "Usar String.isEmpty() ao invés de parsing numérico",
                "HardcodedFormat" => "Implementar formatação dinâmica com StringBuilder",
                "LengthValidation" => "Usar validação flexível: cnpj.length() >= 11 && cnpj.length() <= 18",
                _ => "Revisar código Java para compatibilidade"
            },
            ".js" => issueType switch
            {
                "NumericConversion" => "Usar typeof cnpj === 'string' ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com template strings",
                "TypeDefinition" => "Alterar tipo para string",
                _ => "Revisar código JavaScript para compatibilidade"
            },
            ".ts" => issueType switch
            {
                "NumericConversion" => "Usar typeof cnpj === 'string' ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com template strings",
                "TypeDefinition" => "Alterar tipo para string",
                _ => "Revisar código TypeScript para compatibilidade"
            },
            ".py" => issueType switch
            {
                "NumericConversion" => "Usar isinstance(cnpj, str) ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com f-strings",
                _ => "Revisar código Python para compatibilidade"
            },
            ".php" => issueType switch
            {
                "NumericConversion" => "Usar is_string($cnpj) ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com substr()",
                _ => "Revisar código PHP para compatibilidade"
            },
            ".go" => issueType switch
            {
                "NumericConversion" => "Usar validação de string ao invés de conversões numéricas",
                "HardcodedFormat" => "Implementar formatação dinâmica com strings.Builder",
                _ => "Revisar código Go para compatibilidade"
            },
            ".rb" => issueType switch
            {
                "NumericConversion" => "Usar cnpj.is_a?(String) ao invés de conversões numéricas",
                _ => "Revisar código Ruby para compatibilidade"
            },
            ".sql" => issueType switch
            {
                "SQLType" => "Alterar para VARCHAR(18) ou NVARCHAR(18)",
                _ => "Revisar queries SQL para compatibilidade"
            },
            _ => GetRecommendationForIssueType(issueType)
        };
    }

    private string GetRecommendationForIssueType(string issueType)
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

    private AnalysisStats GenerateStats(int totalFiles, List<CNPJAnalysisResult> results)
    {
        var issuesByType = results.Where(r => r.NeedsCorrection)
            .GroupBy(r => r.IssueType)
            .ToDictionary(g => g.Key, g => g.Count());

        var issuesBySeverity = results.Where(r => r.NeedsCorrection)
            .GroupBy(r => r.Severity)
            .ToDictionary(g => g.Key, g => g.Count());

        return new AnalysisStats
        {
            TotalFiles = totalFiles,
            FilesWithCNPJ = results.GroupBy(r => r.FilePath).Count(),
            TotalCNPJs = results.Count,
            CNPJsNeedingCorrection = results.Count(r => r.NeedsCorrection),
            IssuesByType = issuesByType,
            IssuesBySeverity = issuesBySeverity
        };
    }

    private string GenerateHtmlReport(List<CNPJAnalysisResult> results, AnalysisStats stats)
    {
        var sb = new StringBuilder();
        sb.AppendLine($@"
<!DOCTYPE html>
<html>
<head>
    <title>Relatório de Análise CNPJ</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        .header {{ background: #f8f9fa; padding: 20px; border-radius: 5px; margin-bottom: 20px; }}
        .stats {{ display: flex; gap: 20px; margin-bottom: 20px; }}
        .stat-card {{ background: white; padding: 15px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); flex: 1; }}
        .stat-number {{ font-size: 24px; font-weight: bold; color: #007bff; }}
        .stat-label {{ color: #6c757d; font-size: 14px; }}
        .severity-high {{ color: #dc3545; }}
        .severity-medium {{ color: #fd7e14; }}
        .severity-low {{ color: #28a745; }}
        .file-section {{ margin-bottom: 30px; }}
        .file-title {{ background: #e9ecef; padding: 10px; border-radius: 3px; font-weight: bold; }}
        .issue-item {{ margin: 10px 0; padding: 10px; background: #f8f9fa; border-radius: 3px; }}
        .line-number {{ color: #6c757d; font-weight: bold; }}
        .code {{ background: #e9ecef; padding: 5px; border-radius: 2px; font-family: monospace; }}
        .cnpj {{ color: #007bff; font-weight: bold; }}
        .language-tag {{ background: #6c757d; color: white; padding: 2px 6px; border-radius: 3px; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>📊 Relatório de Análise CNPJ</h1>
        <p>Análise de compatibilidade com novo formato alfanumérico</p>
        <p><strong>Data:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
    </div>

    <div class='stats'>
        <div class='stat-card'>
            <div class='stat-number'>{stats.TotalFiles}</div>
            <div class='stat-label'>Arquivos Analisados</div>
        </div>
        <div class='stat-card'>
            <div class='stat-number'>{stats.FilesWithCNPJ}</div>
            <div class='stat-label'>Arquivos com CNPJ</div>
        </div>
        <div class='stat-card'>
            <div class='stat-number'>{stats.TotalCNPJs}</div>
            <div class='stat-label'>Total de CNPJs</div>
        </div>
        <div class='stat-card'>
            <div class='stat-number severity-high'>{stats.CNPJsNeedingCorrection}</div>
            <div class='stat-label'>Requerem Correção</div>
        </div>
    </div>");

        if (stats.IssuesByType.Any())
        {
            sb.AppendLine("<h2>🔧 Problemas por Tipo</h2>");
            foreach (var issue in stats.IssuesByType.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"<p><strong>{issue.Key}:</strong> {issue.Value} ocorrências</p>");
            }
        }

        if (stats.IssuesBySeverity.Any())
        {
            sb.AppendLine("<h2>⚠️ Problemas por Severidade</h2>");
            foreach (var issue in stats.IssuesBySeverity.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"<p><strong>{issue.Key}:</strong> {issue.Value} ocorrências</p>");
            }
        }

        var groupedResults = results.GroupBy(r => r.FilePath);
        foreach (var fileGroup in groupedResults)
        {
            var extension = Path.GetExtension(fileGroup.Key).ToLower();
            sb.AppendLine($@"
    <div class='file-section'>
        <div class='file-title'>
            📄 {fileGroup.Key} 
            <span class='language-tag'>{extension}</span>
        </div>");

            foreach (var result in fileGroup.OrderBy(r => r.LineNumber))
            {
                var severityClass = $"severity-{result.Severity.ToLower()}";
                sb.AppendLine($@"
        <div class='issue-item'>
            <div><span class='line-number'>Linha {result.LineNumber}</span> - CNPJ: <span class='cnpj'>{result.DetectedCNPJ}</span></div>
            <div><strong>Status:</strong> <span class='{severityClass}'>{(result.NeedsCorrection ? "⚠️ REQUER CORREÇÃO" : "✅ OK")}</span></div>
            <div><strong>Tipo:</strong> {result.IssueType}</div>
            <div><strong>Severidade:</strong> <span class='{severityClass}'>{result.Severity}</span></div>
            <div><strong>Problema:</strong> {result.IssueDescription}</div>
            <div><strong>Ação Recomendada:</strong> {result.RecommendedAction}</div>
            <div><strong>Código:</strong> <code class='code'>{result.LineContent}</code></div>
        </div>");
            }

            sb.AppendLine("    </div>");
        }

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }
}