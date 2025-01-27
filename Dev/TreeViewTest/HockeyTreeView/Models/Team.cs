using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyTreeView.Models;

public class Team
{
	public string Name { get; set; }
	public string Nickname { get; set; }
	public string WebsiteUrl { get; set; }
	public string LogoName { get; set; }
	public string ArenaName { get; set; }
	public string CityName { get; set; }
	public string Description { get; set; }

	public Team()
	{
	}

	public Team(string name, string nickname, string websiteUrl, string logoName, string arenaName, string cityName) : this()
	{
		Name = name;
		Nickname = nickname;
		WebsiteUrl = websiteUrl;
		LogoName = logoName;
		ArenaName = arenaName;
		CityName = cityName;
	}

	public string LogoUri => $"/Assets/{LogoName}";
}