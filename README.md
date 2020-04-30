# Lern. - API

![.NET Core](https://github.com/Lern-PFR/lern-api/workflows/.NET%20Core/badge.svg) ![Docker Image](https://github.com/Lern-PFR/lern-api/workflows/Docker%20Image/badge.svg)

Ce dépôt contient le code source de l'API du projet [Lern.](https://github.com/Lern-PFR/lern-production)

## Utilisation

> Ce document détaille l'utilisation de l'API seule, sans client associé  
> c.f. [lern-production](https://github.com/Lern-PFR/lern-production) pour utiliser l'image complète de production

### Avec l'image Docker (recommandé)

```bash
docker build . -t lern/lern-api
docker run -p 80:80 -p 443:443 lern/lern-api
```

### Dans un environnement .NET Core

```bash
dotnet test
dotnet run --project Lern-API/Lern-API.csproj
```
