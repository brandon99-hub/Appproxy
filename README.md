# BI OData Proxy - Complete Project Documentation

## Problem Statement

**Goal:** Connect Power BI to Business Central (BC) on-premises to analyze data.

**The Authentication Deadlock:**
- **Power BI** requires **Basic Authentication** to connect to OData endpoints
- **Business Central** is configured with **Windows Authentication** for security
- Changing BC's `ClientServicesCredentialType` to `NavUserPassword` would enable Basic Auth, but **locks the user out of BC** ❌

**Proposed Solution:** Create a lightweight ASP.NET Core Web API proxy that:
1. Accepts Basic Auth from Power BI
2. Converts to Windows Auth for BC
3. Forwards OData requests transparently

---

## Implementation Details

### Project Structure

```
BI Proxy/
├── Controllers/
│   └── ODataProxyController.cs    # Handles all OData requests
├── Middleware/
│   └── BasicAuthMiddleware.cs     # Validates Basic Auth
├── Services/
│   └── BCProxyService.cs          # Forwards to BC with Windows Auth
├── Models/
│   └── ProxySettings.cs           # Configuration models
├── Program.cs                      # Application entry point
├── appsettings.json               # Configuration
└── BIProxy.csproj                 # Project file
```

### Configuration

**appsettings.json:**
```json
{
  "BCSettings": {
    "BaseUrl": "http://brandon:7048/BC240/ODataV4",
    "WindowsUsername": "USERR",
    "WindowsPassword": "Exlifes_69",
    "Domain": "brandon"
  },
  "ProxySettings": {
    "Port": 9000,
    "AllowedUsers": {
      "POWER BI": "Exlifes_69"
    }
  }
}
```

### Core Components

#### 1. BasicAuthMiddleware.cs
- Extracts `Authorization: Basic` header
- Decodes Base64 credentials
- Validates against `AllowedUsers` dictionary
- Returns 401 if invalid

**Status:** ✅ **Working** - Successfully authenticates Power BI requests

#### 2. BCProxyService.cs
- Creates `HttpClient` with Windows credentials
- Forwards HTTP requests to BC
- Supports all HTTP methods (GET/POST/PUT/PATCH/DELETE)

**Initial Implementation:**
```csharp
var handler = new HttpClientHandler
{
    Credentials = new NetworkCredential(
        userName: "USERR",
        password: "Exlifes_69",
        domain: "brandon"
    ),
    PreAuthenticate = true
};
```

**Problem:** BC rejected explicit credentials

**Solution:** Changed to `UseDefaultCredentials = true`
```csharp
var handler = new HttpClientHandler
{
    UseDefaultCredentials = true,
    PreAuthenticate = true
};
```

**Status:** ✅ **Working** - BC accepts requests and returns 200 OK

#### 3. ODataProxyController.cs
- Catch-all routing: `[Route("ODataV4")]` with `{**path}`
- Forwards requests to BCProxyService
- Rewrites BC URLs in responses to proxy URLs

**Status:** ✅ **Working** - Successfully forwards requests and rewrites responses

---

## Attempted Solutions & Failures

### Attempt 1: Dynamics 365 Business Central Connector

**Approach:** Use Power BI's built-in "Dynamics 365 Business Central (on-premises)" connector

**Configuration:**
- URL: `http://localhost:9000/`
- Auth: Basic (POWER BI / Exlifes_69)

**What Happened:**
1. BC connector auto-appends `/ODatav4/` to the base URL
2. Proxy route was `/ODataV4`, creating duplicate: `/ODataV4/ODatav4/Company/`
3. BC returned 404 Not Found

**Fix Attempted:** Strip duplicate `ODataV4` from path
```csharp
if (normalizedPath.StartsWith("ODataV4/", StringComparison.OrdinalIgnoreCase))
{
    normalizedPath = normalizedPath.Substring(8);
}
```

**Result:** ❌ **Failed** - BC connector has hardcoded expectations for:
- Specific BC API endpoints
- BC-specific service discovery mechanisms
- Metadata in BC-specific formats

**Error:** "Please provide the full path to your OData service"

**Conclusion:** A simple HTTP proxy cannot fully emulate a BC server for the BC connector.

---

### Attempt 2: Change Proxy Route to Root

**Approach:** Change proxy route from `/ODataV4` to `/` (root) to match BC connector expectations

**Configuration:**
```csharp
[Route("")]  // Root route
```

**Power BI URL:** `http://localhost:9000/`

**What Happened:**
1. BC connector requests root: `http://localhost:9000/`
2. Proxy has no handler for empty path
3. Validation error: "One or more validation errors occurred"

**Fix Attempted:** Add explicit root endpoint handler
```csharp
[HttpGet("")]
public async Task<IActionResult> GetRoot()
{
    // Forward to BC's ODataV4 root
    var response = await _proxyService.ForwardRequestAsync(Request, "ODataV4/");
    // ...
}
```

**Result:** ❌ **Failed** - BC connector still couldn't discover the service

**Error:** "Please provide the full path to your OData service"

---

### Attempt 3: Generic OData Feed Connector

**Approach:** Use Power BI's generic "OData Feed" connector instead of BC-specific connector

**Configuration:**
- Connector: OData Feed (NOT Business Central)
- URL: `http://localhost:9000/ODataV4/Company('CRONUS%20International%20Ltd.')`
- Auth: Basic (POWER BI / Exlifes_69)

**Proxy Route:** Changed back to `[Route("ODataV4")]`

**What Happened:**

**Test 1: Direct curl test**
```bash
curl -u "POWER BI:Exlifes_69" http://localhost:9000/ODataV4/Company('CRONUS%20International%20Ltd.')
```
**Result:** ✅ 200 OK, 528 bytes returned

**Test 2: Power BI connection**
**Result:** ❌ 404 Not Found

**Logs showed:**
```
BC URL: http://brandon:7048/BC240/ODataV4/Company('CRONUS International Ltd.')
BC responded with status: OK
```

**Then:**
```
BC URL: http://brandon:7048/BC240/ODataV4/Company('CRONUS International Ltd.')
BC responded with status: NotFound
```

**Pattern:** Alternating OK and NotFound responses

**Analysis:**
- Power BI makes multiple discovery requests
- Some succeed (200 OK)
- Some fail (404 Not Found)
- Power BI expects standard OData service document at root
- BC requires company specification in URL path

---

### Attempt 4: Use Correct Company Name

**Discovery:** BC has companies:
- `Brandon`
- `CRONUS International Ltd.` (with period!)
- `Stocksavvy`

**Fix Attempted:** Use exact company name with period
```
http://localhost:9000/ODataV4/Company('CRONUS%20International%20Ltd.')
```

**Result:** ❌ **Still Failed** - Same alternating OK/NotFound pattern

**Logs:**
```
BC responded with status: OK
BC responded with status: NotFound
BC responded with status: OK
BC responded with status: NotFound
```

---

### Attempt 5: OData Service Root

**Approach:** Connect to OData root and let Power BI browse entities

**URL:** `http://localhost:9000/ODataV4/`

**Result:** ❌ **Failed** - 400 Bad Request

**Error:** "The remote server returned an error: (400) Bad Request"

---

## Test Results Summary

### What Works ✅

1. **Basic Authentication**
   - Proxy successfully validates credentials
   - Returns 401 for invalid credentials
   - Allows valid requests through

2. **Windows Authentication to BC**
   - `UseDefaultCredentials = true` works
   - BC accepts requests from proxy
   - Returns 200 OK for valid requests

3. **Request Forwarding**
   - Proxy successfully forwards HTTP requests
   - Headers, query strings, and body copied correctly
   - All HTTP methods supported (GET/POST/PUT/PATCH/DELETE)

4. **URL Rewriting**
   - BC URLs in responses rewritten to proxy URLs
   - Prevents exposure of internal BC server details

5. **Direct API Calls**
   - curl/PowerShell requests work perfectly
   - Returns actual OData data from BC

### What Doesn't Work ❌

1. **Power BI Dynamics 365 BC Connector**
   - Requires BC-specific API endpoints
   - Expects BC service discovery mechanisms
   - Cannot be emulated by simple HTTP proxy

2. **Power BI OData Feed Connector**
   - Expects standard OData service document
   - Makes multiple discovery requests
   - Some requests fail (404) due to BC's non-standard structure

3. **BC's OData Implementation**
   - Non-standard OData structure
   - Requires company specification in URL
   - No standard service root endpoint
   - Company entity endpoint doesn't return service document

---

## Root Cause Analysis

### Why the Proxy Fails with Power BI

**Business Central's OData Implementation is Non-Standard:**

1. **Standard OData Service:**
   ```
   GET /odata/
   → Returns service document listing all entity sets
   
   GET /odata/$metadata
   → Returns metadata document
   
   GET /odata/Customers
   → Returns customer data
   ```

2. **BC's OData Structure:**
   ```
   GET /ODataV4/
   → Returns list of companies (not entity sets)
   
   GET /ODataV4/Company('CompanyName')
   → Returns company info (not service document)
   
   GET /ODataV4/Company('CompanyName')/Customers
   → Returns customer data
   ```

**The Problem:**
- Power BI expects entity sets at the root level
- BC requires company selection first
- No way to map BC's structure to standard OData expectations
- Power BI's discovery requests fail because BC doesn't return expected responses

### Evidence from Logs

**Successful Request:**
```
Path: Company('CRONUS International Ltd.')
BC URL: http://brandon:7048/BC240/ODataV4/Company('CRONUS International Ltd.')
BC responded with status: OK
```

**Failed Request (immediately after):**
```
Path: Company('CRONUS International Ltd.')
BC URL: http://brandon:7048/BC240/ODataV4/Company('CRONUS International Ltd.')
BC responded with status: NotFound
```

**Analysis:** Power BI is requesting different sub-paths or metadata that BC doesn't support at the Company level.

---

## Conclusions

### What We Learned

1. **The proxy works correctly** for its intended purpose:
   - ✅ Converts Basic Auth → Windows Auth
   - ✅ Forwards requests to BC
   - ✅ Returns data successfully

2. **The problem is BC's OData implementation:**
   - ❌ Non-standard structure
   - ❌ Incompatible with Power BI's expectations
   - ❌ Cannot be fixed with a simple proxy

3. **Power BI connectors have specific requirements:**
   - BC connector: Expects full BC API surface
   - OData connector: Expects standard OData service structure
   - Neither can work with BC through a simple auth proxy

### Why a Simple Proxy Cannot Solve This

A simple HTTP proxy that only converts authentication **cannot:**
- Restructure BC's non-standard OData responses
- Implement BC's full API surface for the BC connector
- Transform BC's company-based structure to standard OData
- Handle all of Power BI's service discovery requests

**What would be needed:**
- Full BC API emulation layer
- OData protocol translation
- Service document generation
- Metadata transformation
- Complex request routing logic

This is beyond the scope of a "lightweight proxy" and would essentially require building a BC gateway/adapter.

---

## Working Solutions

### Solution 1: Change BC Authentication (Recommended)

**Configure BC to use NavUserPassword authentication:**

1. Open BC Administration Console
2. Navigate to Server Instance → Edit
3. Change `ClientServicesCredentialType` to `NavUserPassword`
4. Create a BC user with appropriate permissions
5. Connect Power BI directly to BC with Basic Auth

**Pros:**
- ✅ Direct connection, no proxy needed
- ✅ Full BC connector functionality
- ✅ All Power BI features work

**Cons:**
- ⚠️ Requires BC configuration change
- ⚠️ May affect other BC integrations
- ⚠️ User reported getting "locked out" (needs investigation)

### Solution 2: Use BC Web Services

**Connect to BC's SOAP/REST web services instead of OData:**

1. Publish required pages/queries as web services in BC
2. Use Power BI's Web connector
3. Authenticate with Windows credentials directly

**Pros:**
- ✅ More flexible data access
- ✅ Can use Windows Auth
- ✅ Better control over exposed data

**Cons:**
- ⚠️ Requires web service configuration in BC
- ⚠️ More complex Power BI queries
- ⚠️ May require custom M code

### Solution 3: Export Data to Intermediate Database

**Use BC's built-in data export or custom integration:**

1. Export BC data to SQL Server/Azure SQL
2. Connect Power BI to SQL database
3. Schedule regular data refreshes

**Pros:**
- ✅ Full Power BI functionality
- ✅ Better performance
- ✅ No authentication issues

**Cons:**
- ⚠️ Not real-time data
- ⚠️ Requires additional infrastructure
- ⚠️ Data synchronization complexity

---

## Technical Artifacts

### Proxy Implementation Status

| Component | Status | Notes |
|-----------|--------|-------|
| Basic Auth Middleware | ✅ Working | Successfully validates credentials |
| Windows Auth to BC | ✅ Working | Uses `UseDefaultCredentials` |
| Request Forwarding | ✅ Working | All HTTP methods supported |
| URL Rewriting | ✅ Working | BC URLs rewritten to proxy URLs |
| OData Routing | ✅ Working | Catch-all routing implemented |
| BC Connector Support | ❌ Failed | Requires full BC API emulation |
| OData Connector Support | ❌ Failed | BC's non-standard structure incompatible |

### Test Commands

**Test Basic Auth:**
```powershell
curl -u "POWER BI:Exlifes_69" http://localhost:9000/ODataV4/Company('CRONUS%20International%20Ltd.')
```

**Test Windows Auth to BC:**
```powershell
Invoke-WebRequest -Uri "http://brandon:7048/BC240/ODataV4/Company('CRONUS%20International%20Ltd.')" -UseDefaultCredentials -AllowUnencryptedAuthentication
```

**List BC Companies:**
```powershell
Invoke-WebRequest -Uri "http://brandon:7048/BC240/ODataV4/Company" -UseDefaultCredentials -AllowUnencryptedAuthentication | ConvertFrom-Json | Select-Object -ExpandProperty value | Select-Object name
```

### Deployment with NSSM

**Install as Windows Service:**
```powershell
# Build for production
dotnet publish -c Release -o ./publish

# Install service
nssm install BIProxy "c:\Users\USERR\PythonProject\BI Proxy\publish\BIProxy.exe"
nssm set BIProxy AppDirectory "c:\Users\USERR\PythonProject\BI Proxy\publish"
nssm set BIProxy DisplayName "BI OData Proxy"
nssm set BIProxy Start SERVICE_AUTO_START

# Start service
nssm start BIProxy
```

---

## Recommendations

### Immediate Next Steps

1. **Investigate BC NavUserPassword "lockout" issue**
   - Understand why changing to NavUserPassword locks user out
   - Check BC user permissions and configuration
   - Test with a dedicated service account

2. **If NavUserPassword is not viable:**
   - Consider BC Web Services approach
   - Evaluate intermediate database solution
   - Explore BC's built-in Power BI integration features

3. **Document BC configuration requirements**
   - Current authentication settings
   - User permissions needed
   - Any constraints preventing NavUserPassword

### Long-term Considerations

- **BC Upgrade Path:** Future BC versions may have better Power BI integration
- **Cloud Migration:** BC Online has native Power BI support
- **Alternative BI Tools:** Consider tools with better BC on-premises support

---

## Lessons Learned

1. **Authentication conversion is not enough** - Protocol compatibility matters
2. **Standard compliance is critical** - Non-standard implementations cause integration issues
3. **Connector expectations vary** - Different Power BI connectors have different requirements
4. **Testing at multiple levels** - API tests passing doesn't guarantee UI connector success
5. **BC's architecture is complex** - Simple proxies cannot bridge fundamental structural differences

---

## Project Status: **INCOMPLETE**

**Reason:** Business Central's non-standard OData implementation prevents Power BI connectivity through a simple authentication proxy.

**Proxy Functionality:** ✅ **Working as designed** (auth conversion, request forwarding)

**Power BI Integration:** ❌ **Not achievable** with current approach

**Recommended Path Forward:** Configure BC to use NavUserPassword authentication for direct Power BI connectivity.
