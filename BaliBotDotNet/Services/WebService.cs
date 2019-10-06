using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BaliBotDotNet.Services
{
    public class WebService
    {
        private readonly HttpClient _http;

        public WebService(HttpClient http)
            => _http = http;

        public async Task<Stream> GetCatPictureAsync()
        {
            var resp = await _http.GetAsync("https://cataas.com/cat");
            return await resp.Content.ReadAsStreamAsync();
        }

        internal async Task<XKCDContainer> GetXKCDAsync(int? id, bool getRandom = false)
        {
            var rng = new Random();
            var jsonResponse = id == null ? await _http.GetAsync("http://xkcd.com/info.0.json") : await _http.GetAsync("http://xkcd.com/" + id + "/info.0.json");

            if (!jsonResponse.IsSuccessStatusCode)
            {
                return null;
            }

            using var document = JsonDocument.Parse(await jsonResponse.Content.ReadAsStringAsync());

            if (getRandom)
            {
                var numberOfComics = document.RootElement.GetProperty("num").GetInt32();
                return await GetXKCDAsync(rng.Next(1, numberOfComics + 1)); //.Next(a,b) returns [a,b[ interval
            }

            var title = document.RootElement.GetProperty("title").GetString();
            var alt = document.RootElement.GetProperty("alt").GetString();
            var image = (await _http.GetAsync(document.RootElement.GetProperty("img").GetString())).Content.ReadAsStreamAsync();
            var container = new XKCDContainer
            {
                Image = await image,
                AltText = alt,
                Title = title,
            };
            return container;
        }
    }

    public class XKCDContainer
    {
        public Stream Image { get; set; }
        public string Title { get; set; }
        public string AltText { get; set; }
    }
}
