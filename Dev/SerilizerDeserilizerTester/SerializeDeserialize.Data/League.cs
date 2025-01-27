namespace SerializeDeserialize.Data;

public class League : PrimaryTombstoneData
{
	public League(string leagueName, string description, string logoUri) : base(leagueName, description, logoUri)
	{
	}

	public List<Team> Teams { get; set; }
}