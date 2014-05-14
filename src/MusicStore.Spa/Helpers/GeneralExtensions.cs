using System;
using Microsoft.AspNet.Mvc.Rendering;

namespace System.Web.Mvc.Html
{
    public static class GeneralExtensions
    {
        public static HtmlString Tag(this HtmlHelper htmlHelper, TagBuilder tagBuilder)
        {
            return htmlHelper.Raw(tagBuilder.ToString());
        }
    }
}