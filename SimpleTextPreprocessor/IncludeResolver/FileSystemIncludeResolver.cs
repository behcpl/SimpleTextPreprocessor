﻿using System;
using System.IO;

namespace SimpleTextPreprocessor.IncludeResolver;

public class FileSystemIncludeResolver : IIncludeResolver
{  
    /// <summary>
    /// Translate any path (local or absolute) into <c>fileId</c> compatible with this resolver.
    /// This value can be passed as <c>string fileId</c> when calling <c>Preprocessor.Process()</c>
    /// </summary>
    /// <param name="path">Local or absolute path of file that is processed</param>
    /// <returns>fileId</returns>
    public string GetFileId(string path)
    {
        return Path.GetFullPath(path);
    }
    
    public bool TryCreateReader(string sourceFileId, string includeParameter, out string? newFileId, out TextReader? reader, IReport? report)
    {
        string strippedName = includeParameter.TrimStart('"').TrimEnd('"');
        
        // TODO: check for matching "" or <> (abandon <> variant?)
        // TODO: check for invalid chars in `strippedName`
        // TODO: option to global file resolution
        
        // TODO: handle potential path exceptions?
        string srcDir = Path.GetDirectoryName(sourceFileId)!;
        string newPath = Path.GetFullPath(Path.Combine(srcDir, strippedName));

        if (!File.Exists(newPath))
        {
            report?.Error(sourceFileId, report.CurrentLine, report.CurrentColumn, $"File '{newPath}' does not exist!");
            newFileId = null;
            reader = null;
            return false;
        }
        
        try
        {
            newFileId = newPath;
            reader = new StreamReader(newPath);
        }
        catch (Exception e)
        {
            report?.Exception(sourceFileId, report.CurrentLine, report.CurrentColumn, e);
            newFileId = null;
            reader = null;
            return false;
        }
        return true;
    }
}