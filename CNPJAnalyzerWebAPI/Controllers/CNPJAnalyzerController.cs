using CNPJAnalyzerWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CNPJAnalyzerWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CnpjAnalyzerController(CnpjAnalyzerService analyzerService) : ControllerBase
{
    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeZipFile(IFormFile zipFile)
    {
        if (zipFile == null || zipFile.Length == 0)
        {
            return BadRequest(new { success = false, message = "Arquivo ZIP não fornecido" });
        }

        if (!zipFile.FileName.ToLower().EndsWith(".zip"))
        {
            return BadRequest(new { success = false, message = "Apenas arquivos ZIP são suportados" });
        }

        if (zipFile.Length > 50 * 1024 * 1024) // 50MB limit
        {
            return BadRequest(new { success = false, message = "Arquivo muito grande. Limite: 50MB" });
        }

        try
        {
            await using var stream = zipFile.OpenReadStream();
            var result = await analyzerService.AnalyzeZipFileAsync(stream);
                
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Erro interno: {ex.Message}" });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.Now });
    }
}