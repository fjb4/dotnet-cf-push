# .NET & Angular "cf push" Demo


Script
- Create application
    - `dotnet new angular --name demo`
- Show application in Visual Studio Code
    - `code .`
    - Show the .NET and Angular parts of the application
- Run the application locally
    - `dotnet watch run`
    - Show that the "Fetch Data" page is making an AJAX call
    - Change the WeatherForecasts API to return 10 days of data
- Deploy the application to Cloud Foundry
    - Introduce the "cf" CLI
        - `cf login`
        - `cf apps`
        - `cf push`
    - `dotnet publish -o publish`
    - `cf push mydemo -p publish --random-route`
        - Explain random route
        - Explain what happens when you push
        - Show app running in Cloud Foundry
        - Run `cf apps` again & `cf app mydemo`
        - Introduce App Manager (http://run.pivotal.io)
            - Alternative to CLI
            - Talk about scaling
            - Scale to 2 instances in App Manager
                - Scale back to 1 instance using `cf scale mydemo -i 1`
- Retrieve data from MongoDB
    - Introduce the Pivotal Marketplace
    - Create MongoDB database `cf create-service mlab sandbox myMongo`
    - Seed MongoDB with weather summaries data
    - Get URL to mlab `cf service myMongo`
    - Bind application to MongoDB service
        - `cf bind-service mydemo myMongo`
        - `cf env myMongo`
    - Update application code
        - `dotnet add package MongoDB.Driver --version 2.9.1`
        - `dotnet add package Newtonsoft.Json --version 12.0.2`
        - Add code snippets from below, explain how it works
    - Push the modified application
        - `dotnet publish -o publish`
        - `cf push -p publish`
- But, in the real world, you'd want to use CI/CD...

MongoDB document:
```
{
    "_id": {
        "$oid": "5d8521c47c213e556133ec04"
    },
    "summaries": [
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    ]
}
```


Add this code to SampleDataController:
```
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

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
```
