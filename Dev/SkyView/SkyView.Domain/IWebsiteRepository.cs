using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyView.Domain;

public interface IWebsiteRepository
{
	Task<IReadOnlyList<Website>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<Website?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
	Task<int> AddAsync(Website website, CancellationToken cancellationToken = default);
	Task UpdateAsync(Website website, CancellationToken cancellationToken = default);
	Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
