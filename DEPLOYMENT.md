# RaceDay MVC Deployment Guide

## Requirements
- Docker Desktop
- .NET 8
- RaceDay REST API
- SQL Server database configured for the API

## Build the Docker Image

Run this command from the RaceDay.Web project directory:

```bash
docker build -t raceday-web .

