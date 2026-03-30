  
namespace TaxManager.Application.Common.Exception;

public class ServiceException: System.Exception
{
    public ServiceException(string message) : base(message) { }
    public ServiceException(string message, System.Exception inner) : base(message, inner) { }
}

public class NotFoundException : ServiceException
{
     // public NotFoundException(string resourceName, object id) 
     //     : base($"{resourceName} with id {id} not found.") { }
    public NotFoundException(string message) 
        : base(message) { }
}

public class UnauthorizedException : ServiceException
{
    public string? ErrorCode { get; }
    public UnauthorizedException(string message, string? errorCode = null)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
public class ApiException : ServiceException
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }

    public ApiException(int statusCode, string message, string? errorCode = null,
            System.Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
    
    public static ApiException BadRequest(string message, string? errorCode = null)
        => new(400, message, errorCode);
    
    public static ApiException Unauthorized(string message = "Unauthorized")
        => new(401, message);
}

public class BusinessRuleException : ServiceException
{
    public BusinessRuleException(string message, System.Exception? innerException = null) 
        : base(message, innerException)
    {
    }

    // Optional: Add error code property if needed
    public string ErrorCode { get; set; }

    public BusinessRuleException(string message, string errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

public class InternalServerErrorException : ServiceException
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }
    public InternalServerErrorException(int statusCode, string message, string? errorCode = null,
        System.Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}


