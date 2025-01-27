namespace SerializeDeserialize.Data;

public class ReportParameters
{
	public string connectionString { get; set; }

	public Dictionary<string, string> Parameters { get; set; }
}