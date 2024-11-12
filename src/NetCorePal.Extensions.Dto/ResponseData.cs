namespace NetCorePal.Extensions.Dto;

public class ResponseData
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="success"></param>
    /// <param name="message"></param>
    /// <param name="code"></param>
    /// <param name="errorData"></param>
    public ResponseData(bool success = true, string message = "", int code = 0, IEnumerable<object>? errorData = null)
    {
        Success = success;
        Message = message;
        Code = code;
        ErrorData = errorData ?? Array.Empty<object>();
    }

    public bool Success { get; protected set; }
    public string Message { get; protected set; }
    public int Code { get; protected set; }

    public IEnumerable<object> ErrorData { get; protected set; }
}


public class ResponseData<T> : ResponseData
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="success"></param>
    /// <param name="message"></param>
    /// <param name="code"></param>
    /// <param name="errorData"></param>
    public ResponseData(T data, bool success = true, string message = "", int code = 0, IEnumerable<object>? errorData = null) : base(success: success, message: message, code: code, errorData: errorData)
    {
        this.Data = data;
    }
    public T Data { get; protected set; }
}
