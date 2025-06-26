# NppDB
This is a repository containing further implementation of NppDB for my bachelors thesis "Enhancement of a Plugin that Simplifies SQL Programming in the Notepad++ Source Code Editor". 
NppDB is a Notepad++ plugin originally developed by [Sangkyu Jung](https://github.com/gutkyu/NppDB) for supporting connection to different databases, execute SQL statements and show query results, 
and further developed by [Priit Post](https://github.com/pripost/NppDB) and [Andres Eelma](https://github.com/aneelm/NppDB).

## GUI Features
![structure image](https://raw.githubusercontent.com/gutkyu/NppDB/gh-pages/images/NppDB_All_n.png)
1. Database Connect Manager
    * Register, unregister, connect and disconnect databases
    * Represent the database objects in tree structure
    * Attach and detach to editor window for executing SQL statements
2. SQL Result
    * Show SQL execution messages and query results
3. Editor
    * Write SQL statements

## Currently Supported Database Management Systems
* MS Access
* PostgreSQL

## Requirements
   * Notepad++ 64-bit version (tested with 8.4.7 or 8.4.9)
   * .Net Framework 4.8
   * MS Access Database Engine 2010 Redistributable (driver) or MS Access Database Engine 2016 Redistributable (driver). The 64-bit driver version can be downloaded here: https://download.cnet.com/microsoft-access-database-engine-2010-redistributable-64-bit/3000-10254_4-75452796.html or here https://www.microsoft.com/en-us/download/details.aspx?id=54920&irgwc=1. If you also have MS Access installed on your computer, in the folder where you downloaded the driver, open the Windows Command Prompt with administrator rights and run the command: "accessdatabaseengine_X64 /quiet". (to use MS Access database)
   * ANTLR 4.11.1 or 4.13.1 (for development)

## Installation
Copy compiled .dll files from project folder or downloaded [.zip package](https://github.com/stenarm/NppDB/releases/tag/NppDB_26.06.2025) package as follows:
   * Place the file "NppDB.Comm.dll" in the root folder of Notepad++ program folder, where "notepad++.exe" is located.
   * Move the remaining .dll and translation.ini files to the "./plugins/NppDB" folder.
   * If you have get an error when starting up Notepad++ after installing the plugin, make sure all the copied .dll files are unblocked, by right-clicking on them and opening properties, and if possible checking box 'unblock'.
 
![Unblock](https://raw.githubusercontent.com/aneelm/NppDB/master/README_images/SecurityUnblock.jpg)

## Quick Start Guide
   1. Open "Database connect manager" (F10).
   2. Register MS Access or PostgreSQL database
         1. For MS Access register new or existing database (.accdb, .mdb) from local filesystem in the manager window, if there is no password, in the password window just click OK.
         2. For PostgreSQL register a new connection and fill out all the connection details.
   4. Expand the database tree to view database objects as you wish.
   5. Write some SQL statements in the editor.
   6. Execute written SQL statement(s) (F9). You can execute either the selected statement(s) or the statement on which the text cursor is located.
   7. Analyse your SQL statement for common mistakes (Shift+F9). Remove the messages when you want (Ctrl+Shift+F9).
   8. When a warning or an error is found, generate and copy an AI prompt to give to a language model of your choosing (Ctrl+F9).

## Usage
### Open Database Connect Manager
   Select 'NppDB/Database Connect Manager' from Notepad++ plugin menu
   or
   Click icon ![Database Connect Manager Icon](https://raw.githubusercontent.com/gutkyu/NppDB/gh-pages/images/DBPPManage16.png) from a toolbar 

### Register new database server
   1. Click icon ![Regiser Icon](https://raw.githubusercontent.com/gutkyu/NppDB/master/NppDB.Core/Resources/add16.png) from  Database-Connect-Manager's toolbar
   2. Select one of database types
   3. Database Connect Node is registered in Database Connect Manager by pass authentication which selected database module produce.
	![Select Database Type](https://raw.githubusercontent.com/aneelm/NppDB/master/README_images/database_system.png)
   4. Connect to database server
         * [MS Access](https://github.com/aneelm/NppDB.MSAccess) 
   	   * [PostgreSQL](https://github.com/aneelm/NppDB.PostgreSQL) 
      
### Getting into detail about sub elements
   Double-click on the nodes to expands sub elements in the database connection manager. If no connection has been made to the database, a connection window will open asking you to connect.

   * First, select 'Open' from database node's popup
   * Second, select prepared sql statements such as 'SELECT … Top 100' or 'SELECT *' or 'DROP' from table node's popup
	![SQL Linked Database Node](https://raw.githubusercontent.com/aneelm/NppDB/master/README_images/database_management.png)

### Executing sql statement
   1. Check that current document can execute sql statement. (ok if with sql-result )
   2. Write an sql statement and then select a block of the statement.
	![Select Blocks](https://raw.githubusercontent.com/gutkyu/NppDB/gh-pages/images/NppDB_SQL_Block.png)
   3. In NppDB plugin menu click 'Execute SQL (F9 shortcut key)' or use the F9 shortcut key to run the sql statement and display the results.
	
## License
MIT
