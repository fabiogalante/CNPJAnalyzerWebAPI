using CNPJAnalyzerWebAPI.Configuration;

namespace CNPJAnalyzerWebAPI.Services;

public interface IFileFilterService
{
    bool ShouldAnalyzeFile(string fileName);
}

public class FileFilterService : IFileFilterService
{
    public bool ShouldAnalyzeFile(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
            
        if (!AnalysisConfiguration.SupportedExtensions.Contains(extension))
            return false;
            
        return !AnalysisConfiguration.IgnorePaths.Any(ignorePath => 
            fileName.Contains(ignorePath, StringComparison.OrdinalIgnoreCase));
    }
}