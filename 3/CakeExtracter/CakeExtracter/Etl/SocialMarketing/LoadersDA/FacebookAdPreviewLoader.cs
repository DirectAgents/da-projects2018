using System;
using System.Collections.Generic;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.LoadersDA;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using FacebookAPI;
using FacebookAPI.Entities;

namespace CakeExtracter.Etl.SocialMarketing.LoadersDA
{
    public class FacebookAdPreviewLoader : Loader<FBAdPreview>
    {
        private TDadPreviewLoader tdAdPreviewLoader;

        public FacebookAdPreviewLoader(int accountId)
        {
            this.tdAdPreviewLoader = new TDadPreviewLoader(accountId);
        }

        protected override int Load(List<FBAdPreview> items)
        {
            var tDadItems = items.Select(i => CreateTDad(i)).ToList();
            tdAdPreviewLoader.AddUpdateDependentTDads(tDadItems);
            return tDadItems.Count;
        }

        public static TDad CreateTDad(FBAdPreview item)
        {
            var tdad = new TDad
            {
                ExternalId = item.AdId,
                Url = item.Url,
                Width = item.Width,
                Height = item.Height
            };
            return tdad;
        }

    }
}
