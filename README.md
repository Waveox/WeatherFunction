# Weather-Function
This repository contains a description and implementation of the programming task used to evaluate software developers.

## Task 1

Must use:
* Azure Function (Cloud/Local)
* Azure Storage (Cloud /Local storage emulator)
* a. Table
* b. Blob
* .Net Core 6.

Achieve:
* Every minute, fetch data from
https://api.openweathermap.org/data/2.5/weather?q=London&amp;appid=YOUR_API_KEY (you
need to sign up to generate a new API Key) and store success/failure attempt log in the table
and full payload in the blob.
* Create a GET API call to list all logs for the specific time period (from/to) (implement an
Azure Function HTTP Trigger that queries the Azure Table Storage and returns logs within
the specified time period).
* Create a GET API call to fetch a payload from blob for the specific log entry (implement an
Azure Function HTTP Trigger that retrieves the specific payload from Azure Blob Storage
based on the log entry ID).

## Running API locally

This project uses Azurite as Azure storage emulator.

To run and test API locally use:

```
docker compose up --build -d
```

Make calls to enpoints with:

```
GET localhost:8080/api/logs?from={fromTime}&to={toTime}
Example: localhost:8080/api/logs?from=2023-01-01T00:00:00Z&to=2024-10-01T13:44:00Z
```

Use RowKey from api/logs.
```
GET localhost:8080/api/payload/{RowKey}
Example: localhost:8080/api/payload/f0fb0b93-3f66-4c0f-b52e-588c81727274
```

Have fun!
