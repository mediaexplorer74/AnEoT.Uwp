﻿using Markdig.Renderers;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace AnEoT.Uwp.Helpers.CustomMarkdown.Renderer
{
    /// <summary>
    /// 自定义的<see cref="LinkInline"/>渲染器
    /// </summary>
    public class CustomLinkInlineRenderer : LinkInlineRenderer
    {
        private readonly string baseUri;

        /// <summary>
        /// 使用指定的参数构造<seealso cref="CustomLinkInlineRenderer"/>的新实例
        /// </summary>
        public CustomLinkInlineRenderer(string baseUri = null)
        {
            this.baseUri = baseUri;
        }

        /// <inheritdoc/>
        protected override void Write(HtmlRenderer renderer, LinkInline link)
        {
            if (link.Url is not null)
            {
                bool isAbsoluteUri = new Uri(link.Url, UriKind.RelativeOrAbsolute).IsAbsoluteUri;

                if (baseUri is not null && !isAbsoluteUri)
                {
                    if (baseUri.EndsWith("/"))
                    {
                        renderer.BaseUrl = new Uri(baseUri, UriKind.Absolute);
                    }
                    else
                    {
                        renderer.BaseUrl = new Uri($"{baseUri}/", UriKind.Absolute);
                    }
                }

                if (isAbsoluteUri is not true)
                {
                    link.Url = link.Url?.Replace(".md", ".html").Replace("README", "index");
                }
            }

            base.Write(renderer, link);
        }
    }
}
