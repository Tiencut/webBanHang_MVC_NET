using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SV22T1020761.Shop.Filters
{
    public class HandleExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<HandleExceptionFilter> _logger;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public HandleExceptionFilter(ILogger<HandleExceptionFilter> logger, ITempDataDictionaryFactory tempDataFactory)
        {
            _logger = logger;
            _tempDataFactory = tempDataFactory;
        }

        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            _logger?.LogError(ex, "Unhandled exception caught by filter");

            // Set friendly error message in TempData and redirect to Home/Index
            try
            {
                var tempData = _tempDataFactory.GetTempData(context.HttpContext);
                tempData["Error"] = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại hoặc liên hệ quản trị.";
            }
            catch
            {
                // ignore TempData failures
            }

            context.Result = new RedirectToActionResult("Index", "Home", null);
            context.ExceptionHandled = true;
        }
    }
}