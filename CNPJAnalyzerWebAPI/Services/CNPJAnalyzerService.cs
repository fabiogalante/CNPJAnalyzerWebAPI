using System.IO.Compression;
using System.Text;
using CNPJAnalyzerWebAPI.Analyzers;
using CNPJAnalyzerWebAPI.Entities;

namespace CNPJAnalyzerWebAPI.Services
{
    public class CnpjAnalyzerService
    {
        private readonly LineAnalyzer _lineAnalyzer = new();
        private readonly IFileFilterService _fileFilterService = new FileFilterService();
        private readonly IReportGeneratorService _reportGeneratorService = new ReportGeneratorService();
        private readonly IStatsGeneratorService _statsGeneratorService = new StatsGeneratorService();

        public async Task<AnalysisResponse> AnalyzeZipFileAsync(Stream zipStream)
        {
            var results = new List<CNPJAnalysisResult>();
            int totalFiles = 0;

            try
            {
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                
                foreach (var entry in archive.Entries)
                {
                    if (_fileFilterService.ShouldAnalyzeFile(entry.FullName))
                    {
                        totalFiles++;
                        var entryResults = await AnalyzeFileEntryAsync(entry);
                        results.AddRange(entryResults);
                    }
                }

                var stats = _statsGeneratorService.GenerateStats(totalFiles, results);
                var reportHtml = _reportGeneratorService.GenerateHtmlReport(results, stats);

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
                    Results = []
                };
            }
        }

        private async Task<List<CNPJAnalysisResult>> AnalyzeFileEntryAsync(ZipArchiveEntry entry)
        {
            var results = new List<CNPJAnalysisResult>();
            
            try
            {
                await using var stream = entry.Open();
                using var reader = new StreamReader(stream, Encoding.UTF8);
                
                string content = await reader.ReadToEndAsync();
                string[] lines = content.Split('\n');
                
                for (int i = 0; i < lines.Length; i++)
                {
                    var lineResults = _lineAnalyzer.AnalyzeLine(entry.FullName, i + 1, lines[i]);
                    results.AddRange(lineResults);
                }
            }
            catch (Exception ex)
            {
                results.Add(CreateErrorResult(entry.FullName, ex));
            }

            return RemoveDuplicates(results);
        }

        private CNPJAnalysisResult CreateErrorResult(string filePath, Exception ex)
        {
            return new CNPJAnalysisResult
            {
                FilePath = filePath,
                LineNumber = 0,
                LineContent = "Erro ao ler arquivo",
                DetectedCNPJ = "",
                NeedsCorrection = true,
                RecommendedAction = "Verificar manualmente",
                IssueDescription = $"Erro ao ler arquivo: {ex.Message}",
                IssueType = "ReadError",
                Severity = "High"
            };
        }

        private List<CNPJAnalysisResult> RemoveDuplicates(List<CNPJAnalysisResult> results)
        {
            return results
                .GroupBy(r => new { r.FilePath, r.LineNumber, r.IssueType })
                .Select(g => g.First())
                .ToList();
        }
    }
}