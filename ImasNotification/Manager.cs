using Mastonet;
using Mastonet.Entities;

namespace ImasNotification
{
    class Manager
    {
        public MastodonClient Client { get; }
        public Manager(string instanceName, string clientId, string clientSecret, string accessToken)
        {
            var postApp = new AppRegistration
            {
                Instance = instanceName,
                ClientId = clientId,
                ClientSecret = clientSecret
            };
            Client = new MastodonClient(postApp, new Auth()
            {
                AccessToken = accessToken
            });
        }
    }
}
