using System.Collections.ObjectModel;

namespace ImasNotification
{
    class PostManager : Manager
    {
        public ObservableCollection<PostContent> Col { get; set; }
        public PostManager(string instanceName, string clientId, string clientSecret, string accessToken) : base(instanceName, clientId, clientSecret, accessToken)
            => Col = new ObservableCollection<PostContent>();
    }
}
