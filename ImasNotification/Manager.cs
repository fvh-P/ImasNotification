using Mastonet;
using Mastonet.Entities;

namespace ImasNotification
{
    class Manager
    {
        public MastodonClient Client { get; }
        public Manager(string instanceName, string clientId, string clientSecret, string accessToken)
        {
            Client = new MastodonClient(new AppRegistration
            {
                Instance = instanceName,
                ClientId = clientId,
                ClientSecret = clientSecret
            }, new Auth()
            {
                AccessToken = accessToken
            });
        }
    }
}
