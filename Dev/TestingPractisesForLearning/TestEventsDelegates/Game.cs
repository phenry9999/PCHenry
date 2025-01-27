namespace TestEventsDelegates;

public class Game : IDisposable
{
	public const int MaxPoints = 25;
	public const int MaxScore = 100;
	Player player = new();

	public event Action? GameOver;

	public async void Play()
	{
		Player player = new();
		player.AchievementUnlocked += OnAchievementUnlocked;

		Party party = new();
		player.AchievementUnlocked += party.Cheering;

		Random random = new Random();
		int turns = 0;

		while (player.TotalPoints < MaxScore)
		{
			turns += 1;
			await player.AddPoints(random.Next(MaxPoints));
			Console.WriteLine($"Turn {turns} has {player.TotalPoints}");
		}

		if (GameOver != null)
		{
			GameOver();
		}
	}
	public static void OnAchievementUnlocked(int totalPoints)
	{
		Console.WriteLine($"Congratulations! Achievement Unlocked for earning points {totalPoints}.");
	}

	public void Dispose()
	{
		Console.WriteLine("Dispose");

		Console.WriteLine("Detaching AchievementUnlocked");
		player.AchievementUnlocked -= OnAchievementUnlocked;
		player.AchievementUnlocked -= new Party().Cheering;

		Console.WriteLine("Game Over");
	}
}

public class Party
{
	public void Cheering(int points)
	{
		Console.WriteLine($"WOOHOO! You got freaking {points} points! AWESOME!");
	}
}