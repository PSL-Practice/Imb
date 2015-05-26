using Imb.Utils;
using NUnit.Framework;

namespace TestImb.Utils
{
    [TestFixture]
    public class TestGoogleImagesLinkDecoder
    {
        private static string Url =
            "http://www.google.co.uk/imgres?" 
            + "imgurl=http://i.ytimg.com/vi/mZKH4v5htFM/hqdefault.jpg" 
            + "&imgrefurl=http://www.youtube.com/watch?v%3DmZKH4v5htFM&h=360&w=450&tbnid=Ns8N2ossxSp0EM:&zoom=1&docid=VBafmJkx0OuDVM&ei=NCBkVbvoEYHSU4CvgKAF&tbm=isch&ved=0CAwQMygIMAg4yAE";

        private static string NonImageUrl =
            "http://www.google.co.uk/aclk?" 
            + "sa=l&ai=CePIUSSVkVcz1LsemjAawlYKYArCj34kG2PillfQBuIz-prEHCAkQASDezc8eKAtgu76Xg9AKoAHww7jeA8gBB6kCoACEaifsu" 
            + "T6qBCZP0OLcQjPeKyQxnvueJwie8MP4iVAOSvdMVIPuw3L-ZlvQlzCaPMAFBaAGJoAH-LvHIYgHAZAHAqgHpr4bqAeTwhuoB5TC" 
            + "G9gHAeASn-PN_M-XppLLAQ&sig=AOD64_0VuaDdnmaO8Ui-sNPfKefLxurFzA&ctype=5&rct=j&q=&ved=0CPcBENgpMAA&adurl=" 
            + "http://clickserve.dartsearch.net/link/click%3Flid%3D92700005633045775%26ds_s_kwgid%3D58700000378955235%26ds_e_adid%3D65408536512%26ds_e_product_group_id%3D253753263672%26ds_e_product_id" 
            + "%3D10824093%26ds_e_product_merchant_id%3D6150726%26ds_e_product_country%3DGB%26ds_e_product_language%3Den%26ds_e_product_channel%3Donline%26ds_e_product_store_id%3D%26ds_e_ad_type%3Dpla%26ds_s_inventory_feed_id%3D97700000001003133%26ds_url_v%3D2%26ds_dest_url%3D" 
            + "http://www.zavvi.com/dvd/farscape-the-complete-seasons-1-4/10824093.html%253Futm_source%253Dgoogleprod%2526utm_medium%253Dcpc%2526utm_campaign%253Dgp_dvd%2526affil%253Dthggpsad%2526switchcurrency%253DGBP%2526shippingcountry%253DGB";

        [Test]
        public void GoogleImageLinkIsRecognised()
        {
            //Arrange
            var decoder = new GoogleImageLinkDecoder();
            
            //Act
            decoder.Decode(Url);

            //Assert
            Assert.That(decoder.IsImageLink, Is.True);
        }

        [Test]
        public void NonGoogleImageLinkIsRecognised()
        {
            //Arrange
            var decoder = new GoogleImageLinkDecoder();
            
            //Act
            decoder.Decode(NonImageUrl);

            //Assert
            Assert.That(decoder.IsImageLink, Is.False);
        }

        [Test]
        public void GoogleImageUrlIsExtracted()
        {
            //Arrange
            var decoder = new GoogleImageLinkDecoder();
            
            //Act
            decoder.Decode(Url);

            //Assert
            Assert.That(decoder.ImageUrl, Is.EqualTo("http://i.ytimg.com/vi/mZKH4v5htFM/hqdefault.jpg"));
        }
    }
}
