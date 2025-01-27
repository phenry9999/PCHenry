namespace FileProcesses;

public class FileProcess
{
	public bool FileExists(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentNullException(nameof(fileName));
		}

		return File.Exists(fileName);
	}
}
