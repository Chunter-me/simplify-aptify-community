# 1 - Build Image

```powershell
docker build -t <AccountName>:latest D:\Repos\simplify-aptify\docker\mssql-<linux|windows>
```

# 2 - Create Container

```powershell
# Linux
docker run --name {CONTAINERNAME} --memory=2500m -e "ACCEPT_EULA=Y" -e "SA_PASSWORD={SAPASSWORDGOESHERE}" -e "MSSQL_AGENT_ENABLED=true" -p {PORTNUMBER}:1433 -v {WINDOWSFOLDERPATH}:/var/opt/mssql/data -d <AccountName>:latest

# Windows
docker run --name {CONTAINERNAME} --env sa_password={SAPASSWORDGOESHERE} -p 8010:1433 --mount type=bind,source={WINDOWSFOLDERPATH},destination=c:\Data\ -d <AccountName>:latest
```

# 3 - Setup Database

- Put your back or DB files in your Volume folder your specified in the container create command.
- Open favorite SQL client and restore/attach your database

# 3 - Set Aptify Enable CLR and Trustworthy

```sql
USE APTIFY;

EXEC sp_configure 'clr enabled', 1;
RECONFIGURE;

EXEC sp_configure 'clr enabled';   -- make sure it took
EXEC sp_changedbowner 'sa';
RECONFIGURE;

EXEC sp_configure 'show advanced options', 1
RECONFIGURE;

EXEC sp_configure 'clr strict security', 0;
RECONFIGURE;

ALTER DATABASE APTIFY SET TRUSTWORTHY ON;
```