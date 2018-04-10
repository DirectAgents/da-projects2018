using System;
using System.Text.RegularExpressions;

namespace FacebookAPI.Entities
{
    public class FBAdPreview
    {
        // From https://developers.facebook.com/docs/marketing-api/generatepreview/v2.6
        public string AdId { get; set; }
        public string BodyHTML { get; set; }

        // Calculate parts of iframe.  Example: <iframe src="https://www.facebook.com/ads/api/preview_iframe.php?d=AQIWKVvIj2hxiP-vFIs92H8eYOPub-A436y4EydPAn2G1aKC0YWKOvw14de21NCAEC-oLaD6Lw7V3IrPiY33lqY_Y-yIa7sGmXT3RVd5QKntmf91kHaXvC3pbsJfQ61c6p8-3LZd5x0H9KQEBr11w5BZmYAVZTxAhkCM-U19D9_Me3dj4wwYQAhhQaOPMSYcMtTILKpDbujN4SIDAucj8N6EmfU6doJzQmqZJ0zzOlSnNQ&t=AQIxg-5nkAYgHIup" width="540" height="450" scrolling="yes" style="border: none;"></iframe>
        public string Url {
            get {
                if (BodyHTML != null) return Regex.Match(BodyHTML, @"<iframe src=""([^""]+)"" ").Groups[1].Value;
                else return null;
            }
        }

        public int Width {
            get {
                if (BodyHTML != null) return Int32.Parse(Regex.Match(BodyHTML, @"width=""([^""]+)""").Groups[1].Value);
                else return 0;
            }
        }

        public int Height
        {
            get
            {
                if (BodyHTML != null) return Int32.Parse(Regex.Match(BodyHTML, @"height=""([^""]+)""").Groups[1].Value) + 100; // Add to height to eliminate need for scrollbar in preview
                else return 0;
            }
        }
    
    }
}
