using System.Globalization;
using Abstractions.Interfaces.Localization;
using Microsoft.AspNetCore.Http;

namespace Localization.Middlewares;

public class ScopedLocalizationMiddleware(IScopedStringLocalizer localizer) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var locale = CultureInfo.CurrentUICulture.Name;

        localizer.SetLocale(locale);

        await next(context);
    }
}