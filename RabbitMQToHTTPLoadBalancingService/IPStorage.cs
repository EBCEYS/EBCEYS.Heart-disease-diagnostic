using HeartDiseasesDiagnosticExtentions.DataSetsClasses;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RabbitMQToHTTPLoadBalancingService
{
    public class IPStorage
    {
        /// <summary>
        /// Key - ip,
        /// Value - current load
        /// </summary>
        public ConcurrentDictionary<string, int> IpsDictionary { get; set; } = new();
        private Logger logger;

        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            Converters = { new JsonStringEnumConverter() },
            WriteIndented = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public void SetIps(List<string> ips, Logger logger)
        {
            this.logger = logger;
            IpsDictionary = new();
            foreach (string ip in ips)
            {
                if (!IpsDictionary.TryGetValue(ip, out _))
                {
                    IpsDictionary[ip] = 0;
                }
            }
            this.logger.Info("Set ips: {@IpsDictionary}", IpsDictionary);
        }

        public string ToJson<T>(T obj)
        {
            return JsonSerializer.Serialize(obj, jsonSerializerOptions);
        }

        public async Task<string> ToJsonAsync<T>(T obj)
        {
            MemoryStream stream = new();
            await JsonSerializer.SerializeAsync(stream, obj, jsonSerializerOptions);
            return stream.ToString();
        }

        public T ToObject<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, jsonSerializerOptions);
        }
        public async Task<T> ToObjectAsync<T>(MemoryStream stream)
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, jsonSerializerOptions);
        }

        public async Task<string> RequestAndResponseAsync(BaseDiagnoseRequest message, double timeout)
        {
            List<string> listOfExcludedIps = new();
            string responseString = null;
            HttpClientHandler handler = new();
            HttpClient httpClient = new(handler)
            {
                Timeout = TimeSpan.FromSeconds(timeout)
            };
            try
            {
                for (int i = 0; i < IpsDictionary.Count; i++)
                {
                    string ip = GetLeastBusyIp(listOfExcludedIps);
                    if (string.IsNullOrEmpty(ip))
                    {
                        break;
                    }
                    IpsDictionary[ip]++;
                    UriBuilder uriBuilder = new($"{ip}/{message.Method ?? throw new Exception("Method is null!")}");
                    string id = Guid.NewGuid().ToString();
                    logger.Info("Try to post request {id} to {ip} with data {@message}", id, uriBuilder.Uri.AbsoluteUri, message);
                    HttpRequestMessage request = new(HttpMethod.Post, uriBuilder.Uri)
                    {
                        Content = new StringContent(ToJson(message), Encoding.UTF8)
                    };
                    (bool result, responseString) = await SendRequestAsync(httpClient, request, id);
                    if (!result)
                    {
                        IpsDictionary[ip]--;
                        listOfExcludedIps.Add(ip);
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Error on http requesting!");
            }
            return responseString;
        }

        private async Task<(bool,string)> SendRequestAsync(HttpClient client, HttpRequestMessage message, string id)
        {
            string responseString = null;
            try
            {
                HttpResponseMessage response = await client.SendAsync(message);
                responseString = await response.Content.ReadAsStringAsync();
                logger.Info("Get response {id}, {response}", id, responseString);
                return (true, responseString);
            }
            catch (Exception)
            {
                logger.Error("Error on sending http request!");
            }
            return (false, responseString);
        }

        private string GetLeastBusyIp(List<string> excludedIps = null)
        {
            if (IpsDictionary.Any())
            {
                KeyValuePair<string, int> result = IpsDictionary.FirstOrDefault();
                bool smthgChanged = false;
                foreach (KeyValuePair<string, int> val in IpsDictionary.ToArray())
                {
                    if (val.Value <= result.Value && !(excludedIps?.Exists(x => x == val.Key) ?? false))
                    {
                        result = val;
                        smthgChanged = true;
                    }
                }
                return smthgChanged ? result.Key : null;
            }
            return null;
        }
    }
}
