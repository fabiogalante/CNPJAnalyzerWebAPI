using System.Text;
using CNPJAnalyzerWebAPI.Entities;

namespace CNPJAnalyzerWebAPI.Services
{
    public interface IReportGeneratorService
    {
        string GenerateHtmlReport(List<CNPJAnalysisResult> results, AnalysisStats stats);
    }

    public class ReportGeneratorService : IReportGeneratorService
    {
        public string GenerateHtmlReport(List<CNPJAnalysisResult> results, AnalysisStats stats)
        {
            var sb = new StringBuilder();
            
            AppendHtmlHeader(sb, stats);
            AppendStatistics(sb, stats);
            AppendResultsByFile(sb, results);
            AppendHtmlFooter(sb);
            
            return sb.ToString();
        }

        private void AppendHtmlHeader(StringBuilder sb, AnalysisStats stats)
        {
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
        }

        private void AppendStatistics(StringBuilder sb, AnalysisStats stats)
        {
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
        }

        private void AppendResultsByFile(StringBuilder sb, List<CNPJAnalysisResult> results)
        {
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
                    AppendIssueItem(sb, result);
                }
                
                sb.AppendLine("    </div>");
            }
        }

        private void AppendIssueItem(StringBuilder sb, CNPJAnalysisResult result)
        {
            var severityClass = $"severity-{result.Severity.ToLower()}";
            var statusIcon = result.NeedsCorrection ? "⚠️ REQUER CORREÇÃO" : "✅ OK";
            
            sb.AppendLine($@"
        <div class='issue-item'>
            <div><span class='line-number'>Linha {result.LineNumber}</span> - CNPJ: <span class='cnpj'>{result.DetectedCNPJ}</span></div>
            <div><strong>Status:</strong> <span class='{severityClass}'>{statusIcon}</span></div>
            <div><strong>Tipo:</strong> {result.IssueType}</div>
            <div><strong>Severidade:</strong> <span class='{severityClass}'>{result.Severity}</span></div>
            <div><strong>Problema:</strong> {result.IssueDescription}</div>
            <div><strong>Ação Recomendada:</strong> {result.RecommendedAction}</div>
            <div><strong>Código:</strong> <code class='code'>{result.LineContent}</code></div>
        </div>");
        }

        private void AppendHtmlFooter(StringBuilder sb)
        {
            sb.AppendLine("</body></html>");
        }
    }
}