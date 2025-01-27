using System.Runtime.InteropServices;

namespace TestEventsDelegates;

public class Program
{
	public static void Main(string[] args)
	{
		Program program = new();

		Game game = new();
		game.Play();
		game.GameOver += program.GameOverMessage;

		Console.ReadLine();
	}

	public void GameOverMessage()
	{
		Console.WriteLine(new string('*', 40));
		Console.WriteLine("Game Over");
		Console.WriteLine(new string('*', 40));
	}
}
