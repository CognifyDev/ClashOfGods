using System.Net.Http;

namespace COG.Utils;

public static class WebUtils
{
    public static string GetWeb(string url)
    {
        using var client = new HttpClient();

        try
        {
            var response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError(e.Message);
        }

        return string.Empty;
    }
}