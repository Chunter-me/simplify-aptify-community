// vim:syntax=powershell

# Based on: https://github.com/Microsoft/mssql-docker/blob/master/windows/mssql-server-windows-developer/dockerfile

# Chunter Local SQL Server with Full-Text

# docker build -t chunter/sql-server C:\Chunter\Chunter.me\AptifyDocker\DockerFiles\SQLServer
# docker run --mount type=bind,source=C:\Chunter\Chunter.me\AptifyDocker\Data\,destination=c:\Data\ -d -p 8010:1433 --name chunter-sql-server chunter/sql-server
# increase container disk size: --storage-opt size=###G

FROM mcr.microsoft.com/windows/servercore:ltsc2019

LABEL maintainer "Christopher Hunter"

# Set working directory
WORKDIR /

# Download Links:
ENV exe "https://go.microsoft.com/fwlink/?linkid=840945"
ENV box "https://go.microsoft.com/fwlink/?linkid=840944"

# Varialbes
ENV sa_password="_" \
    attach_dbs=''

# ADD files into container
ADD ./start.ps1 /

# Switech to Powershell
SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Dowload SQL Installers
RUN Invoke-WebRequest -Uri $env:box -OutFile SQL.box
RUN Invoke-WebRequest -Uri $env:exe -OutFile SQL.exe

# Install Sql Server
RUN Start-Process -Wait -FilePath .\SQL.exe -ArgumentList /qs, /x:setup
RUN .\setup\setup.exe /q /ACTION=Install /INSTANCENAME=MSSQLSERVER /FEATURES=SQLEngine, FullText /UPDATEENABLED=0 /SQLSVCACCOUNT='NT AUTHORITY\System' /SQLSYSADMINACCOUNTS='BUILTIN\ADMINISTRATORS' /TCPENABLED=1 /NPENABLED=0 /IACCEPTSQLSERVERLICENSETERMS
RUN Remove-Item -Recurse -Force SQL.exe, SQL.box, setup

# Stop and Set Environment Varialbes
RUN stop-service MSSQLSERVER
RUN set-itemproperty -path 'HKLM:/software/microsoft/microsoft sql server/mssql14.MSSQLSERVER/mssqlserver/supersocketnetlib/tcp/ipall' -name tcpdynamicports -value ''
RUN set-itemproperty -path 'HKLM:/software/microsoft/microsoft sql server/mssql14.MSSQLSERVER/mssqlserver/supersocketnetlib/tcp/ipall' -name tcpport -value 1433
RUN set-itemproperty -path 'HKLM:/software/microsoft/microsoft sql server/mssql14.MSSQLSERVER/mssqlserver/' -name LoginMode -value 2

# Health Check
HEALTHCHECK CMD [ "sqlcmd", "-Q", "Select 1" ]

# Startup Command
CMD .\start -sa_password $env:sa_password -ACCEPT_EULA 'Y' -attach_dbs \"$env:attach_dbs\" -Verbose