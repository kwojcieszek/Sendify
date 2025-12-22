namespace SendifyClientNet;

public class SendifyResult(bool result, int code)
{
    public bool Result { get; set; } = result;
    public int Code { get; set; } = code;
}