using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace Shikhsa.Services
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(
            string viewName,
            TModel model,
            IDictionary<string, object>? viewBagData = null);
    }

    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(
            string viewName,
            TModel model,
            IDictionary<string, object>? viewBagData = null)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };

            var actionContext = new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor());

            using var sw = new StringWriter();

            var viewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: false);

            if (!viewResult.Success)
            {
                // Agar ~/Views/... path se nahi mila, GetView se bhi try karo
                viewResult = _viewEngine.GetView(
                    executingFilePath: null,
                    viewPath: viewName,
                    isMainPage: false);
            }

            if (!viewResult.Success)
            {
                throw new InvalidOperationException(
                    $"View '{viewName}' not found. Searched locations: " +
                    string.Join(", ", viewResult.SearchedLocations ?? Array.Empty<string>()));
            }

            var viewDictionary = new ViewDataDictionary<TModel>(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = model
            };

            if (viewBagData != null)
            {
                foreach (var item in viewBagData)
                {
                    viewDictionary[item.Key] = item.Value;
                }
            }

            var tempData = new TempDataDictionary(
                actionContext.HttpContext,
                _tempDataProvider);

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDictionary,
                tempData,
                sw,
                new HtmlHelperOptions());

            await viewResult.View.RenderAsync(viewContext);

            return sw.ToString();
        }
    }
}