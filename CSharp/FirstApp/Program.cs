using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace FirstApp
{
    class Program
    {
        private const string _defaultConnectString = "mongodb://localhost:27017";
        private const string _defaultDbName = "firstapp_net";
        private const string _defaultCollName = "locations";
        private static string _connectString;
        private static string _dbName;
        private static string _collName;
        private static MongoClient _connection;
        private static MongoServer _server;

        static void Main(string[] args)
        {
            Initialize();
            ConnectToMongo();
            Reset();
            LocationV1();
        }

        private static void Initialize()
        {
            Console.WriteLine("Initializing from config file");
            try
            {
                _connectString = ConfigurationSettings.AppSettings["connectstring"];
            }
            catch
            {
                _connectString = _defaultConnectString;
            }
            try
            {
                _dbName = ConfigurationSettings.AppSettings["dbname"];
            }
            catch
            {
                _dbName = _defaultDbName;
            }
            try
            {
                _collName = ConfigurationSettings.AppSettings["collname"];
            }
            catch
            {
                _collName = _defaultCollName;
            }
        }

        private static void Reset()
        {
            var db = _server[_dbName];
            db.Drop();
        }

        private static void ConnectToMongo()
        {
            _connection = new MongoClient(_connectString);
            _server = _connection.GetServer();
        }

        private static void LocationV1()
        {
            Console.WriteLine("  **  Executing location 1 script  **  ");
            var db = _server[_dbName];
            var coll = db[_collName];
            var location1 = new BsonDocument
            {
                {"name", "Taj Mahal"},
                {"address", "123 University Ave"},
                {"city", "Palo Alto"},
                {"zipcode", 94301}
            };
            coll.Insert(location1);
            var query = Query.EQ("name", "Taj Mahal");
            var retrievedLocation1 = coll.FindOne(query);
            Console.WriteLine(retrievedLocation1.ToJson());

            Console.WriteLine("Explain plan without index");
            Console.ReadLine();

            var explainWithoutIndex = coll.Find(query).Explain(true);
            Console.WriteLine(explainWithoutIndex.ToJson());

            coll.EnsureIndex("name");

            Console.WriteLine("Explain plan without index");
            Console.ReadLine();
            var explainWithIndex = coll.Find(query).Explain(true);
            Console.WriteLine(explainWithIndex.ToJson());

        }
    }
}
