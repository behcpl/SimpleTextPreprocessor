namespace SimpleTextPreprocessor;

public interface IReport
{
    void Error(string fileId, int line, int column, string message);
}