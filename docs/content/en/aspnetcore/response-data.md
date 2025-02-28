# WebAPI Data Response

To allow the client to better handle response data, we need to encapsulate the data and use a consistent data format so that the client can handle it uniformly based on the data format.

## ResponseData, ResponseData<T> Classes

We define two classes in the package `NetCorePal.Extensions.AspNetCore` to encapsulate response data, namely `ResponseData` and `ResponseData<T>`.

The `ResponseData` class is used to encapsulate responses without data, and the `ResponseData<T>` class is used to encapsulate responses with data.

The type definitions are as follows:

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

## AsResponseData Extension Method

We also define an extension method `AsResponseData` to convert data into `ResponseData` or `ResponseData<T>` objects.

```csharp
using NetCorePal.Extensions.AspNetCore;

var data = new MyData();
var responseData = data.AsResponseData();
```

## Usage Example

In the controller, we can use `ResponseData` and `ResponseData<T>` to encapsulate response data.

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