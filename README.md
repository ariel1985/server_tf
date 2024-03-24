# Server TF

## Description

This project is a translation management API built with .NET Core. 
It allows users to create and manage different applications, each with its own set of translations. Each application is represented by a JSON file in the `translations` directory. 

The API provides endpoints to list all applications and to create a new application. When a new application is created, a JSON file is generated with a set of dummy translations. This project is ideal for managing translations for multiple applications in a centralized manner.

## Installation

1. Clone the repository: `git clone https://github.com/ariel1985/repo.git`
2. Navigate to the project directory: `cd repo`
3. Install the dependencies: `dotnet restore`

## Usage

1. Build the project: `dotnet build`
2. Run the project: `dotnet run`
3. Open your browser and navigate to `http://localhost:5000`

## API Endpoints

- `GET /api/apps`: Returns a list of all applications. Each item in the list is the name of a JSON file (without the .json extension) in the `translations` directory.

- `POST /api/apps`: Creates a new application. The request body should be a string representing the name of the new application. This endpoint creates a new JSON file in the `translations` directory with the provided name. If a file with the same name already exists, the endpoint returns a conflict response.


