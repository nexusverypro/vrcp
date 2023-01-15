// ---------------------------------- NOTICE ---------------------------------- //
// VRCP is made with the MIT License. Notices will be in their respective file. //
// ---------------------------------------------------------------------------- //

/*
MIT License

Copyright (c) 2023 Nexus

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

namespace VRCP.Web
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;

    public interface IHttpProxyClient
    {
        IPromise<ResponseContext> Get(RequestOptions options);
        IPromise<ResponseContext> Post(RequestOptions options);
        IPromise<ResponseContext> Put(RequestOptions options);
        IPromise<ResponseContext> Patch(RequestOptions options);
        IPromise<ResponseContext> Delete(RequestOptions options);

        IPromise<ResponseContext> SendRequest(RequestContext context);
    }

    public class HttpProxyClient : IHttpProxyClient
    {
        static HttpProxyClient()
        {
        }

        public HttpProxyClient(string baseUrl)
        {
            this.baseUrl = baseUrl;

            Task.Factory.StartNew(async (s) =>
            {
                this.HandleRequests();
            }, this, TaskCreationOptions.LongRunning);
        }

        private static RequestContext CreateRequestContext(RequestOptions options)
        {
            RequestBodyData data = new RequestBodyData()
            {
                JsonData = options.jsonData,
                FormData = options.form,
                LiteralData = options.literalData
            };
            RequestBodyDataResult dataResult = data;

            RequestContext result = new RequestContext();
            result.Data = dataResult.Data;
            result.HttpMethod = options.httpMethod;
            result.Uri = options.uri;
            result.Headers = options.headers;
            result.Promise = new Promise<ResponseContext>();
            return result;
        }

        IPromise<ResponseContext> IHttpProxyClient.SendRequest(RequestContext context)
        {
            this.SendBatchedRequest(context);
            return context.Promise;
        }

        private void SendBatchedRequest(RequestContext context)
        {
            _requestQueue.Enqueue(context);
        }

        private async void HandleRequests()
        {
            for (; ; )
            {
                while (_requestQueue.Count <= 0) await Task.Delay(1);

                var context = _requestQueue.Dequeue();
                var time = DateTime.Now;
                var stopwatch = new Stopwatch();
                var client = new HttpClient();

                HttpContent content = null;
                if (context.Data is string) content = new StringContent(context.Data as string);
                else if (context.Data is Dictionary<string, string>) content = new FormUrlEncodedContent(context.Data as Dictionary<string, string>);
                else content = JsonContent.Create(context.Data, context.Data.GetType());

                var request = new HttpRequestMessage()
                {
                    Method = context.HttpMethod,
                    Content = content,
                    RequestUri = new Uri(this.baseUrl + context.Uri)
                };

                stopwatch.Start();
                var result = await client.SendAsync(request);
                stopwatch.Stop();

                string data = "";
                context.Promise.Resolve(new ResponseContext()
                {
                    TimeOfResponse = time,
                    Headers = result.Headers,
                    StatusCode = result.StatusCode,
                    Data = (IsSuccess(result) ? (
                                (data = await result.Content.ReadAsStringAsync()) == "" ? "" : "") 
                                    : "") == "" ? "" : data,
                    Ms = (int)stopwatch.ElapsedMilliseconds
                });
            }
        }

        private static bool IsSuccess(HttpResponseMessage message) => message.StatusCode >= HttpStatusCode.OK && message.StatusCode < HttpStatusCode.BadRequest;

        public IPromise<ResponseContext> Get(RequestOptions options)
        {
            throw new NotImplementedException();
        }

        public IPromise<ResponseContext> Post(RequestOptions options)
        {
            throw new NotImplementedException();
        }

        public IPromise<ResponseContext> Put(RequestOptions options)
        {
            throw new NotImplementedException();
        }

        public IPromise<ResponseContext> Patch(RequestOptions options)
        {
            throw new NotImplementedException();
        }

        public IPromise<ResponseContext> Delete(RequestOptions options)
        {
            throw new NotImplementedException();
        }

        private readonly string baseUrl;
        private Queue<RequestContext> _requestQueue = new Queue<RequestContext>();
    }

    public static class DictionaryExtenstions
    {
        public static HttpRequestHeaders ToRequestHeaders(this Dictionary<string, string> dict)
        {
            HttpRequestHeaders keyValuePairs = Activator.CreateInstance<HttpRequestHeaders>();
            foreach (var v in dict) keyValuePairs.Add(v.Key, v.Value);
            return keyValuePairs;
        }
    }
}
