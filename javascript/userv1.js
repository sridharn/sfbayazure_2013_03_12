db = db.getSiblingDB("firstapp")

user_1 = {
      _id: "sridhar@10gen.com",
      name: "Sridhar",
      twitter: "snanjund",
      checkins: [
       {location: "Lotus Flower", ts: ISODate("2012-09-21T11:52:27.442Z")},
       {location: "Taj Mahal", ts: ISODate("2012-09-22T07:15:00.442Z")}
      ]
  }

db.users.save(user_1)

db.users.ensureIndex({"checkins.location": 1})

// find all users who've checked in here:
db.users.find({"checkins.location":"Lotus Flower"}, {name:1, checkins:1})

// find the last 10 checkins here: - Warning!
db.users.find({"checkins.location":"Lotus Flower"}, {name:1, checkins:1}).sort({"checkins.ts": -1}).limit(10)
