# NppDB
This is a repository containing further implementation of NppDB for my master thesis "Creating a Plugin for Source Code Editor Notepad++ that Simplifies SQL
Programming in MS Access Databases". NppDB is a Notepad++ plugin originally developed by [Sangkyu Jung](https://github.com/gutkyu/NppDB) for supporting connection to different databases, execute SQL statements and show query results.

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

## Currently Supported Databases
MS Access

## Requirements
   * Notepad++ 64-bit version (tested with 8.4.7 or 8.4.9)
   * .Net Framework 4.8
   * MS Access Database Engine 2010 Redistributable (driver). The 64-bit driver version can be downloaded here: https://www.microsoft.com/en-us/download/details.aspx?id=13255. If you also have MS Access installed on your computer, in the folder where you downloaded the driver, open the Windows Command Prompt with administrator rights and run the command: "accessdatabaseengine_X64 /quiet".
   * ANTLR 4.11.1 (for development)

## Installation
Copy compiled .dll files from project folder or downloaded [.zip package](https://github.com/pripost/NppDB/releases/download/v0.9.0/NppDB.zip) package as follows:
   * Place the file "NppDB.Comm.dll" in the root folder of Notepad++ program folder, where "notepad++.exe" is located.
   * Move the remaining two .dll files to the "./plugins/NppDB" folder.

## Quick Start Guide
   1. Open "Database connect manager" (F10).
   2. Register new or existing database (.accdb, .mdb) from local filesystem in the manager window.
   3. Right mouse click and select "Connect" or double click on the database name where to connect the active editor to. If there is a password window, enter the password, or if there is none, just "OK".
   4. Expand the database tree to view database objects as you wish.
   5. Write some SQL statements in the editor.
   6. Execute written SQL statement(s) (F9). You can execute either the selected statement(s) or the statement on which the text cursor is located.

## Usage
### Open Database Connect Manager
   select 'NppDB/Database Connect Manager' from Notepad++ plugin menu
   or
   click icon ![Database Connect Manager Icon](https://raw.githubusercontent.com/gutkyu/NppDB/gh-pages/images/DBPPManage16.png) from a toolbar 

### Register new database server
   1. click icon ![Regiser Icon](https://raw.githubusercontent.com/gutkyu/NppDB/master/NppDB.Core/Resources/add16.png) from  Database-Connect-Manager's toolbar
   2. select one of database types
   3. Database Connect Node is registered in Database Connect Manager by pass authentication which selected database module produce.
	![Select Database Type](https://raw.githubusercontent.com/gutkyu/NppDB/gh-pages/images/NppDB_Sel_DBType.png)
   4. connect to database server
   	* [MS SQL Server](https://github.com/gutkyu/NppDB.MSSQL) 
      
### Getting into detail about sub elements
   perform double-click on the node to expands sub elements.
   because all of connect database manager's nodes are represented in hierarchy, can also use this way for other sub elements
   two method to make a environment to execute sql statements
   * first, select 'Open' from database node's popup
   * second, select prepared sql statements as 'Select … Top 100' or 'Select … Limit 100' from table node's popup
	![SQL Linked Database Node](https://raw.githubusercontent.com/gutkyu/NppDB/gh-pages/images/NppDB_Node_SQL.png)

### Executing sql statement
   1. check that current document can execute sql statement. (ok if with sql-result )
   2. write a sql statement and then select a block of the statement.
	![Select Blocks](https://raw.githubusercontent.com/gutkyu/NppDB/gh-pages/images/NppDB_SQL_Block.png)
   3. perform menu 'Execute SQL (F9 shortcut key)' to display result of the sql statement.
	
## License
MIT
