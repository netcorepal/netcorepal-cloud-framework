namespace NetCorePal.Extensions.Dto;

public static class ResponseDataExtensions
{
    public static ResponseData<TData> AsResponseData<TData>(this TData data, bool success = true, string message = "", int code = 0, IEnumerable<object>? errorData = null)
    {
        return new ResponseData<TData>(data: data, success: success, message: message, code: code, errorData: errorData);
    }

    public static async Task<ResponseData> AsResponseData(this Task task, bool success = true, string message = "", int code = 0, IEnumerable<object>? errorData = null)
    {
        await task;
        return new ResponseData(success: success, message: message, code: code, errorData: errorData);
    }


    public static async Task<ResponseData<TData>> AsResponseData<TData>(this Task<TData> data, bool success = true, string message = "", int code = 0, IEnumerable<object>? errorData = null)
    {
        return new ResponseData<TData>(data: await data, success: success, message: message, code: code, errorData: errorData);
    }
}

