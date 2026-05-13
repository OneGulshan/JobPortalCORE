using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JobPortalCORE.Filters
{

    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            // Ye line Azure Log Stream mein Error record karegi
            _logger.LogError(context.Exception, "Bhai, error aaya hai page: {Page}", context.HttpContext.Request.Path);

            // User ko technical error se bachane ke liye Home/Error par redirect karega
            context.Result = new RedirectToActionResult("Error", "Home", null);
        }
    }
}
