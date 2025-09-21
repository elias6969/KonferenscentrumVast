# KonferenscentrumVast

ASP.NET Core Web API for managing **customers**, **facilities**, **bookings**, and **contracts**.  
The system is built with Entity Framework Core and uses **PostgreSQL** as the database.

---

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download)  
- [PostgreSQL](https://www.postgresql.org/)  

---

## Setup & Running

1. Clone the repository:
```bash
git clone https://github.com/elias6969/KonferenscentrumVast.git
cd KonferenscentrumVast
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Apply migrations to your PostgreSQL database:
```bash
dotnet ef database update
```

4. Start the application:
```bash
dotnet run
```

5. Open Swagger UI in your browser at:
```
https://localhost:5001/swagger
```

---

## Usage Workflow (via Swagger)

When you open Swagger, you will see 4 controllers:

* **Customer**
* **Facility**
* **Booking**
* **BookingContract**

The typical flow to create a booking is:

### 1. Create a Customer

`POST /api/customer`

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@company.com",
  "phone": "123456789",
  "companyName": "ACME Inc.",
  "address": "Main Street 1",
  "postalCode": "12345",
  "city": "Stockholm"
}
```

---

### 2. Create a Facility

`POST /api/facility`

```json
{
  "name": "Conference Room A",
  "description": "Large hall with projector",
  "address": "Main Street 1",
  "postalCode": "12345",
  "city": "Stockholm",
  "maxCapacity": 200,
  "pricePerDay": 1000,
  "isActive": true
}
```

---

### 3. Create a Booking

`POST /api/booking`

```json
{
  "customerId": 1,
  "facilityId": 1,
  "startDate": "2025-09-21T09:00:00Z",
  "endDate": "2025-09-21T17:00:00Z",
  "numberOfParticipants": 50,
  "notes": "Board meeting"
}
```

**Example response:**

```json
{
  "id": 1,
  "customerId": 1,
  "facilityId": 1,
  "startDate": "2025-09-21T09:00:00Z",
  "endDate": "2025-09-21T17:00:00Z",
  "numberOfParticipants": 50,
  "status": "Pending",
  "totalPrice": 1000,
  "createdDate": "2025-09-21T13:41:05.623Z",
  "customerName": "John Doe",
  "customerEmail": "john@company.com",
  "facilityName": "Conference Room A",
  "contractId": null
}
```

---

### 4. (Optional) Create a Booking Contract

Contracts are optional and only needed if you want to define **special terms** or **adjust pricing**.

`POST /api/bookingcontract/booking/{bookingId}`

```json
{
  "terms": "Payment within 30 days",
  "totalAmount": 1200,
  "paymentDueDate": "2025-10-01T00:00:00Z"
}
```

**Example response:**

```json
{
  "id": 1,
  "bookingId": 1,
  "contractNumber": "C-2025-001",
  "status": "Draft",
  "terms": "Payment within 30 days",
  "totalAmount": 1200,
  "currency": "SEK",
  "paymentDueDate": "2025-10-01T00:00:00Z",
  "customerName": "John Doe",
  "facilityName": "Conference Room A",
  "createdDate": "2025-09-21T13:42:00Z"
}
```

---

## Database Schema

PostgreSQL tables created by EF Core migrations:

* `Customers`
* `Facilities`
* `Bookings`
* `BookingContracts`
* `__EFMigrationsHistory`

---

## Adjustments Made
The original code functioned correctly but required minor adjustments to improve clarity and maintainability.
The following changes were made:

* **Renamed generic methods** for clarity:

  * `GetAll()` -> `GetAllContracts`, `GetAllBookings`, `GetAllCustomers`, `GetAllFacilities`
  * `GetById()` -> `GetContractById`, `GetBookingById`, `GetCustomerById`, `GetFacilityById`
  * `Patch()` -> `PatchContract`
* **Standardized naming conventions** across all controllers, so endpoints are explicit and self-explanatory.
* **Added this README** with usage workflow and database overview for better documentation.

### Motivation
These adjustments make the API easier to understand and use, reduce ambiguity, and improve overall developer experience.
Explicit method names and consistent error handling are small changes, but they significantly increase clarity and maintainability.

---

## Notes

* The project uses PostgreSQL, ensure your connection string is set correctly in `appsettings.json`.
* All migrations are included and can be applied with `dotnet ef database update`.
