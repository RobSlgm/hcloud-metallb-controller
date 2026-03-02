using System;
using System.Net;
using RestSharp;

namespace HcloudSlimApi;

public record ApiResult<T>(bool IsSuccessful, HttpStatusCode? Status = null, T? Data = default, Uri? Uri = null, string? MimeType = null, long? ContentLength = null, string? ErrorMessage = null) { }


static class ApiResultExtensions
{
    extension(RestResponseBase response)
    {
        public ApiResult<T> Ok<T>(T? data = default)
        {
            return new ApiResult<T>(IsSuccessful: true, response.StatusCode, data, response.ResponseUri, response.ContentType, response.ContentLength, default);
        }

        public ApiResult<T> Nok<T>(string? msg = null)
        {
            return new ApiResult<T>(IsSuccessful: false, response.StatusCode, Uri: response.ResponseUri, ErrorMessage: msg);
        }
    }
}
