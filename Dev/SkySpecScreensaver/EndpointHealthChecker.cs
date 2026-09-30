using System.Net;

namespace SkySpec.ScreenSaver;

internal sealed class EndpointHealthChecker : IDisposable
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    public async Task<EndpointHealth> CheckAsync(
        EnvironmentEndpoint endpoint,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint.Url);
            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            return new(
                endpoint,
                response.StatusCode == HttpStatusCode.OK,
                (int)response.StatusCode,
                response.ReasonPhrase);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(endpoint, false, null, "Timed out");
        }
        catch (HttpRequestException exception)
        {
            return new(endpoint, false, null, exception.Message);
        }
        catch (UriFormatException exception)
        {
            return new(endpoint, false, null, exception.Message);
        }
    }

    public void Dispose() => _httpClient.Dispose();
}

internal sealed record EndpointHealth(
    EnvironmentEndpoint Endpoint,
    bool IsHealthy,
    int? StatusCode,
    string? Detail);
