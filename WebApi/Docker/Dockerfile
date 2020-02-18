# Install base image
FROM microsoft/mssql-server-linux

# Copy over the SQL file to set up the database
COPY setup.sql /

# Copy over the file with sample data. 
# This data is from https://people.sc.fsu.edu/~jburkardt/data/csv/csv.html
COPY data.csv /

# Temporary SA password to do database setup (Note - change this!)
ENV MSSQL_SA_PASSWORD=Passw@rd!
ENV ACCEPT_EULA=Y

# Set up the database and populate with sample data
RUN ( /opt/mssql/bin/sqlservr --accept-eula & ) | grep -q "Service Broker manager has started" \
    && /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'Passw@rd!' -i /setup.sql