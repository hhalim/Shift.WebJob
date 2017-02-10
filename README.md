# Shift.WebJob
Runs Shift server inside an Azure WebJob container.

First, before deploying this app, sign up for Azure, and do this:
- Create a storage account. This is required for Azure WebJob.
- Create an Azure SQL server resource and run the [Shift create_db.sql](https://github.com/hhalim/Shift/blob/master/Shift/Database/create_db.sql) against it.
- Create an Azure Redis Cache.

Update the Shift WebJob App.config file with Azure SQL and Azure cache connection strings.
```
<connectionStrings>
  <!--AZURE SQL -->
  <add name="ShiftDBConnection" connectionString="Server=tcp:shiftdb.database.windows.net,1433;Initial Catalog=ShiftJobsDB;Persist Security Info=False;User ID=[username];Password=[password];MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" providerName="System.Data.SqlClient" />
</connectionStrings>

<appSettings>
  <add key="UseCache" value="true" /> <!-- Set to TRUE after RedisCache is up and running-->
  <add key="RedisConfiguration" value="shiftcache.redis.cache.windows.net:6379,password=[password],ssl=false,abortConnect=False" />
</appSettings>
```

Update the Azure website configuration, not the WebJob App.config with the storage connections string:
```
<add name="AzureWebJobsDashboard" connectionString="DefaultEndpointsProtocol=https;AccountName=[account];AccountKey=[key];" />
<add name="AzureWebJobsStorage" connectionString="DefaultEndpointsProtocol=https;AccountName=[account];AccountKey=[key];" />
```

There are two ways to deploy the Shift WebJob app:
- Manual. Compile, bundle and deploy the web job to an Azure web site. Use Microsoft documentation here: <https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-create-web-jobs>
- Publish from Visual Studio. Open the project solution with Visual Studio, right click on the Shift.WebJob project and click Publish as Azure WebJob, follow the pop-up wizard to push the app into your Azure account. If an Azure website is required, create one through the Azure portal and use it to populate the connection wizard.

Confirm that the web job app is running successfully from the Azure portal at App Services > [your website] > Web Jobs. To view the log file, use the Kudu dashboard at App Services > [your website] > Advanced Tools > Tools > WebJobs dashboard. Click the Shift web job, and click Toggle Output button to view log and check that no error are shown.

Use the [Shift.Demo.Client](https://github.com/hhalim/Shift.Demo.Client) to send jobs to the Shift WebJob. Please ensure that the configuratio setting in the client app points correctly to the Azure  
