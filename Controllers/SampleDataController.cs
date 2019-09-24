using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace demo.Controllers
{
    [Route("api/[controller]")]
    public class SampleDataController : Controller
    {
//        private static string[] Summaries = new[]
//        {
//            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//        };

        [HttpGet("[action]")]
        public IEnumerable<WeatherForecast> WeatherForecasts()
        {
            var rng = new Random();
            var summaries = GetSummariesFromMongo();
            return Enumerable.Range(1, 10).Select(index => new WeatherForecast
            {
                DateFormatted = DateTime.Now.AddDays(index).ToString("d"),
                TemperatureC = rng.Next(-20, 55),
                Summary = summaries[rng.Next(summaries.Length)]
            });
        }

        private static string[] GetSummariesFromMongo()
        {
            var connString = GetMongoConnectionString();

            var mongoUrl = MongoUrl.Create(connString);
            var client = new MongoClient(mongoUrl);
            var database = client.GetDatabase(mongoUrl.DatabaseName);

            var collection = database.GetCollection<BsonDocument>("weather");
            var document = collection.FindSync<BsonDocument>(FilterDefinition<BsonDocument>.Empty).FirstOrDefault();

            var summaries = document["summaries"].AsBsonArray.Select(x => x.AsString).ToArray();
            return summaries;
        }

        private static string GetMongoConnectionString()
        {
            var servicesJson = Environment.GetEnvironmentVariable("VCAP_SERVICES");
            var services = JObject.Parse(servicesJson);
            var mongoConnString = services.SelectToken("mlab[0].credentials.uri").Value<string>();
            return mongoConnString;
        }


        public class WeatherForecast
        {
            public string DateFormatted { get; set; }
            public int TemperatureC { get; set; }
            public string Summary { get; set; }

            public int TemperatureF
            {
                get { return 32 + (int) (TemperatureC / 0.5556); }
            }
        }
    }
}