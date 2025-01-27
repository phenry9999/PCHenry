namespace HockeyTreeView.Models;

public class Conference
{
	public string Name { get; set; }
	public string Nickname { get; set; }
	public string LogoName { get; set; }
	public IList<Division> Divisions { get; set; }

	public Conference()
	{
		Divisions = new List<Division>();
	}

	public Conference(string name, string nickname, string logoName) : this()
	{
		Name = name;
		Nickname = nickname;
		LogoName = logoName;
	}

	public string LogoUri => $"/Assets/{LogoName}";
}
