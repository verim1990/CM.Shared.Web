using CM.Shared.Kernel.Application.Exceptions;
using CM.Shared.Kernel.Application.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace CM.Shared.Web.Middlewares
{
    public class ResponseWrapper
    {
        private readonly RequestDelegate _next;

        public ResponseWrapper(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IHostingEnvironment env)
        {
            CommonResponse response = null;
            HttpStatusCode code = HttpStatusCode.OK;
            var currentBody = context.Response.Body;

            using (var memoryStream = new MemoryStream())
            {
                // Set the current response to the memorystream.
                context.Response.Body = memoryStream;

                try
                {
                    await _next(context);

                    // Reset the body 
                    context.Response.Body = currentBody;
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var readToEnd = new StreamReader(memoryStream).ReadToEnd();
                    var objResult = JsonConvert.DeserializeObject(readToEnd);

                    code = (HttpStatusCode)context.Response.StatusCode;
                    response = context.Response.StatusCode == (int) HttpStatusCode.OK
                        ? new SuccessResponse(objResult)
                        : new CommonResponse(code);
                }
                catch (Exception e)
                {
                    var type = e.GetType();

                    context.Response.Body = currentBody;

                    if (type == typeof(DomainException) || type == typeof(AppException))
                    {
                        code = HttpStatusCode.BadRequest;
                        response = new ErrorResponse(code, e.Message);
                    }
                    else if (type == typeof(ValidationException))
                    {
                        code = HttpStatusCode.BadRequest;
                        response = new ValidationResponse((e as ValidationException)?.Errors);
                    }
                    else
                    {
                        code = HttpStatusCode.InternalServerError;
                        response = env.IsDevelopment()
                            ? new ErrorDevResponse(code, e.Message, e.StackTrace)
                            : new ErrorResponse(code, "Opsss something went wrong.");
                    }
                }
                finally 
                {
                    context.Response.StatusCode = (int)code;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                }
            }
        }
    }
}