using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Drawing.Printing;
using Microsoft.AspNetCore.Http;

namespace DirectDbWebApp.Extensions {
    public static class HttpRequestExtensions {
        // Метод расширения для проверки, был ли запрос асинхронным (AJAX)
        public static bool IsAsyncRequest(this HttpRequest request) {
            if (request == null) {
                throw new ArgumentNullException(nameof(request));
            }

            // Проверка наличия заголовка X-Requested-With и его значения
            return request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }
    }
}
