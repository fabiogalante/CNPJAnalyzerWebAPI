using Microsoft.AspNetCore.Mvc;

namespace CNPJAnalyzerWebAPI.Controllers;

[ApiController]
[Route("/")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>CNPJ Analyzer API</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        .container { max-width: 800px; margin: 0 auto; }
        .upload-area { 
            border: 2px dashed #ccc; 
            padding: 40px; 
            text-align: center; 
            margin: 20px 0;
            border-radius: 8px;
        }
        .upload-area:hover { border-color: #007bff; }
        .btn { 
            background: #007bff; 
            color: white; 
            padding: 10px 20px; 
            border: none; 
            border-radius: 4px; 
            cursor: pointer; 
        }
        .btn:hover { background: #0056b3; }
        .result { margin-top: 20px; padding: 20px; background: #f8f9fa; border-radius: 4px; }
        .loading { display: none; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🔍 CNPJ Analyzer API</h1>
        <p>Faça upload de um arquivo ZIP para analisar compatibilidade de CNPJs com o novo formato alfanumérico.</p>
        
        <form id='uploadForm' enctype='multipart/form-data'>
            <div class='upload-area'>
                <input type='file' id='zipFile' name='zipFile' accept='.zip' required>
                <p>Clique para selecionar um arquivo ZIP ou arraste aqui</p>
            </div>
            <button type='submit' class='btn'>Analisar ZIP</button>
        </form>

        <div id='loading' class='loading'>
            <p>⏳ Analisando arquivo... Por favor, aguarde.</p>
        </div>

        <div id='result' class='result' style='display: none;'></div>
    </div>

    <script>
        document.getElementById('uploadForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const formData = new FormData();
            const fileInput = document.getElementById('zipFile');
            
            if (!fileInput.files[0]) {
                alert('Por favor, selecione um arquivo ZIP');
                return;
            }
            
            formData.append('zipFile', fileInput.files[0]);
            
            document.getElementById('loading').style.display = 'block';
            document.getElementById('result').style.display = 'none';
            
            try {
                const response = await fetch('/api/cnpjanalyzer/analyze', {
                    method: 'POST',
                    body: formData
                });
                
                const result = await response.json();
                
                if (result.success) {
                    document.getElementById('result').innerHTML = result.reportHtml;
                } else {
                    document.getElementById('result').innerHTML = `<p style='color: red;'>Erro: ${result.message}</p>`;
                }
                
                document.getElementById('result').style.display = 'block';
            } catch (error) {
                document.getElementById('result').innerHTML = `<p style='color: red;'>Erro: ${error.message}</p>`;
                document.getElementById('result').style.display = 'block';
            }
            
            document.getElementById('loading').style.display = 'none';
        });
    </script>
</body>
</html>";

        return Content(html, "text/html; charset=utf-8");
    }
}