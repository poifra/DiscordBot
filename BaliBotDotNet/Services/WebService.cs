using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;

namespace BaliBotDotNet.Services
{
    public class WebService
    {
        private readonly HttpClient _http;
        public WebService(HttpClient http)
        {
            _http = http; 
            _http.Timeout = TimeSpan.FromSeconds(10);
        }

        internal async Task<(Stream,HttpStatusCode)> GetCatPictureAsync(string word = "")
        {
            try
            {
                HttpResponseMessage resp;
                if (word.Equals("cute"))
                {
                    resp = await _http.GetAsync("https://cataas.com/cat/cute");
                    return (await resp.Content.ReadAsStreamAsync(), resp.StatusCode);
                }
                else if (word.Equals("gif"))
                {
                    resp = await _http.GetAsync("https://cataas.com/cat/gif");
                    return (await resp.Content.ReadAsStreamAsync(), resp.StatusCode);
                }
                else
                {
                    resp = await _http.GetAsync("https://api.thecatapi.com/v1/images/search");
                }
                if (!resp.IsSuccessStatusCode)
                {
                    return (null,resp.StatusCode);
                }
                using var document = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
                var image = await (await _http.GetAsync(document.RootElement[0].GetProperty("url").GetString())).Content.ReadAsStreamAsync();
                return (image,resp.StatusCode);
            }
            catch (Exception) //who needs to be specific Kappa
            {
                return (null,HttpStatusCode.RequestTimeout);
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

        internal async Task<LichessContainer> GetLichessPuzzle()
        {
            var jsonResponse = await _http.GetAsync("https://lichess.org/api/puzzle/daily");
            using var document = JsonDocument.Parse(await jsonResponse.Content.ReadAsStringAsync());
            var puzzleID = document.RootElement.GetProperty("puzzle").GetProperty("id");
            List<string> solution = document.RootElement.GetProperty("puzzle").GetProperty("solution").Deserialize<List<string>>();
            var image = await _http.GetAsync($"https://lichess1.org/training/export/gif/thumbnail/{puzzleID}.gif");
            return new LichessContainer { ImageURL=$"https://lichess1.org/training/export/gif/thumbnail/{puzzleID}.gif", Solution = solution };
        }

        internal async Task<Stream> GetFoxPictureAsync()
        {
            var jsonResponse = await _http.GetAsync("https://randomfox.ca/floof/");
            using var document = JsonDocument.Parse(await jsonResponse.Content.ReadAsStringAsync());
            var image = await (await _http.GetAsync(document.RootElement.GetProperty("image").GetString())).Content.ReadAsStreamAsync();
            return image;
        }

        internal async Task<(HttpStatusCode, float?)> GetConversionRateAsync(string source, string destination)
        {
            using var jsonConfig = JsonDocument.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "config.json")));
            string token;
            try
            {
                token = jsonConfig.RootElement.GetProperty("currencyKey").GetString();
            }
            catch (KeyNotFoundException)
            {
                return (HttpStatusCode.NotAcceptable, null);
            }

            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            string conversion = $"{source.ToUpper()}_{destination.ToUpper()}";
            var jsonResponse = await _http.GetAsync($"https://free.currconv.com/api/v8/convert?q={conversion}&compact=y&apiKey={token}");
            float? conversionValue = null;

            if (jsonResponse.IsSuccessStatusCode)
            {
                try
                {
                    using var document = JsonDocument.Parse(await jsonResponse.Content.ReadAsStringAsync());
                    return (HttpStatusCode.OK, float.Parse(document.RootElement.GetProperty(conversion).GetProperty("val").ToString()));
                }
                catch (KeyNotFoundException)
                {
                    return (HttpStatusCode.NotAcceptable, null);
                }

            }

            //backup API
            else
            {
                token = jsonConfig.RootElement.GetProperty("currencyKeyBackup").GetString();
                jsonResponse = await _http.GetAsync($"https://api.currencyapi.com/v3/latest?apikey={token}&base_currency={source.ToUpper()}&currencies={destination.ToUpper()}");
                if (jsonResponse.IsSuccessStatusCode)
                {
                    try
                    {
                        using var document = JsonDocument.Parse(await jsonResponse.Content.ReadAsStringAsync());
                        var value = float.Parse(document.RootElement.GetProperty("data")
                            .GetProperty(destination.ToUpper())
                            .GetProperty("value").ToString());
                        return (HttpStatusCode.OK, value);
                    }
                    catch (KeyNotFoundException)
                    {
                        return (HttpStatusCode.NotAcceptable, null);
                    }

                }
            }
            return (jsonResponse.StatusCode, conversionValue) ;
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

        internal async Task<string> GetFactAsync()
        {
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var jsonResponse = await _http.GetAsync("https://uselessfacts.jsph.pl/api/v2/facts/random");
            if (!jsonResponse.IsSuccessStatusCode)
            {
                return null;
            }
            using var document = JsonDocument.Parse(await jsonResponse.Content.ReadAsStringAsync());
            return document.RootElement.GetProperty("text").ToString();
       
        }
    }

    public class XKCDContainer
    {
        public string ID { get; set; }
        public Stream Image { get; set; }
        public string Title { get; set; }
        public string AltText { get; set; }
    }
    public class LichessContainer
    { 
        public string ImageURL { get; set; }
        public List<string> Solution { get; set; }
    }
}
