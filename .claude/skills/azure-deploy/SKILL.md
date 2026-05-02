---
name: azure-deploy
description: Deploy the .NET 9 banking API to Azure App Service
disable-model-invocation: true
allowed-tools:
  - Bash(dotnet *)
  - Bash(az *)
---

Deploy the .NET 9 banking API to Azure:

1. Build in Release mode: `dotnet build -c Release`
2. Publish: `dotnet publish -c Release -o ./publish`
3. Zip for deployment: `zip -r publish.zip ./publish`
4. Deploy to App Service: `az webapp deploy --resource-group <rg> --name <app-name> --src-path publish.zip`
5. Check deployment: `az webapp show --resource-group <rg> --name <app-name> --query state`

Ask the user for resource group and app name if not provided.
