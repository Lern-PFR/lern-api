# Lern. - API

[![Docker Hub](https://img.shields.io/badge/docker-ready-blue.svg)](https://hub.docker.com/r/lernpfr/lern-api) ![.NET Core](https://github.com/Lern-PFR/lern-api/workflows/.NET%20Core/badge.svg) ![Docker Image](https://github.com/Lern-PFR/lern-api/workflows/Docker%20Image/badge.svg)

Ce dépôt contient le code source de l'API du projet [Lern.](https://github.com/Lern-PFR/lern-production)

## Utilisation

> Ce document détaille l'utilisation de l'API seule, sans client associé  
> c.f. [lern-production](https://github.com/Lern-PFR/lern-production) pour utiliser l'image complète de production

### Image Docker officielle

```bash
docker run -p 80:80 -p 443:443 lernpfr/lern-api -e DB_HOST=127.0.0.1 -e DB_USERNAME=username -e DB_PASSWORD=password -e DB_DATABASE=database -e DB_PORT=5432 -e SECRET_KEY=random_cryptographic_key
```

Pour plus de détails sur les variables d'environnement utilisables, des explications complètes sont disponibles sur [Docker Hub](https://hub.docker.com/r/lernpfr/lern-api).

### Depuis le code source

#### Avec docker-compose (recommandé)

```bash
docker-compose up
```

L'API est disponible après initialisation à l'adresse [](http://localhost:81/)

#### Avec l'image Docker

```bash
docker build . -t lern/lern-api:custom
docker run -p 80:80 -p 443:443 lern/lern-api:custom -e DB_HOST=127.0.0.1 -e DB_USERNAME=username -e DB_PASSWORD=password -e DB_DATABASE=database -e DB_PORT=5432 -e SECRET_KEY=random_cryptographic_key
```

Pour plus de détails sur les variables d'environnement utilisables, des explications complètes sont disponibles sur [Docker Hub](https://hub.docker.com/r/lernpfr/lern-api).

#### Dans un environnement .NET Core

```bash
dotnet test
dotnet run --project Lern-API/Lern-API.csproj
```

Le fichier de configuration se trouve dans `Lern-API/appsettings.json`.
