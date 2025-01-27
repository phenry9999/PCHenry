namespace HockeyTreeView.Models;

public class League
{
	public string Name { get; set; }
	public string Nickname { get; set; }
	public string WebsiteName { get; set; }
	public string LogoName { get; set; }

	public IList<Conference> Conferences { get; set; }

	public League()
	{
		Conferences = new List<Conference>();
	}

	public League(string name, string nickname, string websiteUrl, string logoName) : this()
	{
		Name = name;
		Nickname = nickname;
		WebsiteName = websiteUrl;
		LogoName = logoName;
	}

	public string LogoUri => $"/Assets/{LogoName}";
}