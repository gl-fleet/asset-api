using asset_allocation_api.Context;

namespace asset_allocation_api.Model.Token
{
    public class Token(string accessToken, string refreshToken, Personnel personnel)
    {
        public string AccessToken { get; set; } = accessToken;
        public string RefreshToken { get; set; } = refreshToken;
        public Personnel Personnel { get; set; } = personnel;
    }
}