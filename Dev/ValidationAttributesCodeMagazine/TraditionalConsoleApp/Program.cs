#nullable disable

using TraditionalConsoleApp.ViewModelClasses;

namespace TraditionalConsoleApp
{
	public partial class Program
	{
		static void Main(string[] args)
		{
			//create viewmodel and init Entity object
			ProductViewModel vm = new()
			{
				//Entity = new() { Name = "", ListPrice = 5, StandardCost = 15 }
				Entity = new() { ProductID = 1, Name = "", ProductNumber = ""}
			};

			//validate the data
			var msgs = vm.Validate();

			//display the vailed validate messages
			foreach(ValidationMessage item in msgs)
			{
				Console.WriteLine(item);
			}

			// Display Total Count
			Console.WriteLine();
			Console.WriteLine($"Total Validations Failed: {msgs.Count}");

			// Pause for Results
			Console.ReadKey();
		}
	}
}