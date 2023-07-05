using System;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal class Program
{
	private static void Main(string[] args)
	{
		System.Threading.Timer timer = new System.Threading.Timer(ServiceTimer_Elapsed);

		//2023-05-30 13:50:00.28 Log INFO  - Setting timer interval to 79199714.1851 milliseconds, which means 05-31-2023 11:49
		//2023-05-31 16:03:50.07 Log INFO  - Setting timer interval to 71169941.202 milliseconds, which means 06-01-2023 11:50 (Current DateTime = '05-31-2023 16:03')

		var yesterday = new DateTime(2023, 05, 31, 16, 03, 50, 07);
		//var interval = 79199714.1851;
		var interval = 71169941.202;

		var nextTime = yesterday.AddMilliseconds(interval);

		Console.WriteLine($"Next Time = {nextTime.ToString()}");
		Console.ReadLine();

	}

	private static void ServiceTimer_Elapsed(object? state)
	{
		Console.WriteLine("Timer Elapsed");
	}
}