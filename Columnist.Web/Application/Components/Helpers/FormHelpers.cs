using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Columnist.Web.Application.Components.Helpers;

public static class FormHelpers
{
    public static IHtmlContent TextFieldFor<TModel, TValue>(
        this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression,
        string? requiredMessage = null, int? maxLength = null, string? maxLengthMessage = null)
    {
        var attributes = TextAttributes(requiredMessage, maxLength, maxLengthMessage);
        return Field(html, expression, html.TextBoxFor(expression, attributes));
    }

    public static IHtmlContent TextAreaFieldFor<TModel, TValue>(
        this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, int rows = 3,
        string? requiredMessage = null, int? maxLength = null, string? maxLengthMessage = null)
    {
        var attributes = TextAttributes(requiredMessage, maxLength, maxLengthMessage);
        attributes["rows"] = rows;
        return Field(html, expression, html.TextAreaFor(expression, attributes));
    }

    public static IHtmlContent DateFieldFor<TModel, TValue>(
        this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, DateTime? min = null, string? minMessage = null)
    {
        var attributes = new Dictionary<string, object> { ["class"] = "form-control", ["type"] = "date" };
        if (min is not null)
            attributes["min"] = min.Value.ToString("yyyy-MM-dd");
        if (minMessage is not null)
            attributes["data-min-message"] = minMessage;   // shown live by site.js

        return Field(html, expression, html.TextBoxFor(expression, "{0:yyyy-MM-dd}", attributes));
    }

    // The rules live in the FluentValidation validators, so the client-side attributes are passed in explicitly.
    private static Dictionary<string, object> TextAttributes(string? requiredMessage, int? maxLength, string? maxLengthMessage)
    {
        var attributes = new Dictionary<string, object> { ["class"] = "form-control" };
        if (requiredMessage is not null)
        {
            attributes["data-val"] = "true";
            attributes["data-val-required"] = requiredMessage;
        }
        if (maxLength is not null)
        {
            attributes["data-val"] = "true";
            attributes["data-val-length-max"] = maxLength.Value;   // site.js adds maxlength and the counter
            if (maxLengthMessage is not null)
                attributes["data-val-length"] = maxLengthMessage;
        }
        return attributes;
    }

    private static IHtmlContent Field<TModel, TValue>(
        IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IHtmlContent input)
    {
        var field = new HtmlContentBuilder();
        field.AppendHtml("<div class=\"mb-3\">");
        field.AppendHtml(html.LabelFor(expression, new { @class = "form-label" }));
        field.AppendHtml(input);
        field.AppendHtml(html.ValidationMessageFor(expression, null, new { @class = "text-danger" }));
        field.AppendHtml("</div>");
        return field;
    }
}