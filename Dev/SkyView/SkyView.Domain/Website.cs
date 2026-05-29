using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyView.Domain;
public class Website
{
	[Required]
	public int Id { get; set; }
	[Required]
	public string Name { get; set; }
	public string? Description { get; set; }
	[Required]
	public string Url { get; set; }

	[Required]
	public DateTime CreatedAt { get; set; }

	public Website()
	{
		CreatedAt = DateTime.Now;
	}

	public Website(string name, string url, string? description = null) : this()
	{
		Name = name;
		Url = url;
		Description = description;
	}
}
