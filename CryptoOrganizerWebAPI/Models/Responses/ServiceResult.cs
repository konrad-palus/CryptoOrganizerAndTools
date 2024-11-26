namespace CryptoOrganizerWebAPI.Models.Responses;
using System.Collections.Generic;

public class ServiceResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; }

    public ServiceResult() { }

    public ServiceResult(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public ServiceResult(bool isSuccess, string message, List<string> errors)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors;
    }
}