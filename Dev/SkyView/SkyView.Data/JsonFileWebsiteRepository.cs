using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SkyView.Domain;

namespace SkyView.Data;

public class JsonFileWebsiteRepository : IWebsiteRepository
{
	private readonly string filePath;

	// Static so we don't allocate a new options object per call.
	private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions() { WriteIndented = true };

	public JsonFileWebsiteRepository(string filePath)
	{
		this.filePath = filePath;
	}

	public async Task<int> AddAsync(Website website, CancellationToken cancellationToken = default)
	{
		var websites = await LoadAsync(cancellationToken);

		var newId = websites.Count == 0 ? 1 : websites.Max(website => website.Id) + 1;

		website.Id = newId;
		websites.Add(website);

		await SaveAsync(websites, cancellationToken);
		return newId;
	}

	public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
	{
		var websites = await LoadAsync(cancellationToken);

		var removed = websites.RemoveAll(w => w.Id == id);

		if (removed == 0)
		{
			throw new InvalidOperationException($"Website with Id {id} not found.");
		}

		await SaveAsync(websites, cancellationToken);
	}

	public async Task<IReadOnlyList<Website>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		var websites = await LoadAsync(cancellationToken);
		return websites;
	}

	public async Task<Website?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
	{
		var websites = await LoadAsync(cancellationToken);
		return websites.FirstOrDefault(x => x.Id == id);
	}

	public async Task UpdateAsync(Website website, CancellationToken cancellationToken = default)
	{
		var websites = await LoadAsync(cancellationToken);

		var index = websites.FindIndex(w => w.Id == website.Id);

		if (index < 0)
		{
			throw new InvalidOperationException($"Website with Id {website.Id} not found.");
		}

		websites[index] = website;
		await SaveAsync(websites, cancellationToken);
	}

	private async Task<List<Website>> LoadAsync(CancellationToken cancellationToken)
	{
		if (!File.Exists(filePath))
		{
			return new List<Website>();
		}

		var json = await File.ReadAllTextAsync(filePath, cancellationToken);

		if (string.IsNullOrEmpty(json))
		{
			return new List<Website>();
		}

		var websites = JsonSerializer.Deserialize<List<Website>>(json);
		return websites ?? new List<Website>();
	}

	private async Task SaveAsync(List<Website> websites, CancellationToken cancellationToken)
	{
		var json = JsonSerializer.Serialize(websites, serializerOptions);
		await File.WriteAllTextAsync(filePath, json, cancellationToken);
	}
}
