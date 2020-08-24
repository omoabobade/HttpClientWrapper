using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace HttpClientWrapper
{

    public class ApiResponse<T>
    {
        private string _responseMessage;

        public string ResponseMessage
        {
            get
            {
                if (string.IsNullOrEmpty(_responseMessage))
                {
                    _responseMessage = " ";
                }
                return _responseMessage;
            }
            set { _responseMessage = value; }
        }


        public Exception Exception { get; private set; }

        public HttpStatusCode Status { get; private set; }
        private T _data;

        public T Data
        {
            get
            {
                try
                {
                    return _data;
                }
                catch
                {
                    return default(T);
                }
            }
            set
            {
                _data = value;

            }
        }

        public Dictionary<string, List<string>> Headers { get; set; }

        public bool IsSuccessful;

        public void NotOk(HttpStatusCode status, string msg = "")
        {
            Status = status;
            ResponseMessage = msg;
            Data = default(T);
            IsSuccessful = false;
            try
            {
                if (!string.IsNullOrEmpty(_responseMessage))
                {
                    var o = JsonConvert.DeserializeObject<ErrorMessage>(_responseMessage);
                    ResponseMessage = o.Message;
                }
            }
            catch { }

            //HandleLogout();
        }

        public void Error(HttpStatusCode status, Exception exception, string msg = "")
        {
            Status = status;
            ResponseMessage = msg;
            if (string.IsNullOrEmpty(ResponseMessage))
            {
                ResponseMessage = exception.Message;
            }

            Data = default(T);
            Exception = exception;
            IsSuccessful = false;

            //HandleLogout();
        }

        public void Ok(T data, HttpStatusCode status)
        {
            Status = status;
            Data = data;
            IsSuccessful = true;
        }
    }

}