﻿using System.Text.Json.Serialization;

namespace PersonTrackApi.Utitilies.Results;
public class BaseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    [JsonConstructor]
    public BaseResponse(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }
}
