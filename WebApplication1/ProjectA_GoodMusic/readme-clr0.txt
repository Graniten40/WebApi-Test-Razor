To create the AppWebApi
All applications are using the same software stack

1. Create the database. With Terminal in folder _scripts 
   E.g. database name: sql-music, database type: sqlserver, server: docker, default user: dbo, application: ../AppWebApi
   use application as ../AppWebApi, ../AppRazor or ../AppMvc depending on application you want to build database for.
   
   macOs
   ./database-rebuild-all.sh sql-music sqlserver docker dbo ../AppWebApi
   ./database-rebuild-all.sh sql-music mysql docker dbo ../AppWebApi
   ./database-rebuild-all.sh sql-music postgresql docker dbo ../AppWebApi
   
   Windows
   ./database-rebuild-all.ps1 sql-music sqlserver docker dbo ../AppWebApi
   ./database-rebuild-all.ps1 sql-music mysql docker dbo ../AppWebApi
   ./database-rebuild-all.ps1 sql-music postgresql docker dbo ../AppWebApi

   Ensure no errors from build, migration or database update

2. From Azure Data Studio you can now connect to the database
   Use connection string from user secrets:
   connection string corresponding to Tag
   "sql-friends.<db_type>.docker.root"

3. Use Azure Data Studio to execute SQL script DbContext/SqlScripts/<db_type>/azure/initDatabase.sql

4. Run AppWebApi with or without debugger

   Without debugger:   
   Open a Terminal in folder AppWebApi run: 
   dotnet run -lp https 
   open url: https://localhost:7066/swagger

   Verify your can execute endpoint Admin/Environment and Guest/Info

5. Use endpoint Admin/SeedUsers to seed users into the the database. 
   Use Azure Data Studio to verify content in Users Table

6. Use endpoint Admin/Seed to seed the database, Admin/RemoveSeed to remove the seed
   Verify database seed with endpoint Guest/Info

7. As dbo you can now use and play with all endpoints