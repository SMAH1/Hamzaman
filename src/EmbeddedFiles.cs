using System.Collections.ObjectModel;
using System.Reflection;

namespace Hamzaman;

public class EmbeddedFiles
{
    const string perfix = "Hamzaman.Embedded.";
    ReadOnlyDictionary<string, string> Files;

    public EmbeddedFiles()
    {
        var files = new Dictionary<string, string>();

        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();

        foreach (var resourceName in resourceNames)
        {
            if (resourceName.StartsWith(perfix))
                files.Add(resourceName.Substring(perfix.Length).ToLower(), resourceName);
            else
                Console.WriteLine("Ignored embedded file: " + resourceName);
        }

        Files = files.AsReadOnly();
    }

    public bool Exists(string filename)
    {
        return Files.ContainsKey(filename);
    }

    public byte[]? ReadAllBytes(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();

        using (Stream? stream = assembly.GetManifestResourceStream(Files[filename]))
        {
            if (stream is not null)
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        return [];
    }
}
