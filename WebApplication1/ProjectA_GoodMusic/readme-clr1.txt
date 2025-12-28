To create the AppRazor
All applications are using the same software stack

1. Create the database. With Terminal in folder _scripts 
   E.g. database name: sql-music, database type: sqlserver, server: docker, default user: dbo, application: ../AppWebApi
   use application as ../AppWebApi, ../AppRazor or ../AppMvc depending on application you want to build database for.
   
   macOs
   ./database-rebuild-all.sh sql-music sqlserver docker dbo ../AppRazor
   ./database-rebuild-all.sh sql-music mysql docker dbo ../AppRazor
   ./database-rebuild-all.sh sql-music postgresql docker dbo ../AppRazor
   
   Windows
   ./database-rebuild-all.ps1 sql-music sqlserver docker dbo ../AppRazor
   ./database-rebuild-all.ps1 sql-music mysql docker dbo ../AppRazor
   ./database-rebuild-all.ps1 sql-music postgresql docker dbo ../AppRazor

   Ensure no errors from build, migration or database update

2. From Azure Data Studio you can now connect to the database
   Use connection string from user secrets:
   connection string corresponding to Tag
   "sql-friends.<db_type>.docker.root"

3. Use Azure Data Studio to execute SQL script DbContext/SqlScripts/<db_type>/azure/initDatabase.sql



Running AppRazor using WebApi or Database as source:
----------------------------------------------------

4. We are not using security in this verions, so ensure that in appsettings.json 
   "DefaultDataUser": "dbo"

5. Run AppRazor with debugger

   Use menu alternative to seed and play around with data from the database

6. Use menu Select datasource to WebApi and see that you can access same data but through the WebApi
   
NOTE: From AppRazor and AppMvc perspective, the ONLY change is the DataAccess services injected to the DI 
      This is one of the strength of a well made software stack with loosly couple objects and alyers