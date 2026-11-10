# RaceDay Web MVC Application

## Project Overview

RaceDay is an event management system developed using ASP.NET Core MVC. The application provides separate interfaces for Organisers and Participants, allowing users to manage sporting events, categories, enrolments and race results.

The MVC application communicates with the RaceDay RESTful API (Part 2) for all data operations.

## Technologies Used

- C#
- ASP.NET Core MVC (.NET 8)
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft SQL Server
- Azure Blob Storage
- Bootstrap
- Docker
- GitHub Actions

## Main Features

### Organiser
- View an organiser dashboard and event statistics.
- Create, view, update and delete events.
- Manage event categories.
- View participant enrolments.
- Record and update race results.

### Participant
- Register and log in securely.
- Browse available sporting events.
- View event details and categories.
- Enrol in events and cancel enrolments.
- View personal event enrolments.
- View race results.
- Update profile information.
- Upload and display a profile picture.

## Application Architecture

The project uses an MVC architecture:

- **Models:** Represent application data and view models.
- **Views:** Provide the user interface using Razor and Bootstrap.
- **Controllers:** Handle user requests and communicate with the REST API.
- **Services:** Handle HTTP communication between the MVC application and the API.

The MVC application does not connect directly to the SQL Server database. All data operations are handled through the RaceDay REST API.

## Azure Blob Storage

Azure Blob Storage is used to store participant profile pictures.

The MVC application uploads and retrieves profile pictures through the REST API. Azure connection details are securely configured in the API project and are not committed to this repository.

## Running the Application Locally

### Prerequisites

- Visual Studio 2022
- .NET 8 SDK
- SQL Server
- Docker Desktop (for container execution)
- RaceDay Part 2 REST API

### Step 1: Start the REST API

Run the RaceDay Part 2 API and ensure its database connection is configured correctly.

The API should be accessible at:

`http://localhost:5198`

### Step 2: Run the MVC Application

1. Open `RaceDay.Web` in Visual Studio 2022.
2. Ensure the development API URL is configured as `http://localhost:5198/`.
3. Build the solution.
4. Run the application.

The MVC application will communicate with the running REST API.

## Running with Docker

The project includes a Dockerfile for containerisation.

### Build the Docker image

```bash
docker build -t raceday-web .
```

### Run the Docker container

```bash
docker run -d --name raceday-web-container -p 8080:8080 raceday-web
```

Open the application in your browser:

`http://localhost:8080`

The RaceDay API must also be running and accessible to the Docker container.

## Continuous Integration

GitHub Actions is configured to automatically:

1. Check out the source code.
2. Install the .NET 8 SDK.
3. Restore project dependencies.
4. Build the MVC application in Release configuration.

The workflow runs when changes are pushed to the `main` branch or when a pull request targets `main`.
## Automated Testing

The MVC application includes an xUnit test project for validating profile input data.

The tests cover:
- Required profile fields.
- Invalid email addresses.
- Valid participant profile details.

To run the tests locally using Visual Studio or the .NET CLI:

```bash
dotnet test
## Repository

RaceDay Web MVC source code is maintained in this GitHub repository.

## Developer

Developed as part of the RaceDay event management system project.
## Security Features

The RaceDay MVC application uses the following security measures:

- Session-based authentication through the REST API.
- Role-based access for Organisers and Participants.
- Protected MVC routes using authorization filters.
- Secure session cookies configured as HTTP-only.
- API communication through a centralised service layer.
- Server-side validation of user-submitted form data.
- Profile picture uploads handled securely through the API and Azure Blob Storage.

The application separates Organiser and Participant functionality to prevent unauthorised access to role-specific features.
## System Architecture

RaceDay follows a client-server architecture.

1. **Presentation Layer:** ASP.NET Core MVC controllers and Razor views provide the user interface.
2. **Service Layer:** RaceDayApiService manages HTTP communication between the MVC application and the REST API.
3. **API Layer:** The RaceDay REST API processes requests, applies authorisation and validation, and performs data operations.
4. **Data Layer:** Entity Framework Core communicates with the SQL Server database.
5. **Cloud Storage:** Azure Blob Storage stores uploaded profile pictures.

The MVC application does not access the database directly. All application data is retrieved and updated through the REST API.
YouTube link: https://youtu.be/uW0PGflgjlM?si=g56loclsb0faTJTn
