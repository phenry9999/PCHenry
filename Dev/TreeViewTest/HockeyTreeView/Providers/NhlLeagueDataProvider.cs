using HockeyTreeView.Models;

namespace HockeyTreeView.Providers;

public class NhlLeagueDataProvider : ILeagueDataProvider
{
	public async Task<League> GetAllAsync()
	{
		var nhl = new League("Natiional Hockey League", "NHL", "https://www.nhl.com", "NhlLeague.gif");

		var easternConference = new Conference("Eastern Conference", "EAST", "EasternConference.gif");
		nhl.Conferences.Add(easternConference);

		var atlanticDivision = new Division("Atlantic Division", "ATL", "AtlanticDivision.png");
		easternConference.Divisions.Add(atlanticDivision);

		atlanticDivision.Teams.Add(new Team("Boston Bruins", "Bruins", "https://www.nhl.com/bruins", "BostonBruins.gif", "TD Garden", "Boston"));
		atlanticDivision.Teams.Add(new Team("Buffalo Sabres", "Sabres", "https://www.nhl.com/sabres", "BuffaloSabres.gif", "KeyBank Center", "Buffalo"));
		atlanticDivision.Teams.Add(new Team("Detroit Red Wings", "Red Wings", "https://www.nhl.com/redwings", "DetroitRedWings.gif", "Little Caesars Arena", "Detroit"));
		atlanticDivision.Teams.Add(new Team("Florida Panthers", "Panthers", "https://www.nhl.com/panthers", "FloridaPanthers.gif", "BB&T Center", "Sunrise"));
		atlanticDivision.Teams.Add(new Team("Montreal Canadiens", "Canadiens", "https://www.nhl.com/canadiens", "MontrealCanadiens.gif", "Bell Centre", "Montreal"));
		atlanticDivision.Teams.Add(new Team("Ottawa Senators", "Senators", "https://www.nhl.com/senators", "OttawaSenators.gif", "Canadian Tire Centre", "Ottawa"));
		atlanticDivision.Teams.Add(new Team("Tampa Bay Lightning", "Lightning", "https://www.nhl.com/lightning", "TampaBayLightning.gif", "Amalie Arena", "Tampa"));
		atlanticDivision.Teams.Add(new Team("Toronto Maple Leafs", "Maple Leafs", "https://www.nhl.com/mapleleafs", "TorontoMapleLeafs.gif", "Scotiabank Arena", "Toronto"));

		var metropolitanDivision = new Division("Metropolitan Division", "MET", "MetropolitanDivision.png");
		easternConference.Divisions.Add(metropolitanDivision);
		metropolitanDivision.Teams.Add(new Team("Carolina Hurricanes", "Hurricanes", "https://www.nhl.com/hurricanes", "CarolinaHurricanes.gif", "PNC Arena", "Raleigh"));
		metropolitanDivision.Teams.Add(new Team("Columbus Blue Jackets", "Blue Jackets", "https://www.nhl.com/bluejackets", "ColumbusBlueJackets.gif", "Nationwide Arena", "Columbus"));
		metropolitanDivision.Teams.Add(new Team("New Jersey Devils", "Devils", "https://www.nhl.com/devils", "NewJerseyDevils.gif", "Prudential Center", "Newark"));
		metropolitanDivision.Teams.Add(new Team("New York Islanders", "Islanders", "https://www.nhl.com/islanders", "NewYorkIslanders.gif", "Barclays Center", "Brooklyn"));
		metropolitanDivision.Teams.Add(new Team("New York Rangers", "Rangers", "https://www.nhl.com/rangers", "NewYorkRangers.gif", "Madison Square Garden", "New York"));
		metropolitanDivision.Teams.Add(new Team("Philadelphia Flyers", "Flyers", "https://www.nhl.com/flyers", "PhiladelphiaFlyers.gif", "Wells Fargo Center", "Philadelphia"));
		metropolitanDivision.Teams.Add(new Team("Pittsburgh Penguins", "Penguins", "https://www.nhl.com/penguins", "PittsburghPenguins.gif", "PPG Paints Arena", "Pittsburgh"));
		metropolitanDivision.Teams.Add(new Team("Washington Capitals", "Capitals", "https://www.nhl.com/capitals", "WashingtonCapitals.gif", "Capital One Arena", "Washington"));

		var westernConference = new Conference("Western Conference", "WEST", "WesternConference.gif");
		nhl.Conferences.Add(westernConference);

		var centralDivision = new Division("Central Division", "CEN", "CentralDivision.png");
		westernConference.Divisions.Add(centralDivision);

		centralDivision.Teams.Add(new Team("Chicago Blackhawks", "Blackhawks", "https://www.nhl.com/blackhawks", "ChicagoBlackhawks.gif", "United Center", "Chicago"));
		centralDivision.Teams.Add(new Team("Colorado Avalanche", "Avalanche", "https://www.nhl.com/avalanche", "ColoradoAvalanche.gif", "Pepsi Center", "Denver"));
		centralDivision.Teams.Add(new Team("Dallas Stars", "Stars", "https://www.nhl.com/stars", "DallasStars.gif", "American Airlines Center", "Dallas"));
		centralDivision.Teams.Add(new Team("Minnesota Wild", "Wild", "https://www.nhl.com/wild", "MinnesotaWild.gif", "Xcel Energy Center", "St. Paul"));
		centralDivision.Teams.Add(new Team("Nashville Predators", "Predators", "https://www.nhl.com/predators", "NashvillePredators.gif", "Bridgestone Arena", "Nashville"));
		centralDivision.Teams.Add(new Team("St. Louis Blues", "Blues", "https://www.nhl.com/blues", "StLouisBlues.gif", "Enterprise Center", "St. Louis"));
		centralDivision.Teams.Add(new Team("Winnipeg Jets", "Jets", "https://www.nhl.com/jets", "WinnipegJets.gif", "Bell MTS Place", "Winnipeg"));
		centralDivision.Teams.Add(new Team("Utah Hockey Club", "Hockey Club", "https://www.nhl.com/utah/", "UtahHockeyClub.gif", "Delta Center", "Salt Lake City"));

		var pacificDivision = new Division("Pacific Division", "PAC", "PacificDivision.png");
		westernConference.Divisions.Add(pacificDivision);
		pacificDivision.Teams.Add(new Team("Anaheim Ducks", "Ducks", "https://www.nhl.com/ducks", "AnaheimDucks.gif", "Honda Center", "Anaheim"));
		pacificDivision.Teams.Add(new Team("Calgary Flames", "Flames", "https://www.nhl.com/flames", "CalgaryFlames.gif", "Scotiabank Saddledome", "Calgary"));
		pacificDivision.Teams.Add(new Team("Edmonton Oilers", "Oilers", "https://www.nhl.com/oilers", "EdmontonOilers.gif", "Rogers Place", "Edmonton"));
		pacificDivision.Teams.Add(new Team("Los Angeles Kings", "Kings", "https://www.nhl.com/kings", "LosAngelesKings.gif", "Staples Center", "Los Angeles"));
		pacificDivision.Teams.Add(new Team("San Jose Sharks", "Sharks", "https://www.nhl.com/sharks", "SanJoseSharks.gif", "SAP Center", "San Jose"));
		pacificDivision.Teams.Add(new Team("Seattle Kraken", "Kraken", "https://www.nhl.com/kraken", "SeattleKraken.gif", "Climate Pledge Arena", "Seattle"));
		pacificDivision.Teams.Add(new Team("Vancouver Canucks", "Canucks", "https://www.nhl.com/canucks", "VancouverCanucks.gif", "Rogers Arena", "Vancouver"));
		pacificDivision.Teams.Add(new Team("Vegas Golden Knights", "Golden Knights", "https://www.nhl.com/goldenknights/", "VegasGoldenKnights.gif", "T-Mobile Arena", "Vegas"));

		return nhl;
	}
}
