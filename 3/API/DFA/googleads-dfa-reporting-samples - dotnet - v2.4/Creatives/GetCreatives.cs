/*
 * Copyright 2015 Google Inc
 *
 * Licensed under the Apache License, Version 2.0(the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Linq;
using Google.Apis.Dfareporting.v2_4;
using Google.Apis.Dfareporting.v2_4.Data;
using System.IO;

namespace DfaReporting.Samples
{
    /// <summary>
    /// This example lists all existing active creatives for a given advertiser.
    /// To get an advertiser ID, run GetAdvertisers.cs.
    /// </summary>
    class GetCreatives : SampleBase
    {
        /// <summary>
        /// Returns a description about the code example.
        /// </summary>
        public override string Description
        {
            get
            {
                return "This example lists all existing active creatives for a given" +
                    " advertiser. To get an advertiser ID, run GetAdvertisers.cs.\n";
            }
        }

        /// <summary>
        /// Main method, to run this code example as a standalone application.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            SampleBase codeExample = new GetCreatives();
            Console.WriteLine(codeExample.Description);
            codeExample.Run(DfaReportingFactory.getInstance());
        }

        /// <summary>
        /// Run the code example.
        /// </summary>
        /// <param name="service">An initialized Dfa Reporting service object
        /// </param>
        public override void Run(DfareportingService service)
        {
            long profileId = long.Parse(_T("2256830"));
            long advertiserId = long.Parse(_T("4993567")); // Crackle
            StreamWriter creativesImagePreviews = new StreamWriter(@"CreativesImagePreviews.html");
            creativesImagePreviews.WriteLine(
                "<html><body><table border=1><tr><th>Creative Id</th><th>Creative Name</th><th>Creative Type</th>"
                + "<th>Creative Asset Type</th><th>Creative Asset Hacked URL</th><th>Creative Asset Hacked Image</th></tr>"
                );

            // Limit the fields returned.
            String fields = "nextPageToken,creatives(id,name,type,creativeAssets/assetIdentifier,htmlCode)";

            CreativesListResponse creatives;
            String nextPageToken = null;

            do
            {
                // Create and execute the campaigns list request.
                CreativesResource.ListRequest request = service.Creatives.List(profileId);
                request.Active = true;
                //request.AdvertiserId = advertiserId;
                request.Fields = fields;
                request.PageToken = nextPageToken;
                creatives = request.Execute();

                foreach (Creative creative in creatives.Creatives)
                {
                    // Console
                    Console.WriteLine("Found {0} type creative with Id {1} and Name \"{2}\" and HtmlCode \"{2}\".",
                        creative.Type, creative.Id, creative.Name, creative.HtmlCode);
                    if (creative.CreativeAssets != null)
                    {
                        if (creative.CreativeAssets.Any()) Console.WriteLine("    Creative Assessets (Type, ETag, Name):");
                        foreach (CreativeAsset creativeAsset in creative.CreativeAssets)
                        {
                            Console.WriteLine("    \"{0}\", \"{1}\", \"{2}\""
                                , creativeAsset.AssetIdentifier.Type, creativeAsset.AssetIdentifier.ETag, creativeAsset.AssetIdentifier.Name);
                        }
                    }

                    // HTML
                    if ((creative.CreativeAssets != null) && (creative.CreativeAssets.Any()))
                    {
                        for(var i = 0; i < creative.CreativeAssets.Count; i++)
                        {
                            var creativeAssetId = creative.CreativeAssets[i].AssetIdentifier;
                            if (i == 0) creativesImagePreviews.Write("<tr><td>{0}</td><td>{1}</td><td>{2}</td>", creative.Id, creative.Name, creative.Type);
                            else creativesImagePreviews.Write("<tr><td colspan=3></td>");
                            creativesImagePreviews.Write("<td>{0}</td>", creativeAssetId.Type);
                            switch (creativeAssetId.Type)
                            {
                                case "IMAGE":
                                case "HTML_IMAGE":
                                    var creativeImageAssetUrl = "https://s0.doubleclick.net/viewad/" + advertiserId + "/" + creativeAssetId.Name;
                                    creativesImagePreviews.WriteLine("<td>{0}</td><td><img src = \"{0}\"/></td></tr>", creativeImageAssetUrl);
                                    break;
                                case "FLASH":
                                    //<iframe src="https://s0.2mdn.net/4993567/1446831708014/1-Crackle_AttackTheBlock_160x600_WatchFree.html" width="160" height="600" frameborder="0" scrolling="no" allowfullscreen="true" style="width: 160px; height: 600px;"></iframe>
                                default:
                                    creativesImagePreviews.WriteLine("<td>{0}</td><td></td></tr>", creativeAssetId.Name);
                                    break;
                            }
                        }
                    }
                }

                // Update the next page token.
                nextPageToken = creatives.NextPageToken;
            } while (creatives.Creatives.Any() && !String.IsNullOrEmpty(nextPageToken));

            creativesImagePreviews.WriteLine("</table></body></html>");
            creativesImagePreviews.Close();
        }
    }
}
