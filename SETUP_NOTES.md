# Configuration Notes

## Before First Run

1. **Update Windows Password**
   - Open `appsettings.json`
   - Replace `"WindowsPassword": "your-windows-password"` with your actual password
   - Save the file

2. **Verify BC Connection Details**
   - Confirm BC server: `http://brandon:7048/BC240/ODataV4`
   - Confirm domain: `brandon`
   - Confirm username: `USERR`

3. **Test BC Connectivity**
   - Open browser and navigate to: `http://brandon:7048/BC240/ODataV4`
   - You should see OData service document (may prompt for Windows auth)

## Quick Start Commands

### Development (Local Testing)
```bash
cd "c:\Users\USERR\PythonProject\BI Proxy"
dotnet run
```

### Production Build
```bash
cd "c:\Users\USERR\PythonProject\BI Proxy"
dotnet publish -c Release -o ./publish
```

### NSSM Installation (Run as Administrator)
```bash
# Assuming NSSM is in C:\Tools\nssm
cd C:\Tools\nssm\win64

# Install service
nssm install BIProxy "c:\Users\USERR\PythonProject\BI Proxy\publish\BIProxy.exe"
nssm set BIProxy AppDirectory "c:\Users\USERR\PythonProject\BI Proxy\publish"
nssm set BIProxy DisplayName "BI OData Proxy"
nssm set BIProxy Description "Authentication proxy for Power BI to Business Central"
nssm set BIProxy Start SERVICE_AUTO_START

# Start service
nssm start BIProxy

# Check status
nssm status BIProxy
```

## Power BI Connection String

```
URL: http://localhost:9000/ODataV4/Company('CRONUS%20International%20Ltd.')
Authentication: Basic
Username: POWER BI
Password: Exlifes_69
```

## Testing Checklist

- [ ] Updated Windows password in appsettings.json
- [ ] Tested BC connectivity from browser
- [ ] Built project successfully (`dotnet build`)
- [ ] Ran proxy locally (`dotnet run`)
- [ ] Tested with curl (valid credentials)
- [ ] Tested with curl (invalid credentials - should get 401)
- [ ] Published for production (`dotnet publish`)
- [ ] Installed NSSM service
- [ ] Started NSSM service
- [ ] Verified service is running (`nssm status BIProxy`)
- [ ] Connected from Power BI Desktop
- [ ] Verified data loads in Power BI

## Common Issues

### Issue: Port 9000 already in use
**Solution:** 
```bash
# Find process using port 9000
netstat -ano | findstr :9000

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F
```

### Issue: 401 Unauthorized from BC
**Solution:**
- Verify Windows credentials in appsettings.json
- Ensure user `USERR` has access to BC
- Check domain is correct (`brandon`)

### Issue: Service won't start
**Solution:**
- Check Event Viewer → Windows Logs → Application
- Verify .NET 8.0 Runtime is installed
- Try running manually first: `dotnet "c:\Users\USERR\PythonProject\BI Proxy\publish\BIProxy.dll"`

### Issue: Power BI can't connect
**Solution:**
- Verify proxy is running: `curl http://localhost:9000/ODataV4/`
- Check Windows Firewall isn't blocking port 9000
- Ensure credentials are exactly: `POWER BI` / `Exlifes_69`
