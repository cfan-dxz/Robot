using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;

namespace Robot.Driver
{
    public class HttpHelper
    {
        /// <summary>
        /// post obj
        /// </summary>
        /// <param name="url"></param>
        /// <param name="obj"></param>
        /// <param name="header"></param>
        /// <returns>string</returns>
        public static string Post(string url, object obj, Dictionary<string, string> header = null)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest(Method.POST);
            if (obj != null)
            {
                request.AddHeader("content-type", "application/json");
                request.AddParameter("application/json", JsonConvert.SerializeObject(obj), ParameterType.RequestBody);
            }
            if (header != null)
            {
                foreach (var key in header.Keys)
                {
                    request.AddHeader(key, header[key]);
                }
            }
            IRestResponse response = client.Post(request);
            return response.Content;
        }

        /// <summary>
        /// post obj deserialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="obj"></param>
        /// <param name="header"></param>
        /// <returns>obj</returns>
        public static T Post<T>(string url, object obj, Dictionary<string, string> header = null)
        {
            var result = Post(url, obj, header);
            return JsonConvert.DeserializeObject<T>(result);
        }

        /// <summary>
        /// get
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="header"></param>
        /// <returns>string</returns>
        public static string Get(string url, Dictionary<string, object> param = null, Dictionary<string, string> header = null)
        {
            return Request(url, param, header, Method.GET);
        }

        /// <summary>
        /// get deserialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="header"></param>
        /// <returns>obj</returns>
        public static T Get<T>(string url, Dictionary<string, object> param, Dictionary<string, string> header = null)
        {
            return Request<T>(url, param, header, Method.GET);
        }

        /// <summary>
        /// request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="header"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static string Request(string url, Dictionary<string, object> param = null, Dictionary<string, string> header = null, Method method = Method.GET)
        {
            RestClient client = new RestClient(url);
            RestRequest request = new RestRequest(method);
            if (param != null)
            {
                foreach (var item in param)
                {
                    request.AddParameter(item.Key, item.Value);
                }
            }
            if (header != null)
            {
                foreach (var key in header.Keys)
                {
                    request.AddHeader(key, header[key]);
                }
            }
            IRestResponse response = method == Method.GET ? client.Get(request) : client.Post(request);
            return response.Content;
        }

        /// <summary>
        /// request deserialization
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="header"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static T Request<T>(string url, Dictionary<string, object> param = null, Dictionary<string, string> header = null, Method method = Method.GET)
        {
            string result = Request(url, param, header, method);
            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
