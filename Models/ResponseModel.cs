namespace Models;
public class ResponseModel<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
    public ResponseModel()
    {
        IsSuccess = true;
    }
    public ResponseModel(bool isSuccess, string message, T data)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
    }
}