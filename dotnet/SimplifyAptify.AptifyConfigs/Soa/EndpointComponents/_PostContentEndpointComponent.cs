using Newtonsoft.Json;
using PB.Rexies.AptifyBits;
using System;

namespace SimplifyAptify.AptifyConfigs.Soa.EndpointComponents
{
    public abstract class PostContentEndpointComponent<TPostData> : BaseEndpointComponent where TPostData : class, new()
    {
        public string PostContent
        {
            get => Properties.GetString(nameof(PostContent));
            set => Properties[nameof(PostContent)] = value;
        }

        protected TPostData PostData { get; private set; }

        public void EnsureSetPostData()
        {
            if (string.IsNullOrWhiteSpace(PostContent))
            {
                throw new ArgumentException("Post content is required.");
            }

            PostData = JsonConvert.DeserializeObject<TPostData>(PostContent);
        }
    }
}