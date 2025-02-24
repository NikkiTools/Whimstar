using System.Net.Http.Json;
using System.Text.Json;

namespace Whimstar;

public static class ApiUtilities
{
    public static readonly JsonSerializerOptions JsonWebOptions = new(JsonSerializerDefaults.Web);
    private static readonly HttpClient Client = new();

    private const string ApiUrl = "https://gacha.lukefz.xyz/infinitynikki";

    public static async Task<AesKeyInfo?> GetAesKeyInfo()
    {
        return await Client.GetFromJsonAsync<AesKeyInfo>($"{ApiUrl}/keys", JsonWebOptions);
    }
}
