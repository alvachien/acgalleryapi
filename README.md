# achihapi
Web API for [AC Gallery](https://github.com/alvachien/acgallery.git), built on ASP.NET Core.

## Install
To install this Web API to your own server, please follow the steps below.


### Step 1. Clone or Download
You can clone this [repository](https://github.com/alvachien/acgalleryapi.git) or download it.


### Step 2. Setup your own database.
You need setup your own database (SQL Server based), and run scripts under folder 'sqls':
DBSchema.sql

As the project was keep updated, the database schema changes are logged with order.

Also, the scripts have taken the upgrading into account. For instance, when adding a column to a table, the original table defintion will be updated directly, there will be a ALTER table script to handle existing tables.


### Step 3. Change the appsettings.json by adding the connection string:
The appsettings.json has been removed because it defines the connection strings to the database. This file is manadatory to make the API works. 

An example file look like following:
```javascript
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=XXX;Initial Catalog=XXX;Persist Security Info=True;User ID=XXX;Password=XXX;",
    "DebugConnection": "Server=XXX;Database=XXX;Integrated Security=SSPI;MultipleActiveResultSets=true"
  },
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  }
}
```


### Step 4. Deployment
By default, this Web API can deploy to IIS or IIS Express, or any other HTTP server which can host ASP.NET Core.


## Development Tools
Though the whole project was compiled with Visual Studio 2017 Community Version, the project can be processed by any IDE which supports ASP.NET Core.


## Unit Test
This unit test project also included. You can run the unit test to ensure the codes run successfully.


# Author
**Alva Chien (Hongjun Qian) | 钱红俊**

A programmer, a photographer and a father.
 
Contact me:

1. Via mail: alvachien@163.com. Or,
2. [Check my flickr](http://www.flickr.com/photos/alvachien). 


# Licence
MIT
