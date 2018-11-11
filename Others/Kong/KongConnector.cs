using CM.Shared.Kernel.Application.Settings;
using CM.Shared.Kernel.Others.Kong;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CM.Shared.Web.Others.Kong
{
    public class KongConnector
    {
        public HttpClient KongHttpClient { get; set; }

        public KongSettings KongSettings { get; set; }

        public KongServiceSettings KongServiceSettings { get; set; }

        public ServiceSettings ServiceSettings { get; set; }

        public KongConnector(KongSettings kongSettings, KongServiceSettings kongServiceSettings, ServiceSettings serviceSettings)
        {
            
            KongHttpClient = HttpClientFactory.Create();
            KongSettings = kongSettings;
            KongServiceSettings = kongServiceSettings;
            ServiceSettings = serviceSettings;

            if (kongSettings.Host != null && kongSettings.ApiPort.HasValue)
            {
                KongHttpClient.BaseAddress = new Uri($"http://{kongSettings.Host}:{kongSettings.ApiPort}");
            }
        }

        public async Task<bool> IsRunning()
        {
            try
            {
                var response = await KongHttpClient.GetStringAsync("/");

                return response.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsRegistered()
        {
            try
            {
                var response = await KongHttpClient.GetAsync($"/services/{KongServiceSettings.Name}");

                return response.StatusCode == System.Net.HttpStatusCode.OK;
            } catch
            {
                return false;
            }
        }

        public async Task Register()
        {
            if (await IsRegistered())
                return;

            await KongHttpClient.PostAsJsonAsync("/services", new
            {
                name = KongServiceSettings.Name,
                url = $"https://{ServiceSettings.LocalUrl}:443"
            });

            await KongHttpClient.PostAsJsonAsync($"/services/{KongServiceSettings.Name}/routes", new
            {
                preserve_host = true,
                paths = new List<string>()
                {
                    $"/{ServiceSettings.Path}"
                }
            });
        }
    }
}
