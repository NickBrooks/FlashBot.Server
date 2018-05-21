using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace FlashBot.Engine.Models
{
    public class RefreshToken : TableEntity
    {
        public RefreshToken(string token, string userId)
        {
            PartitionKey = token;
            RowKey = userId;
        }

        public RefreshToken()
        {

        }
    }

    public class JWTObject
    {
        public string token { get; set; }
        public long expiration { get; set; }
        public string refresh_token { get; set; }
    }

    public class AuthClaim
    {
        public string user_id { get; set; }
    }

    public class AuthHeader
    {
        public string alg { get; set; }
        public string typ { get; set; }
        public string issuer { get; set; }
        public long expiration { get; set; }

        public AuthHeader(long _expiration)
        {
            alg = "HS256";
            typ = "JWT";
            issuer = "FlashBot";
            expiration = _expiration;
        }

        public AuthHeader()
        {

        }
    }
}
