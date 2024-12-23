namespace SimpleTextPreprocessor;

public interface IReport
{
    void Error(int fileId, string filePath, int line, int column, string message);
}