## User Secrets (Required)

This project uses .NET User Secrets.

Before running the API, import the provided secrets:

```powershell
cd AppWebApi
dotnet user-secrets clear
dotnet user-secrets set --id 4750e777-b3e2-46ca-be40-98f6be82577c --file user-secrets.json
