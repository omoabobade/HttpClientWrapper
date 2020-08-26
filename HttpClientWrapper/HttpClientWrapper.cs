using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Reflection;

namespace HttpClientWrapper
{
    public class ApiClient
    {
        public virtual string ContentType { get; set; }
        public virtual bool ReadString { get; set; }
        public HttpClient client { get; set; }

        public ApiClient(string baseurl, bool addheaders = true)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(baseurl);
            client.DefaultRequestHeaders.Accept.Clear();
            if (addheaders)
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            //client.DefaultRequestHeaders.Connection.Clear();
            //client.DefaultRequestHeaders.ConnectionClose = true;
            ContentType = "application/json";
            //ContentType = "application/x-www-form-urlencoded";
            ReadString = false;
        }

        public async Task<ApiResponse<T>> PostAsync<T>(object postdata, string url)
        {
            var rsp = await RequestAsync<T>(postdata, url, "POST");

            return rsp;
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string getdata, string url)
        {
            StringBuilder sb = new StringBuilder(url);
            if (!string.IsNullOrEmpty(getdata))
            {
                sb.Append(getdata);
            }
            url = sb.ToString();
            var rsp = await RequestAsync<T>(null, url, "GET");
            return rsp;
        }

        public async Task<ApiResponse<T>> PutAsync<T>(object putdata, string url)
        {
            return await RequestAsync<T>(putdata, url, "PUT");
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string deletedata, string url)
        {
            StringBuilder sb = new StringBuilder(url);
            if (!string.IsNullOrEmpty(deletedata))
            {
                sb.Append(deletedata);
            }
            url = sb.ToString();
            return await RequestAsync<T>(null, url, "DELETE");
        }

        private async Task<ApiResponse<T>> RequestAsync<T>(object postdata, string url, string method)
        {

            var resp = new ApiResponse<T>();

            HttpResponseMessage responseMessage = null;
            try
            {
                if (method == "POST")
                {
                    var param = SerializeData(postdata);
                    HttpContent contentPost = new StringContent(param, Encoding.UTF8, ContentType);
                    responseMessage = await client.PostAsync(url, contentPost);
                }
                else if (method == "GET")
                {
                    responseMessage = await client.GetAsync(url);
                }
                else if (method == "PUT")
                {
                    var param = SerializeData(postdata);
                    HttpContent contentPost = new StringContent(param, Encoding.UTF8, ContentType);
                    responseMessage = await client.PutAsync(url, contentPost);
                }
                else if (method == "DELETE")
                {
                    responseMessage = await client.DeleteAsync(url);
                }
                else
                {
                    responseMessage = client.GetAsync(url).Result;
                }
            }

            catch (Exception ex)
            {
                
            }

            if (responseMessage.IsSuccessStatusCode && !ReadString)
            {

                try
                {
                    string responseBody = await responseMessage.Content.ReadAsStringAsync();
                    T data = JsonConvert.DeserializeObject<T>(responseBody);
                    //T data = await responseMessage.Content.ReadAsAsync<T>();
                    resp.Ok(data, responseMessage.StatusCode);
                    resp.Headers = responseMessage.Headers.ToDictionary(g => g.Key, g => g.Value.ToList());

                }
                catch (Exception ex)
                {
                    resp.Error(responseMessage.StatusCode, ex);
                }
            }
            else if (responseMessage.IsSuccessStatusCode && ReadString)
            {
                try
                {
                    T data = default(T);
                    resp.Ok(data, responseMessage.StatusCode);
                    resp.ResponseMessage = await responseMessage.Content.ReadAsStringAsync();
                    resp.Headers = responseMessage.Headers.ToDictionary(g => g.Key, g => g.Value.ToList());
                }
                catch (Exception ex)
                {
                    resp.Error(responseMessage.StatusCode, ex);
                }
            }
            else
            {

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    resp.NotOk(responseMessage.StatusCode, responseMessage.ReasonPhrase);
                }
                else if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    resp.NotOk(responseMessage.StatusCode, responseMessage.ReasonPhrase);
                }
                else if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                {
                    resp.NotOk(responseMessage.StatusCode, responseMessage.ReasonPhrase);
                }
                else
                {
                    try
                    {
                        resp.NotOk(responseMessage.StatusCode, responseMessage.Content.ReadAsStringAsync().Result);
                    }
                    catch (Exception ex)
                    {
                        //resp.NotOk(responseMessage.StatusCode, responseMessage.Content.ReadAsAsync<object>().Result.ToString());
                        resp.Error(responseMessage.StatusCode, ex);
                        //resp.NotOk(responseMessage.StatusCode, responseMessage.ReasonPhrase);
                    }
                }

                try
                {
                    //just hecking for data
                    T data = JsonConvert.DeserializeObject<T>(await responseMessage.Content.ReadAsStringAsync());
                    resp.Data = data;
                }
                catch
                {

                }


            }
            return resp;
        }


        public ApiResponse<T> Post<T>(object postdata, string url)
        {
            return Request<T>(postdata, url, "POST");
        }

        public ApiResponse<T> Put<T>(object postdata, string url)
        {
            return Request<T>(postdata, url, "PUT");
        }

        public ApiResponse<T> Get<T>(string getdata, string url)
        {
            StringBuilder sb = new StringBuilder(url);
            if (!string.IsNullOrEmpty(getdata))
            {
                sb.Append(getdata);
            }
            url = sb.ToString();
            return Request<T>(null, url, "GET");
        }

        public ApiResponse<T> Delete<T>(string deletedata, string url)
        {
            StringBuilder sb = new StringBuilder(url);
            if (!string.IsNullOrEmpty(deletedata))
            {
                //if (!sb.ToString().EndsWith("/"))
                //{
                //    sb.Append("/");
                //}
                sb.Append(deletedata);
            }
            url = sb.ToString();
            return Request<T>(null, url, "DELETE");
        }

        private ApiResponse<T> Request<T>(object postdata, string url, string method)
        {
            HttpResponseMessage responseMessage = null;
            try
            {
                if (method == "POST")
                {
                    var param = SerializeData(postdata);
                    HttpContent contentPost = new StringContent(param, Encoding.UTF8, ContentType);
                    responseMessage = client.PostAsync(url, contentPost).Result;
                }
                else if (method == "GET")
                {
                    responseMessage = client.GetAsync(url).Result;
                }
                else if (method == "PUT")
                {
                    var param = SerializeData(postdata);
                    HttpContent contentPost = new StringContent(param, Encoding.UTF8, ContentType);
                    responseMessage = client.PutAsync(url, contentPost).Result;
                }
                else if (method == "DELETE")
                {
                    responseMessage = client.DeleteAsync(url).Result;
                }
                else
                {
                    responseMessage = client.GetAsync(url).Result;
                }

            }
            catch (Exception ex)
            {
                responseMessage.StatusCode = HttpStatusCode.InternalServerError;
                responseMessage.ReasonPhrase = string.Format("RestHttpClient.SendRequest failed: {0}", ex);
               // Log.Error(ex);
            }

            var resp = new ApiResponse<T>();


            if (responseMessage.IsSuccessStatusCode && !ReadString)
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<T>(responseMessage.Content.ReadAsStringAsync().Result);
                    resp.Ok(data, responseMessage.StatusCode);
                    resp.Headers = responseMessage.Headers.ToDictionary(g => g.Key, g => g.Value.ToList());
                }
                catch (Exception ex)
                {
                    resp.Error(responseMessage.StatusCode, ex);
                    //Log.Error(ex);
                }
            }
            else if (responseMessage.IsSuccessStatusCode && ReadString)
            {
                try
                {
                    T data = default(T);
                    resp.Ok(data, responseMessage.StatusCode);
                    resp.ResponseMessage = responseMessage.Content.ReadAsStringAsync().Result;
                    resp.Headers = responseMessage.Headers.ToDictionary(g => g.Key, g => g.Value.ToList());
                }
                catch (Exception ex)
                {
                    resp.Error(responseMessage.StatusCode, ex);
                   // Log.Error(ex);
                }
            }
            else
            {

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    resp.NotOk(responseMessage.StatusCode, responseMessage.ReasonPhrase);
                }
                else
                {
                    try
                    {
                        resp.NotOk(responseMessage.StatusCode, responseMessage.Content.ReadAsStringAsync().Result);
                    }
                    catch (Exception ex)
                    {
                        //resp.NotOk(responseMessage.StatusCode, responseMessage.Content.ReadAsAsync<object>().Result.ToString());
                        resp.Error(responseMessage.StatusCode, ex);
                        //resp.NotOk(responseMessage.StatusCode, responseMessage.ReasonPhrase);
                    }
                }
            }
            return resp;
        }

        public string SerializeData(object postdata)
        {
            var resp = "";
            switch (ContentType)
            {
                case "application/json":
                    resp = JsonConvert.SerializeObject(postdata);
                    break;
                case "application/x-www-form-urlencoded":
                    var properties = from p in postdata.GetType().GetProperties()
                                     where p.GetValue(postdata, null) != null
                                     select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(postdata, null).ToString());

                    resp = String.Join("&", properties.ToArray());
                    break;
            }
            return resp;
        }

        public string SerializePayload(object postdata, string type)
        {
            var resp = "";
            switch (type)
            {
                case "json":
                    resp = JsonConvert.SerializeObject(postdata);
                    break;
                case "qstring":
                    var properties = postdata.GetType().GetProperties()
                        .Where(x => x.GetValue(postdata, null) != null &&
                        (x.GetCustomAttributes(true).FirstOrDefault(attr => (attr as JsonIgnoreAttribute) != null) as JsonIgnoreAttribute) == null)
                        .Select<PropertyInfo, string>(property =>
                        {
                            string value = HttpUtility.UrlEncode(property.GetValue(postdata, null).ToString());
                            string name = property.Name;
                            JsonPropertyAttribute nameAttr = property.GetCustomAttributes(true)
                            .FirstOrDefault(x => (x as JsonPropertyAttribute) != null) as JsonPropertyAttribute;

                            if (nameAttr != null)
                            {
                                name = nameAttr.PropertyName;
                            }
                            return $"{name}={value}";
                        });

                    resp = "?" + String.Join("&", properties.ToArray());
                    break;
            }
            return resp;
        }
        public object Parse(Type dataType, string ValueToConvert)
        {
            TypeConverter obj = TypeDescriptor.GetConverter(dataType);
            object value = obj.ConvertFromString(null, CultureInfo.InvariantCulture, ValueToConvert);
            return value;
        }
    }
}
