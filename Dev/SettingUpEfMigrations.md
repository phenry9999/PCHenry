# Setting up key projects

## App.Domain

1. create **App.Domain** class library project
2. create\place new entities\models in the Domain project

## App.Data

1. create **App.Data** class library project
2. using NuGet install Microsoft.EntityFrameworkCore.SqlServer => needed for SqlServer
3. using NuGet install Microsoft.EntityFrameworkCore.Tools => needed for migrations
4. create AppContext : DbContext
   4.1 add DbSet<TableName> TableNames {get; set;}, rinse and repeat for each table\model
   4.2 connection string can go in here in OnConfiguring()
5. Setup a project reference to App.Domain

# Setting EF Migrations

## In .Data project

1. add nuget Microsoft.EntityFrameworkCore.Tools, this contains the migration commands

## In EXE project

1. make sure this is an executable
2. add project references to **App.Data** and **App.Design**
3. add nuget Microsoft.EntityFrameworkCore.Design
4. set as startup project
5. in Package Manager Console (PMC), set Default Project dropdown to .Data project
6. in PMC run add-migration YOUR_DESCRIPTION
7. goto .Data and look at Migrations folder for your file with suffix YOUR_DESCRIPTION
