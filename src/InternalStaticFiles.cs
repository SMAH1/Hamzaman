using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Hamzaman;

public static class InternalStaticFiles
{
    private static AppSettings _settings = null!;
    private static EmbeddedFiles _embeddedFiles = null!;

    private static byte[]? ReadFileContent(string filename, out bool error)
    {
        error = false;
        var root = _settings.Root;
        var fullPathName = Path.Combine(root, filename);

        filename = filename.ToLower();

        if (string.Compare(filename, "appsettings.json", true) != 0)
        {
            try
            {
                if (File.Exists(fullPathName))
                    return File.ReadAllBytes(fullPathName);
                else if (_embeddedFiles.Exists(filename))
                    return _embeddedFiles.ReadAllBytes(filename);
            }
            catch { error = true; }
        }
        else
        {
            error = true;
        }
        return null;
    }

    private static string GetMimeTypeForFileExtension(string filePath)
    {
        const string DefaultContentType = "application/octet-stream";

        var provider = new FileExtensionContentTypeProvider();

        if (!provider.TryGetContentType(filePath, out string? contentType))
        {
            contentType = DefaultContentType;
        }

        return contentType!;
    }

    public static void StaticFilesApi(this IEndpointRouteBuilder app, AppSettings settings, EmbeddedFiles embeddedFiles)
    {
        _settings = settings;
        _embeddedFiles = embeddedFiles;
        app.MapGet("/{*filename}",
            (
                [FromRoute] string filename
            ) =>
            {
                var mime = GetMimeTypeForFileExtension(filename);
                if (string.IsNullOrEmpty(mime)) return Results.Content(content: "MIME type not support!", contentType: "text/plain", statusCode: 404);

                var bytes = ReadFileContent(filename, out var err);
                if (err) return Results.Content(content: "Error occured!", contentType: "text/plain", statusCode: 404);
                if (bytes == null) return Results.Content(content: "File not found!", contentType: "text/plain", statusCode: 404);
                return Results.Bytes(bytes, mime);
            })
            ;
    }
}
