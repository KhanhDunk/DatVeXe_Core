using System;
using Helper.Enums;

namespace Models.DTO
{
    public class ResponseDTO<T>
    {
        public bool Success { get; set; }
        public ResponseCode Code { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public ResponseDTO() { }

        public ResponseDTO(bool success, string message, T? data = default, ResponseCode code = ResponseCode.Success)
        {
            Success = success;
            Code = code;
            Message = message;
            Data = data;
        }
    }
}
