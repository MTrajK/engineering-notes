# Store Configuration and Secrets Locally
> How to store configuration and secrets locally when developing apps in Windows


## Core principles

1. **Never commit secrets**
    - keep ```.env*```, local secret stores, and IDE run configurations **out of source control** (```appsettings.*.json``` is not suggested to keep out of source control, but be sure you're not storing secrets there - or even worst, commiting secrets)
2. **Use “config layering”**
    - repo-safe defaults → developer-local overrides → secret store
3. **Separate “configuration” from “secrets”**
    - feature flags / URLs can live in env files
    - credentials/tokens belong in a secret store

## Configuration files

**Node.js (React, etc..):**
- .env
- .env.example
- .env.development
- .env.local
- .env.development.local

**.NET:**
- appsettings.json
- appsettings.Development.json
- appsettings.Staging.json
- appsettings.Production.json


**```.env*``` and ```appsettings.*.json``` are not the same type of files!**

### .env*

```.env``` configurations become environment variables (it's an environment variable definition file).\
```.env``` file is handled by [dotenv](https://www.npmjs.com/package/dotenv).\
Dotenv is a module that loads environment variables from a ```.env``` file into [process.env](https://nodejs.org/docs/latest/api/process.html#process_process_env). *In short: storing configuration in the environment separate from code*.\
Process.env is a built-in Node.js object that provides access to the **host machine's environment variables**.\
It does **NOT overwrite** existing environment variables:
- Environment variables from the OS
- Variables injected by the parent process (Docker, CI, shell, etc.)

**Note:** ```.env``` files are used also in **Python** (via python-dotenv) and **PHP** (via vlucas/phpdotenv).

### appsettings.*.json
But configurations from ```appsettings.json``` don't become environment variables (like they become environment variables in ```.env``` using dotenv). These configurations are loaded by the .NET configuration provider system and accessed through IConfiguration. *In short: the ```appsettings.json``` is an application configuration source*.\
These congiturations could be overwritten during the merge process (configuration layers merging) if variables with the same names were found from:
- User secrets
- Environment variables from the OS
- Command line


## dotnet user-secrets
> A .NET-specific development-time configuration provider.

Find more details [here](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0&tabs=windows).

Open terminal and init user secret in the root of the application (where ```Program.cs``` is located):
```powershell
cd {{ LOCAL REPOS FOLDER }}\{{ LOCAL ROOT PROJECT }}
dotnet user-secrets init
```

And use this command to set all the secrets:
```powershell
dotnet user-secrets set "Parameters:Name" "Value"
```

When the .NET app is started, these values will be available through IConfiguration (also these values will override the values provided through ```appsettings.json```).


## setx
> .NET and Node.JS development environment variables storage

```setx``` is usable, but it is not a best-practice mechanism for managing secrets (```setx``` is weak for secrets), especially beyond trivial local development.
- Writes the variable persistently into the user (or machine) environment in the Windows registry (It's persistent registry-backed variable).
- It is stored in plaintext in the registry.

Store
```powershell
setx MY_SECRET "value"
```

Read (note: needs to open a new terminal, so the new terminal will read from the registry)
```powershell
$env:MY_SECRET
```

### Temporary stored in the shell

Store
```powershell
$env:MY_VAR="my_value"
```

Read (available only in the same terminal where stored)
```powershell
$env:MY_VAR
```

Available only from the apps that are started in the same shell:
- ```dotnet run``` (accessed through IConfiguration)
- ```npm start``` (accessed through process.env)


## Secrets used from the systems and applications
> Not development related

### Windows Credential Manager
> Native store

See [Credential Manager in Windows](https://support.microsoft.com/en-us/windows/credential-manager-in-windows-1b5c916a-6a16-889f-8581-fc16e8165ac0).\
Windows Credential Manager is a built-in Windows feature that securely stores usernames, passwords, and certificates for websites, applications, and network resources. Accessible via the Control Panel, it allows users to view, add, or remove saved credentials to automate logins.

### PowerShell SecretManagement + SecretStore

In order to use SecretManagement, you'll need to install 2 powershell modules, open terminal and install these 2 modules:
```powershell
Install-Module Microsoft.PowerShell.SecretManagement
Install-Module Microsoft.PowerShell.SecretStore
```
Set a default secret vault:
```powershell
Register-SecretVault -Name SecretStore -ModuleName Microsoft.PowerShell.SecretStore -DefaultVault
```
Choose how to access the Secret store, with (first command) or without (second command) a password:
```powershell
Set-SecretStoreConfiguration -Authentication Password -Interaction Prompt
# OR
Set-SecretStoreConfiguration -Authentication None -Interaction None
```
Now you can store anything into SecretStore (which is encrypted locally).\
Example the GIT PAToken:
```powershell
Set-Secret -Name PAToken -Secret "REPLACEWITHTOKEN"
```
Get the token as plain text:
```powershell
Get-Secret -Name PAToken -AsPlainText
```
Example configure NuGet source using PAToken:
```powershell
dotnet nuget add source --username YOURUSERNAME --password (Get-Secret PAToken -AsPlainText) --store-password-in-clear-text --name github "https://nuget.pkg.github.com/SomeCompany/index.json"
```
