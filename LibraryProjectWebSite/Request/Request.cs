using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Flurl.Http;

namespace LibraryProjectWebSite.Request
{
    public class Request
    {
        private FlurlClient _client;
        internal const string BaseUrl = "http://www.libraryproject.somee.com/";
        public Request()
        {
            _client = new FlurlClient(BaseUrl);
        }

        public Task<TResult> GetAsync<TResult>(string path,object headers=null)
        {
            return _client.WithHeaders(headers).Request(path).GetJsonAsync<TResult>();
        }

        public Task<TResult> PostAsync<TResult>(string path, object postData,Dictionary<string,string> headers=null)
        {
            return _client.WithHeaders(headers).Request(path).PostUrlEncodedAsync(postData).ReceiveJson<TResult>();
        }

    }
}