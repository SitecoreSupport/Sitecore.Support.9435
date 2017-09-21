using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.XA.Feature.CreativeExchange.Extensions;
using Sitecore.XA.Feature.CreativeExchange.Pipelines.Import.RenderingProcessing;
using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sitecore.Support.XA.Feature.CreativeExchange.Pipelines.Import.RenderingProcessing
{
    public class StylesImporter : Sitecore.XA.Feature.CreativeExchange.Pipelines.Import.RenderingProcessing.StylesImporter
    {
        protected override void CreateMissingComponentClass(ImportRenderingProcessingArgs args, string parameterName, List<string> parameterValues)
        {
            // fix 9435
            LayoutField field = new LayoutField(args.Page);

            string layoutXml = field.Value;
            string deviceLayoutXml = GetCurrentDeviceLayoutXml(args, layoutXml, args.Page.DisplayName, args.ImportContext.DeviceId);
            string renderingXmlNode = GetRenderingXmlNode(args, args.Page, deviceLayoutXml, args.RenderingUniqueId);

            if (renderingXmlNode == null)
            {
                args.Messages.AddWarning(Translate.Text(Sitecore.XA.Feature.CreativeExchange.Texts.CantFindTheComponentWithIdOnThePageMaybeItIsLoadedFromTheFallbackDevice), args.Rendering.ID, args.Page.DisplayName);
                return;
            }

            Item designsItem = PresentationContext.GetPageDesignsItem(args.Page);
            if (designsItem == null)
            {
                return;
            }

            Item stylesItem = designsItem.Parent.FirstChildInheritingFrom(Sitecore.XA.Foundation.Presentation.Templates.Styles.ID);
            List<string> usedClasses = GetUsedClasses(args, stylesItem, args.Rendering, parameterValues).ToList();

            Match paramsMatch = Regex.Match(renderingXmlNode, GetParametersString(@"([^""]+)"));
            string renderingParamsStringValue = paramsMatch.Groups[1].Value;
            string newRenderingParamsString = CreateNewParamsString(args, renderingParamsStringValue, parameterName, usedClasses);
            string newRenderingXmlNode = paramsMatch.Success ? renderingXmlNode.Replace(paramsMatch.Value, newRenderingParamsString) : renderingXmlNode.Insert(renderingXmlNode.LastIndexOf('\"') + 1, " " + newRenderingParamsString);
            string newDeviceLayoutXml = deviceLayoutXml.Replace(renderingXmlNode, newRenderingXmlNode);
            layoutXml = layoutXml.Replace(deviceLayoutXml, newDeviceLayoutXml);

            if (field.Value != layoutXml)
            {
                // fix 9435
                args.Page.Editing.BeginEdit();
                field.Value = layoutXml.Replace(GetParametersString(string.Empty), string.Empty);
                // fix 9435
                args.Page.Editing.EndEdit();
            }
        }
    }
}