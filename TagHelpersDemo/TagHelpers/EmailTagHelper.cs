﻿using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace WebApplication5.TagHelpers3
{
    public class EmailTagHelper : TagHelper
    {
        private const string EmailDomain = "contoso.com";
        public string MailTo { get; set; }
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";                                 // Replaces <email> with <a> tag
            var content = await output.GetChildContentAsync();
            var target = content.GetContent() + "@" + EmailDomain;
            output.Attributes.SetAttribute("href", "mailto:" + target);
            output.PreContent.SetHtmlContent("<strong>");
            output.PostContent.SetHtmlContent("</strong>");
            output.Content.SetContent(target);
        }
    }
}

/* 
public IActionResult Contact()
{
    ViewData["Message"] = "Your contact page.";


    return View();
}
*/