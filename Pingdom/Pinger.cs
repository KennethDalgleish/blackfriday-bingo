using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Komplett.HttpClient;

namespace blackfriday_bingo.Pingdom
{
    public class Pinger
    {
        private readonly Uri _uri;
        private const int IntervalMillis = 5000;
        private const int HttpTimeOut = 4000;

        private readonly JsonClient _jsonClient;

        public Pinger(Uri uri)
        {
            _uri = uri;
            _jsonClient = new JsonClient(uri.GetLeftPart(UriPartial.Authority));
        }

        public async void Start()
        {
            while (await Ping())
            {
                Thread.Sleep(IntervalMillis);
            }
        }

        private async Task<bool> Ping()
        {
            using (var cancellationTokenSource = new CancellationTokenSource(HttpTimeOut))
            {
                var watch = Stopwatch.StartNew();
                try
                {
                    var result = await _jsonClient.GetAsync(_uri.PathAndQuery, cancellationTokenSource.Token);

                    if (result.IsSuccessStatusCode)
                    {
                        Reporter.Add(PingReport.CreateSuccess(_uri, watch.ElapsedMilliseconds));
                    }
                    else
                    {
                        Reporter.Add(PingReport.CreateFailure(_uri, watch.ElapsedMilliseconds, result.StatusCode, result.ReasonPhrase));
                    }
                
                }
                catch (Exception e)
                {
                    Reporter.Add(PingReport.CreateFailure(_uri, watch.ElapsedMilliseconds, HttpStatusCode.SeeOther, e.Message));
                }
            }

            return true;
        }
    }
}