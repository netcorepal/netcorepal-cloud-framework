# webapi数据响应

为了让客户端能够更好地处理响应数据，我们需要对数据进行封装，使用一致的数据格式，这样客户端就可以根据数据格式进行统一的处理。

## RespnseData、ResponseData<T> 类

我们在包`NetCorePal.Extensions.AspNetCore`中定义了两个类来封装响应数据，分别是 `ResponseData` 和 `ResponseData<T>`。

`ResponseData` 类用于封装不带数据的响应，`ResponseData<T>` 类用于封装带数据的响应。

类型定义如下：

```csharp
public class ResponseData
{
    public ResponseData(bool success = true, string message = "", int code = 0, IEnumerable<object>? errorData = null)
    {
        Success = success;
        Message = message;
        Code = code;
        ErrorData = errorData ?? KnownException.EmptyErrorData;
    }

    public bool Success { get; protected set; }
    public string Message { get; protected set; }
    public int Code { get; protected set; }

    public IEnumerable<object> ErrorData { get; protected set; }
}


public class ResponseData<T> : ResponseData
{
    public ResponseData(T data, bool success = true, string message = "", int code = 0, IEnumerable<object>? errorData = null) : base(success: success, message: message, code: code, errorData: errorData)
    {
        this.Data = data;
    }
    public T Data { get; protected set; }
}
```

## AsResponseData 扩展方法

我们还定义了一个扩展方法 `AsResponseData`，用于将数据转换为 `ResponseData` 或 `ResponseData<T>` 对象。

```csharp
using NetCorePal.Extensions.AspNetCore;

var data = new MyData();
var responseData = data.AsResponseData();
```


## 使用示例

在控制器中，我们可以使用 `ResponseData` 和 `ResponseData<T>` 来封装响应数据。

```csharp
[HttpGet]
public async Task<ResponseData<IEnumerable<WeatherForecast>>> Get()
{
    var rng = new Random();
    var result = await Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
    {
        Date = DateTime.Now.AddDays(index),
        TemperatureC = rng.Next(-20, 55),
        Summary = Summaries[rng.Next(Summaries.Length)]
    }));

    return result.AsResponseData();
}
```