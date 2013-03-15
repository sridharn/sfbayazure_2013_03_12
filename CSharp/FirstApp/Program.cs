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
        private const string _defaultLocationCollName = "locations";
        private const string _defaultUserCollName = "users";
        private const string _defaultCheckinCollName = "checkins";
        private static string _connectString;
        private static string _dbName;
        private static string _locationCollName;
        private static string _userCollName;
        private static string _checkinCollName;
        private static MongoClient _connection;
        private static MongoServer _server;

        static void Main(string[] args)
        {
            Initialize();
            ConnectToMongo();
            Reset();
            LocationV1();
            LocationV2();
            LocationV3();
            UserV1();
            UserV2();
            SimpleStatsV1();
            SimpleStatsV2();
        }

        private static void Initialize()
        {
            Console.WriteLine("\nInitializing settings");
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
                _locationCollName = ConfigurationSettings.AppSettings["locationcollname"];
            }
            catch
            {
                _locationCollName = _defaultLocationCollName;
            }
            try
            {
                _userCollName = ConfigurationSettings.AppSettings["usercollname"];
            }
            catch
            {
                _userCollName = _defaultUserCollName;
            }
            try
            {
                _checkinCollName = ConfigurationSettings.AppSettings["checkincollname"];
            }
            catch
            {
                _checkinCollName = _defaultCheckinCollName;
            }
        }

        private static void Reset()
        {
            Console.WriteLine("\nReset db by dropping db {0}", _dbName);
            var db = _server[_dbName];
            db.Drop();
        }

        private static void ConnectToMongo()
        {
            Console.WriteLine("\nConnecting to MongoDB using connect string {0}", _connectString);
            _connection = new MongoClient(_connectString);
            _server = _connection.GetServer();
        }

        private static void LocationV1()
        {
            Console.WriteLine("\nExecuting location 1 scenario");
            var db = _server[_dbName];
            var coll = db[_locationCollName];
            CreateSampleLocationDoc(coll);
            var query = Query.EQ("name", "Taj Mahal");
            var retrievedLocation = coll.FindOne(query);
            Console.WriteLine(retrievedLocation.ToJson());

            Console.WriteLine("Explain plan without index");
            var explainWithoutIndex = coll.Find(query).Explain(true);
            Console.WriteLine(explainWithoutIndex.ToJson());

            coll.EnsureIndex("name");

            Console.WriteLine("Explain plan with index");
            var explainWithIndex = coll.Find(query).Explain(true);
            Console.WriteLine(explainWithIndex.ToJson());

        }

        private static void CreateSampleLocationDoc(MongoCollection<BsonDocument> coll)
        {
            var location1 = new BsonDocument
            {
                {"name", "Taj Mahal"},
                {"address", "123 University Ave"},
                {"city", "Palo Alto"},
                {"zipcode", 94301}
            };
            coll.Insert(location1);
        }

        private static void LocationV2()
        {
            Console.WriteLine("\nExecuting location 2 scenario");
            var db = _server[_dbName];
            var coll = db[_locationCollName];
            var location2 = new BsonDocument
            {
                {"name", "Lotus Flower"},
                {"address", "234 University Ave"},
                {"city", "Palo Alto"},
                {"zipcode", 94301},
                {"tags", new BsonArray { "restaurant", "dumplings" }}
            };
            coll.Insert(location2);
            var query = Query.EQ("tags", "dumplings");
            var retrievedLocation = coll.FindOne(query);
            Console.WriteLine(retrievedLocation.ToJson());

            Console.WriteLine("Explain plan without index");
            var explainWithoutIndex = coll.Find(query).Explain(true);
            Console.WriteLine(explainWithoutIndex.ToJson());

            coll.EnsureIndex("tags");

            Console.WriteLine("Explain plan with index");
            var explainWithIndex = coll.Find(query).Explain(true);
            Console.WriteLine(explainWithIndex.ToJson());

        }

        private static void LocationV3()
        {
            Console.WriteLine("Executing location 3 scenario");
            var db = _server[_dbName];
            var coll = db[_locationCollName];
            var location3 = new BsonDocument
            {
                {"name", "El Capitan"},
                {"address", "345 University Ave"},
                {"city", "Palo Alto"},
                {"zipcode", 94301},
                {"tags", new BsonArray { "restaurant", "tacos" }},
                {"lat_long", new BsonArray { 52.5184, 13.387 }}
            };
            coll.Insert(location3);
            var query = Query.Near("lat_long", 52.53, 13.4);
            try
            {
                var retrievedLocation = coll.FindOne(query);
            }
            catch (MongoQueryException e)
            {
                Console.WriteLine("Expected exception");
                Console.WriteLine(e.Message);
            }

            coll.EnsureIndex(IndexKeys.GeoSpatial("lat_long"));

            Console.WriteLine("Explain plan with index");
            var explainWithIndex = coll.Find(query).Explain(true);
            Console.WriteLine(explainWithIndex.ToJson());

        }

        private static void UserV1()
        {
            Console.WriteLine("\nExecuting user 1 scenario");
            var db = _server[_dbName];
            var coll = db[_userCollName];
            var user = new BsonDocument
            {
                {"_id", "sridhar@10gen.com"},
                {"name", "Sridhar"},
                {"twitter", "snanjund"},
                {"checkins", new BsonArray 
                    {
                       new BsonDocument 
                       {
                           {"location", "Lotus Flower"}, 
                           {"ts", new DateTime(2012, 9, 21, 11, 52, 27, 442)}
                       },
                       new BsonDocument
                       {
                           {"location", "Taj Mahal"}, 
                           {"ts", new DateTime(2012, 09, 22, 7, 15, 0, 442)}
                       }
                    }
                }
            };
            coll.Save(user);
            coll.EnsureIndex("checkins.location");

            Console.WriteLine("Find all users who checked in at Lotus Flower");
            var query = Query.EQ("checkins.location", "Lotus Flower");
            var fields = new string[] {"name", "checkins"};
            var cursor = coll.Find(query);
            cursor.Fields = Fields.Include(fields);
            foreach(BsonDocument res in cursor)
            {
                Console.WriteLine(res.ToJson());
            }

            Console.WriteLine("Find the last 10 checkins at Lotus Flower");
            var newcursor = coll.Find(query).SetFields(Fields.Include(fields)).
                SetSortOrder(SortBy.Descending("checkins.ts")).SetLimit(10);
            foreach(BsonDocument res in newcursor)
            {
                Console.WriteLine(res.ToJson());
            }
        }

        private static void UserV2()
        {
            Console.WriteLine("\nExecuting user 2 scenario");
            var db = _server[_dbName];
            var usercoll = db[_userCollName];
            var user = new BsonDocument
            {
                {"_id", "sridhar@10gen.com"},
                {"name", "Sridhar"},
                {"twitter", "snanjund"}
            };
            usercoll.Save(user);
            var locationcoll = db[_locationCollName];
            CreateSampleLocationDoc(locationcoll);
            var locationDoc = locationcoll.FindOne(Query.EQ("name", "Taj Mahal"));
            var locationId = locationDoc["_id"];

            var checkin1 = new BsonDocument
            {
                {"location", locationId},
                {"user", "sridhar@10gen.com"},
                {"ts", new DateTime(2012, 9, 21, 11, 52, 27, 442)}
            };
            var checkinColl = db[_checkinCollName];
            checkinColl.Save(checkin1);
            checkinColl.EnsureIndex("user");

            Console.WriteLine("Finding checkins");
            foreach (var checkin in checkinColl.Find(Query.EQ(
                "user", "sridhar@10gen.com")))
            {
                Console.WriteLine(checkin.ToJson()); 
            }
        }

        private static void SimpleStatsV1()
        {
            Console.WriteLine("\nExecuting simple stats 1 scenario");
            var db = _server[_dbName];
            var coll = db[_userCollName];
            var user2 = new BsonDocument
            {
                {"_id", "user2@10gen.com"},
                {"name", "user2"},
                {"twitter", "user2"},
                {"checkins", new BsonArray 
                    {
                       new BsonDocument 
                       {
                           {"location", "Lotus Flower"}, 
                           {"ts", new DateTime(2012, 9, 21, 11, 52, 27, 442)}
                       },
                       new BsonDocument
                       {
                           {"location", "Taj Mahal"}, 
                           {"ts", new DateTime(2012, 09, 22, 7, 15, 0, 442)}
                       }
                    }
                }
            };
            coll.Save(user2);
            var user3 = new BsonDocument
            {
                {"_id", "user3@10gen.com"},
                {"name", "user3"},
                {"twitter", "user3"},
                {"checkins", new BsonArray 
                    {
                       new BsonDocument 
                       {
                           {"location", "Lotus Flower"}, 
                           {"ts", new DateTime(2012, 10, 22, 11, 52, 27, 442)}
                       }
                    }
                }
            };
            coll.Save(user3);

            // This query does not do what you expect
            // db.users.find({"checkins.location":"Lotus Flower"}, 
                // {name:1, checkins:1}).sort({"checkins.ts": -1}).limit(10)
            var cursor = coll.Find(Query.EQ("checkins.location", "Lotus Flower")).
                SetFields(Fields.Include(new string[] {"name", "checkins"} )).
                SetSortOrder(SortBy.Descending("checkins.ts")).SetLimit(10);

            foreach (var checkin in cursor)
            {
                Console.WriteLine(checkin.ToJson());
            }
        }

        private static void SimpleStatsV2()
        {
        }

    }
}
