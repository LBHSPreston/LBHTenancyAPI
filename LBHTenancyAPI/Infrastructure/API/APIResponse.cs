using System;
using System.Net;
using LBHTenancyAPI.Infrastructure.UseCase.Execution;
using LBHTenancyAPI.UseCases.ArrearsAgreements;
using Newtonsoft.Json;

namespace LBHTenancyAPI.Infrastructure.API
{
    /// <summary>
    /// API Response wrapper for all API responses
    /// If a request has been successful this will be denoted by the statusCode
    ///     Then the 'data' property will be populated
    /// If a request has not been successful denoted
    ///     Then the Error property will be populated
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class APIResponse<T> where T: class
    {
        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("error")]
        public APIError Error { get; set; }

        public APIResponse(IExecuteWrapper<T> executeWrapper)
        {
            if (executeWrapper.IsSuccess)
            {
                StatusCode = (int)HttpStatusCode.OK;
                Data = executeWrapper?.Result;
                return;
            }

            if (executeWrapper == null || (executeWrapper?.Error != null && !executeWrapper.Error.IsValid && executeWrapper.Error.ValidationErrors != null))
            {
                StatusCode = (int)HttpStatusCode.BadRequest;
                Error = executeWrapper.Error;
                return;
            }

            if (executeWrapper?.Error != null)
            {
                StatusCode = (int)HttpStatusCode.InternalServerError;
                Error = executeWrapper?.Error;
            }

        }

        public APIResponse(Exception ex)
        {
            StatusCode = (int) HttpStatusCode.InternalServerError;
            Error = new APIError(ex);
        }

        public APIResponse(ApiException ex)
        {
            StatusCode = (int)ex.StatusCode;
            Error = new APIError(ex);
        }
    }
}
