using CNPJAnalyzerWebAPI.Entities;

namespace CNPJAnalyzerWebAPI.Services
{
    public interface IStatsGeneratorService
    {
        AnalysisStats GenerateStats(int totalFiles, List<CNPJAnalysisResult> results);
    }

    public class StatsGeneratorService : IStatsGeneratorService
    {
        public AnalysisStats GenerateStats(int totalFiles, List<CNPJAnalysisResult> results)
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
    }
}