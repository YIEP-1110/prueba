using Microsoft.AspNetCore.Components.Forms;

namespace prueba.Services;

public interface IPdfStorageService
{
    Task<string> SaveAsync(IBrowserFile file);
    void Delete(string? url);
}

public class LocalPdfStorageService : IPdfStorageService
{
    private const long MaxBytes = 10 * 1024 * 1024; 
    private const string Folder = "uploads/ponencias";
    private readonly IWebHostEnvironment _env;

    public LocalPdfStorageService(IWebHostEnvironment env) => _env = env;

    public async Task<string> SaveAsync(IBrowserFile file)
    {
        if (!Path.GetExtension(file.Name).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            || file.ContentType != "application/pdf")
            throw new InvalidOperationException("El archivo debe ser un PDF.");

        var dir = Path.Combine(_env.WebRootPath, Folder);
        Directory.CreateDirectory(dir);

        var nombre = $"{Guid.NewGuid():N}.pdf";
        var ruta = Path.Combine(dir, nombre);

        try
        {
            await using (var src = file.OpenReadStream(MaxBytes))
            await using (var dst = File.Create(ruta))
                await src.CopyToAsync(dst);

            var header = new byte[4];
            await using (var fs = File.OpenRead(ruta))
                await fs.ReadExactlyAsync(header, 0, 4);
            if (System.Text.Encoding.ASCII.GetString(header) != "%PDF")
                throw new InvalidOperationException("El archivo no es un PDF válido.");
        }
        catch
        {
            if (File.Exists(ruta)) File.Delete(ruta);
            throw;
        }

        return $"/{Folder}/{nombre}";
    }

    public void Delete(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith($"/{Folder}/")) return;
        var ruta = Path.Combine(_env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(ruta)) File.Delete(ruta);
    }
}