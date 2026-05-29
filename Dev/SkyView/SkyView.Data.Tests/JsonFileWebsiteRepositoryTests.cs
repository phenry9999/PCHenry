using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SkyView.Domain;

namespace SkyView.Data.Tests;
public class JsonFileWebsiteRepositoryTests : IDisposable
{
	private readonly string filePath;
	private readonly JsonFileWebsiteRepository repository;

	public JsonFileWebsiteRepositoryTests()
	{
		var now = DateTime.Now.ToString("yyyyMMddhhmmss");
		filePath = Path.Combine(Path.GetTempPath(), $"SkyView.Test.{now}.{Guid.NewGuid()}.json");
		repository = new JsonFileWebsiteRepository(filePath);
	}

	public void Dispose()
	{
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}

	[Fact]
	public void GetAllAsynWithNoFileReturnsEmptyList()
	{
		var result = repository.GetAllAsync().Result;

		Assert.Empty(result);
	}

	[Fact]
	public void AddAsycWithFristWebsiteAssignsIdOne()
	{
		var website = new Website { Name = "GitHub", Url = "https://github.com", CreatedAt = DateTime.Now };

		var id = repository.AddAsync(website).Result;

		Assert.Equal(1, id);
		Assert.Equal(1, website.Id);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(3)]
	[InlineData(10)]
	public void AddAsyncAssignsSequentialIds(int numberOfWebsites)
	{
		var websites = new List<Website>();

		for (int i = 0; i < numberOfWebsites; i++)
		{
			websites.Add(new Website { Name = $"Website {i + 1}", Url = $"https://website{i + 1}.com", CreatedAt = DateTime.Now });
		}

		foreach (var website in websites)
		{
			var id = repository.AddAsync(website).Result;

			Assert.Equal(websites.IndexOf(website) + 1, id);
			Assert.Equal(websites.IndexOf(website) + 1, website.Id);
		}
	}

	[Fact]
	public async Task GetByIdAsyncWithMissingIdReturnsNull()
	{
		var website = new Website { Name = "GitHub", Url = "https://github.com", CreatedAt = DateTime.Now };

		var id = repository.AddAsync(website).Result;

		var result = await repository.GetByIdAsync(9999);

		Assert.Null(result);
	}

	[Fact]
	public async Task UpdateAsyncWithMissingIdThrowsInvalidOperationException()
	{
		var website = new Website
		{
			Id = 999,
			Name = "Nonexistent",
			Url = "https://nowhere.example",
			CreatedAt = DateTime.Now
		};

		await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateAsync(website));
	}

	[Fact]
	public async Task UpdateAsyncWithMissingIdThrowsInvalidOperationExceptionWithRightId()
	{
		// Arrange
		var website = new Website
		{
			Id = 999,
			Name = "Nonexistent",
			Url = "https://nowhere.example",
			CreatedAt = DateTime.Now
		};

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => repository.UpdateAsync(website));

		Assert.Contains("999", ex.Message);
		Assert.Equal("Website with Id 999 not found.", ex.Message);
	}

	[Fact]
	public async Task AddThenGetAllRoundTripsThroughFile()
	{
		var original = new Website
		{
			Name = "Anthropic",
			Url = "https://anthropic.com",
			CreatedAt = DateTime.Now
		};

		await repository.AddAsync(original);

		var freshRepository = new JsonFileWebsiteRepository(filePath);
		var result = await freshRepository.GetAllAsync();

		var loaded = Assert.Single(result);
		Assert.Equal("Anthropic", loaded.Name);
		Assert.Equal("https://anthropic.com", loaded.Url);
	}
}
