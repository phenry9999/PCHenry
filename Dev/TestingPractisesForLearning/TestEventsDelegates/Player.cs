namespace TestEventsDelegates;

public class Player
{
	public int TotalPoints { get; private set; }

	public delegate void AchievementUnlockedHandler(int points);
	public event AchievementUnlockedHandler? AchievementUnlocked;


	public async Task AddPoints(int points)
	{
		TotalPoints += points;
		Console.WriteLine($"Player earned {points}. Total Points: {TotalPoints}");
		await Task.Delay(1000);

		if (TotalPoints >= Game.MaxScore)
		{
			AchievementUnlocked(TotalPoints);
		}
	}
}