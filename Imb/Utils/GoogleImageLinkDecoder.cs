using System.Text.RegularExpressions;

namespace Imb.Utils
{
    public class GoogleImageLinkDecoder
    {
        public void Decode(string url)
        {
            var regex = new Regex(@"^http://www\.google\.[a-zA-Z.]*/imgres\?(?'params'.*)");
            var match = regex.Match(url);
            IsImageLink = match.Success && TryGetImageUrl(match.Result("$1"));
        }

        private bool TryGetImageUrl(string urlParams)
        {
            var regex = new Regex(@"imgurl=(?'imageUrl'[^&]*)");
            var match = regex.Match(urlParams);
            if (!match.Success)
                return false;

            ImageUrl = match.Result("$1");
            return true;
        }

        public bool IsImageLink { get; private set; }
        public string ImageUrl { get; private set; }
    }
}