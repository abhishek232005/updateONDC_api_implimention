namespace Kuvik.ONDC.Api.Configuration;

/// <summary>Loads a git-ignored local .env file without logging its values. Deployment environment variables always win.</summary>
public static class DotEnv
{
    public static void LoadForLocalDevelopment()
    {
        if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Production", StringComparison.OrdinalIgnoreCase))
            return;

        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),
            Path.Combine(Directory.GetCurrentDirectory(), "Kuvik.ONDC.Api", ".env")
        };

        var file = candidates.FirstOrDefault(File.Exists);
        if (file is null) return;

        foreach (var line in File.ReadLines(file))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;
            var separator = trimmed.IndexOf('=');
            if (separator < 1) continue;
            var key = trimmed[..separator].Trim();
            var value = trimmed[(separator + 1)..].Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
                Environment.SetEnvironmentVariable(key, value);
        }
    }
}
