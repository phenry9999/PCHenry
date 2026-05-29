namespace SkyView.Domain;

public class LinkCheckResult
{
	public int Id { get; set; }
	public int WebsiteId { get; set; }
	public DateTime CheckedAt { get; set; }
	public LinkStatus Status { get; set; }
	public int? HttpStatusCode { get; set; }
	public long ResponseTimeMs { get; set; }
	public string? Error { get; set; }
}