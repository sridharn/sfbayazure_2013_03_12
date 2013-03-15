Scripts and presentation from my talk at the SF Bay Area Azure Meetup on 3/12/2013 titled "MongoDB - A Hands on Intro"

* 2013_03_BayAzure_Intro.pdf - The slides
* javascript - A set of .js files corresponding to the "Building your first application" section of the talk. The code from these files should be directly executable from the MongoDB shell
* CSharp - A simple CSharp command line program that corresponds to the .js files above. There is a method per .js file and can be configured to connect to given mongodb instance. This uses the Official 10Gen MongoDB .Net driver which can be installed from nuget and has been tested with v1.7.1 of the driver
	* CSharp\app.config - The connection string, db name, collection names can be set here. Default is to connect to the default localhost instance running on port 27017