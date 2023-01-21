#nullable disable

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TraditionalConsoleApp
{
	public class Product
	{
		public int ProductID { get; set; }

		[Display(Name = "Product Name")]
		[Required(ErrorMessage = "{0} must be filled in dammit")]
		public string Name { get; set; }

		[Required(ErrorMessage = "{0} Must be filled in")]
		[Display(Name = "Product Number")]
		public string ProductNumber { get; set; }

		[Display(Name = "Product Color")]
		public string Color { get; set; }

		[Required]
		[Display(Name = "Cost")]
		[Range(0.01, 9999, ErrorMessage = "{0} must be between {1} and {2}")]
		public decimal? StandardCost { get; set; }

		[Required]
		[Display(Name = "Price")]
		public decimal? ListPrice { get; set; }

		[Required]
		[Display(Name = "Start Selling Date")]
		public DateTime SellStartDate { get; set; }

		[Display(Name = "End Selling Date")]
		public DateTime? SellEndDate { get; set; }

		[Display(Name = "Date Discontinued")]
		public DateTime? DiscontinuedDate { get; set; }

		public override string ToString()
		{
			return $"{Name} ({ProductID})";
		}
	}
}