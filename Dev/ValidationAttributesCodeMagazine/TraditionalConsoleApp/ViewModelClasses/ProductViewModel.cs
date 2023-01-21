using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

using static TraditionalConsoleApp.Program;

namespace TraditionalConsoleApp.ViewModelClasses
{
	public class ProductViewModel
	{
		public ProductViewModel()
		{
			Entity = new();
		}

		public Product Entity { get; set; }

		public List<ValidationMessage> Validate()
		{
			List<ValidationMessage> msgs = new();

			// Create instance of ValidationContext object
			ValidationContext context = new(Entity, serviceProvider: null, items: null);
			List<ValidationResult> results = new();

			// Call TryValidateObject() method
			if(!Validator.TryValidateObject(Entity, context, results, true))
			{
				// Get validation results
				foreach(ValidationResult item in results)
				{
					string propName = string.Empty;
					if(item.MemberNames.Any())
					{
						propName = ((string[])item.MemberNames)[0];
					}

					// Build new ValidationMessage object
					ValidationMessage msg = new()
					{
						ErrorMessage = item.ErrorMessage,
						PropertyName = propName
					};

					// Add validation object to list
					msgs.Add(msg);
				}
			}

			return msgs;
		}
	}
}
