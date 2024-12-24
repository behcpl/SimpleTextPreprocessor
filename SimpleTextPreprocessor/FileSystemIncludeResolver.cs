using System.IO;

namespace SimpleTextPreprocessor;

public class FileSystemIncludeResolver : IIncludeResolver
{  
    /// <summary>
    /// Translate any path (local or absolute) into "fileId" compatible with this resolver.
    /// This value can be passed as "string fileId" when calling Preprocessor.Process()
    /// </summary>
    /// <param name="path">Local or absolute path of file that is processed</param>
    /// <returns>fileId</returns>
    public string GetFileId(string path)
    {
        return Path.GetFullPath(path);
    }
    
    public bool TryCreateReader(string sourceFileId, string includeParameter, out string? newFileId, out TextReader? reader, IReport? report)
    {
        string strippedName = includeParameter.TrimStart().TrimStart('"').TrimEnd().TrimEnd('"');
        
        // TODO: check for matching "" or <> (abandon <> variant?)
        // TODO: check for invalid chars in `strippedName`
        // TODO: option to global file resolution
        
        // TODO: handle potential path exceptions?
        string srcDir = Path.GetDirectoryName(sourceFileId)!;
        string newPath = Path.GetFullPath(Path.Combine(srcDir, strippedName));

        if (!File.Exists(newPath))
        {
            report?.Error(sourceFileId, 0, 0, $"File '{newPath}' does not exist!");
            newFileId = null;
            reader = null;
            return false;
        }
        
        // TODO: handle IO exception?
        newFileId = newPath;
        reader = new StreamReader(newPath);
        return true;
    }
}