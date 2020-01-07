## External hard drive trawler
A C# Windows Forms application that allows the directory structure of multiple hard drives to be traversed and the contents saved to a MySql database. The app was tested with the MariaDB 10.1.21 database installed as part of XAMPP Version: 7.0.16 but it should work with any MySql compatible database. The following line in MySqlDB.cs with need to be changed for the server and login credentials of the database:

`public static string connectionString = "server=localhost;user=mgs_user;database=filesearch;password=pa55word";`

### Database

Create a new database called 'filesearch' in phpMyAdmin and then run the [SQL script](database.sql) to create the tables and stored procedures used by the application. 

### NuGet Packages

[MySqlConnector](https://mysqlconnector.net/)

May also work with Oracle's version called MySql.Data on NuGet but has not been tested yet.

### Companion application

A [php application](https://github.com/DavidK1717/HD-file-search) that searches the database.