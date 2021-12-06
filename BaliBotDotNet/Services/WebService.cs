using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace BaliBotDotNet.Services
{
    public class WebService
    {
        private readonly HttpClient _http;
        private Dictionary<string, Func<Task<Stream>>> _animals;
        public WebService(HttpClient http)
        {
            _http = http;
            _http.Timeout = TimeSpan.FromSeconds(3);
            _animals = new Dictionary<string, Func<Task<Stream>>>
            {
                { "duck", GetDuckPictureAsync }
            };
        }

        internal async Task<Stream> GetCatPictureAsync(string word = "")
        {
            try
            {
                HttpResponseMessage resp;
                if (word.Equals("cute"))
                {
                    resp = await _http.GetAsync("https://cataas.com/cat/cute");
                }
                else if (word.Equals("gif"))
                {
                    resp = await _http.GetAsync("https://cataas.com/cat/gif");
                }
                else
                {
                    resp = await _http.GetAsync("https://cataas.com/cat");
                }
                return await resp.Content.ReadAsStreamAsync();
            }
            catch (Exception) //who needs to be specific Kappa
            {
                return null;
            }
        }

        internal async Task<Stream> GetDogPictureAsync()
        {
            var jsonResponse = await _http.GetAsync("https://dog.ceo/api/breeds/image/random");
            using var document = JsonDocument.Parse(await jsonResponse.Content.ReadAsStringAsync());
            var image = await (await _http.GetAsync(document.RootElement.GetProperty("message").GetString())).Content.ReadAsStreamAsync();
            return image;
        }

        internal async Task<Stream> GetDuckPictureAsync()
        {
            var jsonResponse = await _http.GetAsync("https://random-d.uk/api/v2/random");
            using var document = JsonDocument.Parse(await jsonResponse.Content.ReadAsStringAsync());
            var image = await (await _http.GetAsync(document.RootElement.GetProperty("url").GetString())).Content.ReadAsStreamAsync();
            return image;
        }

        internal async Task<Stream> GetFoxPictureAsync()
        {
            var jsonResponse = await _http.GetAsync("https://randomfox.ca/floof/");
            using var document = JsonDocument.Parse(await jsonResponse.Content.ReadAsStringAsync());
            var image = await (await _http.GetAsync(document.RootElement.GetProperty("image").GetString())).Content.ReadAsStreamAsync();
            return image;
        }

        internal async Task<float?> GetConversionRateAsync(string source, string destination)
        {
            using var jsonConfig = JsonDocument.Parse(File.ReadAllText(Environment.CurrentDirectory + "\\config.json"));
            string token = jsonConfig.RootElement.GetProperty("currencyKey").GetString();

            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string conversion = $"{source.ToUpper()}_{destination.ToUpper()}";
            var jsonResponse = await _http.GetAsync($"https://free.currconv.com/api/v5/convert?q={conversion}&compact=y&apiKey={token}");
            if (!jsonResponse.IsSuccessStatusCode)
            {
                return null;
            }
            using var document = JsonDocument.Parse(await jsonResponse.Content.ReadAsStringAsync());
            try
            {
                return float.Parse(document.RootElement.GetProperty(conversion).GetProperty("val").ToString());
            }
            catch (KeyNotFoundException)
            {
                return null;
            }

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

            var num = document.RootElement.GetProperty("num").ToString();
            var title = document.RootElement.GetProperty("title").GetString();
            var alt = document.RootElement.GetProperty("alt").GetString();
            var image = (await _http.GetAsync(document.RootElement.GetProperty("img").GetString())).Content.ReadAsStreamAsync();
            var container = new XKCDContainer
            {
                ID = num,
                Image = await image,
                AltText = alt,
                Title = title,
            };
            return container;
        }

        internal async Task<string> GetDadJokeAsync()
        {
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var jsonResponse = await _http.GetAsync("https://icanhazdadjoke.com/");
            if (!jsonResponse.IsSuccessStatusCode)
            {
                return null;
            }
            using var document = JsonDocument.Parse(await jsonResponse.Content.ReadAsStringAsync());
            return document.RootElement.GetProperty("joke").ToString();
        }
    }

    public class XKCDContainer
    {
        public string ID { get; set; }
        public Stream Image { get; set; }
        public string Title { get; set; }
        public string AltText { get; set; }
    }
}
