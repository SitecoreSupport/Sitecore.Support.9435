using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.XA.Feature.CreativeExchange.Pipelines.Import.RenderingProcessing;
using Sitecore.XA.Foundation.IoC;
using Sitecore.XA.Foundation.SitecoreExtensions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sitecore.Support.XA.Feature.CreativeExchange.Pipelines.Import.RenderingProcessing
{
    public class RenderingDetailsExtractor : ImportRenderingProcessingBase
    {
        // Fields
        protected IContentRepository ContentRepository;

        // Methods
        protected virtual Item GetAttributeValueItem(ImportRenderingProcessingArgs args, string attributeName)
        {
            if (args.Attributes.ContainsKey(attributeName))
            {
                List<string> source = args.Attributes[attributeName];
                if (source.Any<string>())
                {
                    /*add args.Page.Language */
                    return this.ContentRepository.GetItem(ID.Parse(source.First<string>()), args.Page.Language);
                }
            }
            return null;
        }

        protected virtual ID GetRenderingUniqueId(ImportRenderingProcessingArgs args)
        {
            if (!args.Attributes.ContainsKey("data-uniqueid"))
            {
                return null;
            }
            return ID.Parse(args.Attributes["data-uniqueid"].First<string>());
        }

        public override void Process(ImportRenderingProcessingArgs args)
        {
            this.ContentRepository = ServiceLocator.Current.Resolve<IContentRepository>();
            args.RenderingSourceItem = this.GetAttributeValueItem(args, "data-pageid");
            args.Rendering = this.GetAttributeValueItem(args, "data-renderingid");
            args.RenderingUniqueId = this.GetRenderingUniqueId(args);
        }
    }
}
