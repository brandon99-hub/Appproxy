# Kianda Admissions Proxy (formerly BI OData Proxy)

## Overview
This project is an ASP.NET Core 8 Web API designed to act as a secure backend-for-frontend (BFF) proxy between a web-based admissions form and a Business Central (BC) on-premises instance.

Originally designed as a generic BI OData Proxy for Power BI, the project has been pivoted and tailored specifically to handle student admission applications for Kianda School. 

It solves a critical security and integration challenge: 
- **Security:** External web clients cannot authenticate directly with the on-premise Business Central server via Windows Authentication.
- **Complexity:** A single admission application requires writing to multiple interconnected tables in Business Central (Admissions, Schools, Relatives).

## How It Works
1. **Authentication (API Key):** The proxy accepts a JSON payload from the frontend, secured by an `X-Api-Key` header.
2. **Translation & Orchestration:** The proxy breaks down the single, complex JSON payload into multiple, flat Business Central (BC) entity models.
3. **Delegation (Windows Auth):** The proxy uses its own underlying Windows service identity (`UseDefaultCredentials = true`) to authenticate transparently with Business Central and executes the necessary HTTP POST requests to the BC OData V4 endpoints.

---

## Project Structure

```text
APP Proxy/
├── Controllers/
│   └── AdmissionsController.cs    # Orchestrates the incoming admission payload to BC
├── Middleware/
│   └── ApiKeyMiddleware.cs        # Validates the X-Api-Key header
├── Services/
│   └── BCProxyService.cs          # Executes HTTP POST requests to BC OData endpoints
├── Models/
│   ├── AdmissionsPayload.cs       # Defines the JSON structure expected from the frontend
│   ├── BCModels.cs                # Defines the JSON structure required by Business Central
│   └── ProxySettings.cs           # Configuration mapping models
├── Program.cs                     # Application entry point & DI configuration
├── appsettings.json               # Application configuration (Keys, URLs)
└── BIProxy.csproj                 # Project file
```

## API Endpoints

### `POST /api/Admissions`
Accepts a student admission application and posts it to Business Central.

**Headers Required:**
- `X-Api-Key`: The API key matching the `ProxySettings:ApiKey` in `appsettings.json`.
- `Content-Type`: `application/json`

**Body Example:**
```json
{
  "candidate": {
    "fullName": "Jane Doe",
    "dob": "2010-05-15",
    "religion": "Christian",
    "denomination": "Catholic",
    "birthOrder": "1st",
    "medicalInfo": "None"
  },
  "parentDetails": {
    "residency": "Nairobi",
    "houseTelephoneNo": "020123456",
    "houseNo": 12
  },
  "additionalInfo": {
    "source": "Website",
    "hasAppliedBefore": false,
    "previousApplicationYears": []
  },
  "schoolsAttended": [
    {
      "schoolName": "Primary School Name",
      "yearsRange": "2018-2023"
    }
  ],
  "siblings": [
    {
      "name": "John Doe",
      "relationship": "Brother",
      "schoolName": "Another School",
      "dob": "2012-08-20"
    }
  ]
}
```

**Success Response:**
```json
{
  "success": true,
  "admissionNo": "ADM-00123"
}
```

---

## Business Central Integration

The proxy interacts with the following Business Central OData V4 endpoints relative to the `BCSettings:BaseUrl` configured in `appsettings.json`:

1. `/Admissions`: Creates the primary admission record.
2. `/AppSchools`: Creates records for schools previously attended.
3. `/AppRelatives`: Creates records for siblings (brothers/sisters).
4. `/AppRelations`: Creates records for non-sibling relations.
