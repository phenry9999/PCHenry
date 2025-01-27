namespace HockeyTreeView.Models;

public class Division
{
	public string Name { get; set; }
	public string Nickname { get; set; }
	public string LogoName { get; set; }
	public IList<Team> Teams { get; set; }

	public Division()
	{
		Teams = new List<Team>();
	}

	public Division(string name, string nickname, string logoName) : this()
	{
		Name = name;
		Nickname = nickname;
		LogoName = logoName;
	}

	public string LogoUri => $"/Assets/{LogoName}";
}
