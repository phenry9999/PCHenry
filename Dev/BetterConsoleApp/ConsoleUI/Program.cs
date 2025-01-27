using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ConsoleUI;

//DI, Serilog, Settings

internal class Program
{
	static void Main(string[] args)
	{
		var builder = new ConfigurationBuilder();
		BuildConfig(builder);

		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(builder.Build())
			.Enrich.FromLogContext()
			.WriteTo.Console()
			//.WriteTo.Debug()
			.CreateLogger();

		Log.Logger.Information("Application Starting");

		var host = Host.CreateDefaultBuilder()
			.ConfigureServices((context, services) =>
			{

			})
			.UseSerilog()
			.Build();
	}

	static void BuildConfig(IConfigurationBuilder builder)
	{
		builder.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("AVANTI_ENVIRONMENT") ?? "Production"}.json", optional: true)
			.AddEnvironmentVariables();
	}
}
