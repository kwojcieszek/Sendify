namespace SendifyClientNet
{
    public class SendifyResult
    {
        public bool Result { get; set; }
        public int Code { get; set; }

        public SendifyResult(bool result, int code)
        {
            Result = result;
            Code = code;
        }
    }
}