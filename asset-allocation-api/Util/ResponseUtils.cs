using Microsoft.AspNetCore.Mvc;
using asset_allocation_api.Config;
using asset_allocation_api.Models;
using Serilog.Context;
using System.Runtime.CompilerServices;

#pragma warning disable CA2254

namespace asset_allocation_api.Util
{
    public class ResponseUtils
    {
        public static ObjectResult ReturnResponse<T>(ILogger logger, Exception? ex, Response<T> resp, T data, int statusCode, bool statusSuccess, string message, [CallerMemberName] string callerName = "Unknown class", [CallerLineNumber] int callerLine = -1, params object?[] values)
        {
            if (data != null)
                resp.Data = data;
            resp.Message[AssetAllocationConfig.ConstResultString] = string.Format(message, values);
            resp.Status.Code = statusCode;
            resp.Status.Success = statusSuccess;
            resp.ExecuteEnd = DateTime.Now;

            using IDisposable a = LogContext.PushProperty("Method", callerName);
            using IDisposable b = LogContext.PushProperty("Line", callerLine);
            if (ex == null)
            {
                // logger.LogInformation(message, values);
            }
            else
            {
                logger.LogError(ex, message, values);
            }
            ObjectResult objectResult = new(resp)
            {
                StatusCode = statusCode
            };
            return objectResult;
        }
    }
}