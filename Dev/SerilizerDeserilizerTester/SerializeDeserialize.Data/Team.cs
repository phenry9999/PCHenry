namespace SerializeDeserialize.Data;

public class Team : PrimaryTombstoneData
{
	public Team(string leagueName, string description, string logoUri) : base(leagueName, description, logoUri)
	{
	}
}
