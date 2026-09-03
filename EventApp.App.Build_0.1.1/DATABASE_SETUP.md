# Database & Storage Setup Guide

## Database Configuration

This application uses different connection strings based on the environment:

- **appsettings.json**: Default connection string (LocalDB) - committed to Git
- **appsettings.Development.json**: Development override - can be customized per developer

### Setup Database using the SQL Script

Create a new database named `db-eventapp` in your SQL Server instance. You can do this using SQL Server Management Studio (SSMS) or any other SQL client.
Use the following SQL script for the tables and sample data: `EventApp.App.Build_0.1.1/Database/event-app_create.sql'

### Quick Setup for Your Machine

#### Option 1: SQL Server Express (Recommended for most developers)

If you see `.\SQLEXPRESS` in SQL Server Management Studio, use this connection string in your `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "db-eventapp": "Server=.\\SQLEXPRESS;Database=db-eventapp;Integrated Security=True;TrustServerCertificate=True;"
}
```

#### Option 2: SQL Server LocalDB

If you prefer LocalDB, use this connection string:

```json
"ConnectionStrings": {
  "db-eventapp": "Server=(localdb)\\mssqllocaldb;Database=db-eventapp;Integrated Security=True;TrustServerCertificate=True;"
}
```

#### Option 3: Named SQL Server Instance

If your SQL Server has a different instance name (like `machinename\SQLEXPRESS`), replace the Server value:

```json
"ConnectionStrings": {
  "db-eventapp": "Server=YOUR-MACHINE-NAME\\SQLEXPRESS;Database=db-eventapp;Integrated Security=True;TrustServerCertificate=True;"
}
```

### How to Apply Your Configuration

1. Open `.\appsettings.Development.json`
2. Add or update the `ConnectionStrings` section with your preferred connection string
3. **Do NOT commit** `appsettings.Development.json` if it contains machine-specific settings
4. Run the application - it will use your Development settings automatically

### Connection String Components Explained

- **Server**: Your SQL Server instance location
  - `.` or `localhost` = default local instance
  - `.\SQLEXPRESS` = SQL Server Express default instance
  - `(localdb)\mssqllocaldb` = LocalDB instance
- **Database**: `db-eventapp` - the database name (should be the same for everyone)
- **Integrated Security=True**: Uses your Windows authentication (no username/password needed)
- **TrustServerCertificate=True**: Allows encrypted connections without certificate validation

## Image Storage Configuration

### Storage Location

Images are stored in the project's `wwwroot\uploads` directory:

- **.\wwwroot\uploads\** - Web application
- **.\wwwroot\uploads\** - Organiser portal

### Directory Structure

```
wwwroot\
└── uploads\
    └── events\
        ├── {organiser_id}\
        │   ├── {event_id}\
        │   │   └── uploads\
        │   │       ├── {guid}.jpg
        │   │       ├── {guid}.png
        │   │       └── ...
        │   └── cache\  (temporary uploads during event creation)
        └── placeholder.jpg  (fallback when no image exists)
```

### How Images Work

1. **Upload** (via Organiser Portal):
   - Organiser uploads images through Create Event page
   - Files validated: max 5MB, formats: `.jpg`, `.jpeg`, `.png`, `.webp`
   - Saved to `cache` folder temporarily
   - Moved to permanent `uploads/{event_id}` folder after event creation
   - URLs stored in `dbo.event_images` database table

2. **Access** (via Web App):
   - Images served at URL: `/images/events/{organiser_id}/{event_id}/uploads/{filename}`
   - Physical path: `wwwroot\uploads\events\{organiser_id}\{event_id}\uploads\{filename}`
   - Fallback: `/images/events/placeholder.jpg` if no image exists

3. **Configuration**:
   - Set in `appsettings.json` under `SharedStorage:ImagesPath`
   - Current default: `.\wwwroot\uploads`
   - Both App and Organiser projects must use the same path for shared storage

### Changing Image Storage Location

To use a different directory (e.g., network drive, external storage):

1. Update `appsettings.json` in **both** projects:
   ```json
   "SharedStorage": {
     "ImagesPath": "D:\\MyImageStorage"
   }
   ```

2. Ensure the directory is accessible and has write permissions

3. The application will automatically create subdirectories as needed

## Troubleshooting

### Database Connection Errors

**Error: "Could not open a connection to SQL Server"**

1. Verify SQL Server is running:
   - Open "SQL Server Configuration Manager"
   - Check if your SQL Server instance is running

2. Test connection in SSMS:
   - Open SQL Server Management Studio
   - Connect using the server name from your connection string
   - Copy the successful connection string format

3. Check your connection string:
   - Make sure the Server name matches your SSMS connection
   - Verify the instance name is correct (SQLEXPRESS, MSSQLSERVER, etc.)

**Database doesn't exist**

The database needs to be created first. You can either:
- Create it manually in SSMS: `CREATE DATABASE [db-eventapp]`
- Run database migration scripts (if available in the project)

### Image Upload Errors

**Error: "SharedStorage:ImagesPath ontbreekt"**

- The `SharedStorage:ImagesPath` setting is missing from `appsettings.json`
- Add it back with your desired path

**Images not appearing**

1. Check directory permissions - application needs read/write access
2. Verify images exist in the physical directory
3. Check database `event_images` table for correct URLs
4. Ensure `Program.cs` static file middleware is configured correctly

## Team Guidelines

✅ **DO commit**: 
- `appsettings.json` with generic/default settings
- Default image storage path: `.\wwwroot\uploads`

❌ **DON'T commit**: 
- Machine-specific changes to `appsettings.Development.json`
- Images in `wwwroot\uploads` (add to `.gitignore`)

Each developer should configure their own `appsettings.Development.json` based on their local SQL Server setup.

