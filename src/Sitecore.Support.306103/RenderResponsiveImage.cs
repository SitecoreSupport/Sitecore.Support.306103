namespace Sitecore.Support.XA.Foundation.RenderingVariants.Pipelines.RenderVariantField
{
  using Sitecore;
  using Sitecore.Abstractions;
  using Sitecore.Data.Fields;
  using Sitecore.Data.Items;
  using Sitecore.DependencyInjection;
  using Sitecore.Resources.Media;
  using Sitecore.Web.UI.WebControls;
  using Sitecore.XA.Foundation.RenderingVariants;
  using Sitecore.XA.Foundation.RenderingVariants.Fields;
  using Sitecore.XA.Foundation.Variants.Abstractions.Models;
  using Sitecore.XA.Foundation.Variants.Abstractions.Pipelines.RenderVariantField;
  using System;
  using System.Collections.Specialized;
  using System.Linq;
  using System.Web;
  using System.Web.UI.HtmlControls;

  public class RenderResponsiveImage : RenderVariantFieldProcessor
  {
    public override Type SupportedType => typeof(VariantResponsiveImage);

    public override RendererMode RendererMode => RendererMode.Html;

    public override void RenderField(RenderVariantFieldArgs args)
    {
      VariantResponsiveImage variantResponsiveImage = args.VariantField as VariantResponsiveImage;
      if (args.Item.Paths.IsMediaItem)
      {
        args.ResultControl = CreateResponsiveImage(args.Item, variantResponsiveImage, args.Item[Templates.Image.Fields.Alt]);
        args.Result = RenderControl(args.ResultControl);
      }
      else if (!string.IsNullOrWhiteSpace(variantResponsiveImage?.FieldName))
      {
        if (Context.PageMode.IsExperienceEditorEditing)
        {
          args.ResultControl = new FieldRenderer
          {
            Item = args.Item,
            FieldName = variantResponsiveImage.FieldName,
            DisableWebEditing = !args.IsControlEditable
          };
          args.Result = RenderControl(args.ResultControl);
        }
        else
        {
          Field field = args.Item.Fields[variantResponsiveImage.FieldName];
          if (field != null)
          {
            ImageField imageField = FieldTypeManager.GetField(field) as ImageField;
            if (imageField?.MediaItem != null)
            {
              string altText = string.IsNullOrWhiteSpace(imageField.Alt) ? imageField.MediaItem[Templates.Image.Fields.Alt] : imageField.Alt;
              args.ResultControl = CreateResponsiveImage(imageField.MediaItem, variantResponsiveImage, altText);
              args.Result = RenderControl(args.ResultControl);
            }
          }
        }
      }
    }

    protected virtual HtmlImage CreateResponsiveImage(Item mediaItem, VariantResponsiveImage variantResponsiveImage, string altText)
    {
      string text = ServiceLocator.GetRequiredResetableService<BaseMediaManager>().Value.GetMediaUrl(mediaItem);
      string sourceSet = GetSourceSet(variantResponsiveImage.Widths, text);
      if (!string.IsNullOrWhiteSpace(variantResponsiveImage.DefaultSize))
      {
        text = AddWidthParam(text, variantResponsiveImage.DefaultSize);
      }
      HtmlImage htmlGenericControl = new System.Web.UI.HtmlControls.HtmlImage();
      htmlGenericControl.Attributes.Add("src", text);
      if (!string.IsNullOrWhiteSpace(altText))
      {
        htmlGenericControl.Attributes.Add("alt", altText);
      }
      if (!string.IsNullOrWhiteSpace(variantResponsiveImage.Sizes))
      {
        htmlGenericControl.Attributes.Add("sizes", variantResponsiveImage.Sizes);
      }
      if (!string.IsNullOrWhiteSpace(sourceSet))
      {
        htmlGenericControl.Attributes.Add("srcset", sourceSet);
      }
      return htmlGenericControl;
    }

    protected virtual string GetSourceSet(string widthsValue, string url)
    {
      string text = string.Empty;
      if (string.IsNullOrWhiteSpace(url))
      {
        return text;
      }
      string[] array = widthsValue.Split(new char[1]
      {
            ','
      }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string text2 in array)
      {
        if (int.TryParse(text2, out int _))
        {
          if (!string.IsNullOrWhiteSpace(text) && !text.EndsWith(",", StringComparison.OrdinalIgnoreCase))
          {
            text += ",";
          }
          text += $"{AddWidthParam(url, text2)} {text2}w";
        }
      }
      return text;
    }

    protected virtual string AddWidthParam(string mediaLink, string defaultSize)
    {
      if (!string.IsNullOrWhiteSpace(defaultSize))
      {
        int num = mediaLink.IndexOf("?", StringComparison.OrdinalIgnoreCase);
        NameValueCollection nameValueCollection = (num != -1) ? HttpUtility.ParseQueryString(mediaLink.Substring(num + 1)) : HttpUtility.ParseQueryString(string.Empty);
        if (nameValueCollection.AllKeys.Contains("w"))
        {
          nameValueCollection["w"] = defaultSize;
        }
        else
        {
          nameValueCollection.Add("w", defaultSize);
        }
        mediaLink = ((num == -1) ? (mediaLink + "?" + nameValueCollection) : (mediaLink.Substring(0, num) + "?" + nameValueCollection));
      }
      return ProtectAssetLink(mediaLink);
    }

    protected virtual string ProtectAssetLink(string link)
    {
      return HashingUtils.ProtectAssetUrl(link);
    }
  }
}