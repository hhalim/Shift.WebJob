# Shift.WebJob
Runs Shift server inside an Azure WebJob container.

First, before deploying this app, sign up for Azure, and do this:
- Create a storage account. This is required for Azure WebJob.
- Create Azure Redis Cache or create an Azure SQL server resource and run the [Shift create_db.sql](https://github.com/hhalim/Shift/blob/master/Shift/Database/create_db.sql) against it.

Update the Shift WebJob App.config file with Azure connection strings.
```
<connectionStrings>
   <!--AZURE SQL 
    <add name="ShiftDBConnection" connectionString="Server=tcp:shiftdb.database.windows.net,1433;Initial Catalog=ShiftJobsDB;Persist Security Info=False;User ID=[username];Password=[password];MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" providerName="System.Data.SqlClient" />
    -->
    
    <add name="ShiftDBConnection" connectionString="shiftcache.redis.cache.windows.net:6379,password=[password],ssl=false,abortConnect=False" providerName="System.Data.Redis" />
</connectionStrings>

  <appSettings>
    <!-- Shift server settings -->
    <add key="MaxRunnableJobs" value="2" />
    <add key="ShiftWorkers" value="2" />
    <!--
    <add key="ShiftPID" value="fae0b0bdff8e4409b05011068f2c8054" />
    -->

    <add key="AssemblyFolder" value="client-assemblies\" />
    <!-- <add key="AssemblyListPath" value="client-assemblies\assemblylist.txt" /> -->

    <!-- 
    <add key="StorageMode" value="mssql" />
    <add key="StorageMode" value="mongo" />
    <add key="StorageMode" value="documentdb" />
    <add key="DocumentDBAuthKey" value="C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==" />
    -->
    <add key="StorageMode" value="redis" />

    <!-- Set to 0 or low 1 sec for StorageMode = redis-->
    <add key="ProgressDBInterval" value="00:00:00" />
    
    <add key="ForceStopServer" value="true" />
    <add key="StopServerDelay" value="5000" />

    <add key="TimerInterval" value="5000" />
    <add key="CleanUpTimerInterval" value="10000" />

    <!--
    <add key="AutoDeletePeriod" value="120" />
    <add key="ShiftEncryptionParametersKey" value="[OPTIONAL_ENCRYPTIONKEY]"/> 
    <add key="PollingOnce" value="true" />
    -->
  </appSettings>
```

Update the Azure website configuration, not the WebJob App.config with the storage connections string:
```
<add name="AzureWebJobsDashboard" connectionString="DefaultEndpointsProtocol=https;AccountName=[account];AccountKey=[key];" />
<add name="AzureWebJobsStorage" connectionString="DefaultEndpointsProtocol=https;AccountName=[account];AccountKey=[key];" />
```

There are two ways to deploy the Shift WebJob app:
- Manually compile, bundle, and deploy the web job to an Azure web site. Use Microsoft documentation here: <https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-create-web-jobs>
- Directly publish the project from Visual Studio. Open the project solution with Visual Studio 2015, right click on the Shift.WebJob project and click Publish as Azure WebJob, follow the pop-up wizard to push the app into your Azure account. If an Azure website is required, create one through the Azure portal and use it to populate the connection wizard.

Confirm that the web job app is running successfully from the Azure portal at App Services > [your website] > Web Jobs. To view the log file, use the Kudu dashboard at App Services > [your website] > Advanced Tools > Tools > WebJobs dashboard. Click the Shift web job, and click Toggle Output button to view log and check that no error are shown.

You can use the [Shift.Demo.Client](https://github.com/hhalim/Shift.Demo.Client) to send jobs to the Shift WebJob server. Please ensure that the configuration setting in the client app points correctly to the same Azure storage as configured for the server.  

## Stopping WebJob
Please note that stopping the Azure web job process is similar to pushing the off power switch, which means that all running jobs will be stuck in **running** status without actually running in the server. The zombie jobs status will change into an **error** status when the original web job process runs again.

My recommendation is to send **STOP** command to all the running jobs and also wait for jobs without cancelation handle to complete successfully first before turning off the web job server.  
