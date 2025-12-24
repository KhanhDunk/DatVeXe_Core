using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO
{
    public class ResponseDTO<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? Code { get; set; }
        public T? Data { get; set; }

        public ResponseDTO() { }

        public ResponseDTO(bool success, string message, T? data = default, string? code = null)
        {
            Success = success;
            Message = message;
            Data = data;
            Code = code;
        }

    }
}
