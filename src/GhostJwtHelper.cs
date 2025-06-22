using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;

namespace GhostIntegration;

internal static class GhostJwtHelper
{
    [JsonObject(MemberSerialization.OptIn)]
    private class Header
    {
        [JsonProperty(PropertyName = "alg")]
        public string Algorithm { get; }

        [JsonProperty(PropertyName = "kid")]
        public string KeyId { get; }

        [JsonProperty(PropertyName = "typ")]
        public string Type { get; }

        public Header(string keyId)
        {
            Algorithm = "HS256";
            KeyId = keyId;
            Type = "JWT";
        }

        public string ToBase64()
        {
            var json = JsonConvert.SerializeObject(this);
            var bytes = Encoding.UTF8.GetBytes(json);
            return Base64Url.EncodeToString(bytes);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    private class Payload
    {
        [JsonProperty(PropertyName = "iat")]
        public long TimeNow { get; }

        [JsonProperty(PropertyName = "exp")]
        public long TimeExpiration { get; }

        [JsonProperty(PropertyName = "aud")]
        public string Audience { get; }

        public Payload()
        {
            var now = DateTimeOffset.UtcNow;

            TimeNow = now.ToUnixTimeSeconds();
            TimeExpiration = now.AddMinutes(5).ToUnixTimeSeconds();
            Audience = "/admin/";
        }

        public string ToBase64()
        {
            var json = JsonConvert.SerializeObject(this);
            var bytes = Encoding.UTF8.GetBytes(json);
            return Base64Url.EncodeToString(bytes);
        }
    }

    public static string? GetToken(string apiKey)
    {
        var keyParts = apiKey.Split(':');
        if (keyParts.Length != 2 || keyParts[0].Length != 24 || keyParts[1].Length != 64)
        {
            Console.WriteLine("Invalid API key");
            return null;
        }

        var secret = ToByteArray(keyParts[1]);
        var header = new Header(keyParts[0]);
        var payload = new Payload();

        var body = header.ToBase64() + "." + payload.ToBase64();

        var hash = new HMACSHA256(secret);
        var signature = hash.ComputeHash(Encoding.UTF8.GetBytes(body));

        return body + "." + Base64Url.EncodeToString(signature);
    }

    private static byte[] ToByteArray(string hex)
    {
        if (hex.Length % 2 == 1)
            throw new Exception("Invalid hex value");

        var arr = new byte[hex.Length >> 1];

        for (var i = 0; i < hex.Length >> 1; ++i)
            arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));

        return arr;

        static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }
}
