using System.Collections.Generic;

namespace HttpClientWrapper
{
    public class ErrorMessage
    {
        public bool Verdict { get; set; }
        public string Message { get; set; }
        public List<string> Messages { get; set; }


        public static ErrorMessage Send(bool v1, string v2)
        {
            return new ErrorMessage()
            {
                Verdict = v1,
                Message = v2
            };
        }
    }
}