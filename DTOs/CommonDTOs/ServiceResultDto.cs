namespace UserManagement.API.DTOs.CommonDTOs
{
    public class ServiceResultDto<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public T? Data { get; set; }

        public static ServiceResultDto<T> Ok(string message, T? data = default)
        {
            return new ServiceResultDto<T>
            {
                Success = true,
                Message = message,
                StatusCode = 200,
                Data = data
            };
        }

        public static ServiceResultDto<T> BadRequest(string message)
        {
            return new ServiceResultDto<T>
            {
                Success = false,
                Message = message,
                StatusCode = 400
            };
        }

        public static ServiceResultDto<T> Unauthorized(string message)
        {
            return new ServiceResultDto<T>
            {
                Success = false,
                Message = message,
                StatusCode = 401
            };
        }

        public static ServiceResultDto<T> NotFound(string message)
        {
            return new ServiceResultDto<T>
            {
                Success = false,
                Message = message,
                StatusCode = 404
            };
        }
    }
}