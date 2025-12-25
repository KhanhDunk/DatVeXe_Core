namespace backend.Helper.Enums;

public enum ResponseCode
{
    Success = 200, // Thành công 
    InvalidData = 400, // Dữ liệu không hợp lệ 
    UserExists = 401, // Không tồn tại 
    ServerError = 500 // Lỗi server 
}
