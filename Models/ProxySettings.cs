namespace BIProxy.Models;

public class BCSettings
{
    public string? BaseUrl { get; set; }
}

public class ProxySettings
{
    public int Port { get; set; }
    public string? ApiKey { get; set; }
}
