namespace CNPJAnalyzerWebAPI.Entities;

public class CNPJAnalysisResult
{
    public string FilePath { get; set; }
    public int LineNumber { get; set; }
    public string LineContent { get; set; }
    public string DetectedCNPJ { get; set; }
    public bool NeedsCorrection { get; set; }
    public string RecommendedAction { get; set; }
    public string IssueDescription { get; set; }
    public string IssueType { get; set; }
    public string Severity { get; set; }
}

public class AnalysisResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public AnalysisStats Stats { get; set; }
    public List<CNPJAnalysisResult> Results { get; set; }
    public string ReportHtml { get; set; }
}

public class AnalysisStats
{
    public int TotalFiles { get; set; }
    public int FilesWithCNPJ { get; set; }
    public int TotalCNPJs { get; set; }
    public int CNPJsNeedingCorrection { get; set; }
    public Dictionary<string, int> IssuesByType { get; set; }
    public Dictionary<string, int> IssuesBySeverity { get; set; }
}