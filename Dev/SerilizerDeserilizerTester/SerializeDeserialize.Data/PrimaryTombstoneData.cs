namespace SerializeDeserialize.Data;

public class PrimaryTombstoneData
{
	public string LeagueName { get; set; }
	public string Description { get; set; }
	public Uri Logo { get; set; }

	public PrimaryTombstoneData(string leagueName, string description, string logoUri)
	{
		if (string.IsNullOrWhiteSpace(leagueName))
		{
			throw new ArgumentException("League name cannot be null or whitespace.", nameof(leagueName));
		}

		this.LeagueName = leagueName;

		if (!string.IsNullOrEmpty(description))
		{
			this.Description = description;
		}

		if (!string.IsNullOrWhiteSpace(logoUri))
		{
			this.Logo = new Uri(logoUri);
		}
	}
}