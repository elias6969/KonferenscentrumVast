# KonferenscentrumVäst

ASP.NET Core Web API for managing customers, facilities, bookings, contracts, and file uploads.
The project is containerized with Docker and deployed to Google Cloud Run, using Cloud SQL (PostgreSQL) for persistence and Cloud Storage for secure file management.
Sensitive configuration values such as database connection strings, JWT keys, and storage bucket names are managed via Google Secret Manager.

## Requirements

* .NET 8 SDK
* Docker
* Google Cloud CLI
* PostgreSQL
 (for local development)

### Local Development Setup

Clone the repository:
```bash
git clone https://github.com/elias6969/KonferenscentrumVast.git
cd KonferenscentrumVast
```

Restore dependencies:
```bash
dotnet restore
```

Apply EF Core migrations:
```bash
dotnet ef database update
```

Start the API:
```bash
dotnet run
```

Open Swagger UI:
```bash
https://localhost:5001/swagger
```

## Running with Docker

**Build the Docker image:**
```bash
docker build -t konferenscentrumvast:latest .
```

**Run the container locally:**
```bash
docker run -p 8080:8080 konferenscentrumvast:latest
```

Access the API at:
```bash
http://localhost:8080/swagger
```
Google Cloud Deployment
**1. Configure environment**

Make sure your Google Cloud project is active and authenticated:
```bash
gcloud auth login
gcloud config set project <your-project-id>
```

2. Build and push image to Artifact Registry
```bash
docker build -t europe-north1-docker.pkg.dev/<project-id>/<repo-name>/konferenscentrumvast:latest .
docker push europe-north1-docker.pkg.dev/<project-id>/<repo-name>/konferenscentrumvast:latest
```

3. Deploy to Cloud Run
```bash
gcloud run deploy konferenscentrumvast \
  --image europe-north1-docker.pkg.dev/<project-id>/<repo-name>/konferenscentrumvast:latest \
  --region europe-north1 \
  --platform managed \
  --allow-unauthenticated
```

4. Configure secrets
```bash
echo -n "your-db-connection-string" | gcloud secrets create db-connection-string --data-file=-
echo -n "your-jwt-key" | gcloud secrets create jwt-key --data-file=-
echo -n "your-bucket-name" | gcloud secrets create file-bucket-name --data-file=-
```

Secrets are automatically retrieved at runtime via the Secret Manager API in Program.cs.

## File Handling

Uploaded files (contracts and facility images) are stored in Google Cloud Storage, while metadata such as file name, type, uploader, and linked entities are saved in Cloud SQL.

**Endpoints**
Endpoint	Method	Description
**POST /api/file/upload**
Uploads a file to Cloud Storage and saves metadata in Cloud SQL

**GET /api/file/{id}**	
Returns a signed URL for secure file download

**GET /api/file/booking/{bookingId}**
Lists files associated with a booking

**GET /api/file/facility/{facilityId}**
Lists files associated with a facility

**DELETE /api/file/{id}**
Deletes a file from both storage and database

All file routes are protected with JWT authentication and use signed URLs for secure, time-limited downloads.

## Usage Workflow

Typical flow through the API (via Swagger):

Create a Customer -> Create a Facility -> Create a Booking

(Optional) Create a Booking Contract

Upload related files for contracts or facilities
Each step can be performed directly through Swagger or Postman.

## Security

* JWT authentication is required for all protected endpoints

* Sensitive data (DB connection, JWT, bucket name) is stored securely in Secret Manager

* No secrets are stored in appsettings or code

* Signed URLs limit external access to files, supporting GDPR compliance

## Monitoring and Scaling

**The API is deployed on Google Cloud Run, which provides:**

* Automatic horizontal scaling based on traffic

* Built-in health checks and version rollbacks

* Centralized logging via Cloud Logging

* Metrics and performance tracking via Cloud Monitoring

## Database Schema
PostgreSQL tables created by EF Core migrations:

* Customers
* Facilities
* Bookings=
* BookingContracts
* Files
* __EFMigrationsHistory

### Notes

The project runs locally and in the cloud without code changes.

Ensure GOOGLE_APPLICATION_CREDENTIALS is configured when running locally with Google Cloud services.

EF migrations apply automatically on Cloud Run startup
