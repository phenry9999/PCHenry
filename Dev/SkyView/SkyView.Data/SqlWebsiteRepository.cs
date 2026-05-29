using System.Text.Json;
using SkyView.Domain;

namespace SkyView.Data;

public class SqlWebsiteRepository : IWebsiteRepository
{

	public SqlWebsiteRepository(string connectionStringName)
	{

	}

	public Task<int> AddAsync(Website website, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<IReadOnlyList<Website>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<Website?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task UpdateAsync(Website website, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}