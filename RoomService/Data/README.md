# Room Service XML-Based Seeding

## Overview
The RoomService now supports reading default room configurations from XML files instead of hardcoded values. This allows for easier configuration management and deployment flexibility.

## How It Works

### 1. **XML Storage Mode**
- **Configuration**: Set `Storage:Type` to `"Database"` in `appsettings.json`
- **Default Rooms**: Loaded from `Data/default-rooms.xml` on startup
- **Runtime Storage**: All rooms stored in SQLite database 
- **Operations**: CRUD operations use database (fast performance)

### 2. **XML File Structure**
```xml
<?xml version="1.0" encoding="utf-8"?>
<ArrayOfRoomCardViewModel>
  <RoomCardViewModel>
    <Id>1</Id>
    <RoomName>Deluxe Suite</RoomName>
    <ImageUrl>/images/delux.jpg</ImageUrl>
    <Description>Luxurious room with sea view...</Description>
    <Amenities>
      <string>King Bed</string>
      <string>Wi-Fi</string>
    </Amenities>
    <isDefault>true</isDefault>
    <Price>299.99</Price>
    <NumberOfRooms>5</NumberOfRooms>
  </RoomCardViewModel>
  <!-- More rooms... -->
</ArrayOfRoomCardViewModel>
```

### 3. **Configuration Options**
```json
{
  "Storage": {
    "Type": "Database",
    "XmlFilePath": "rooms.xml",
    "DefaultRoomsXmlPath": "Data/default-rooms.xml"
  }
}
```

### 4. **Seeding Process**
1. Service starts up
2. Checks if database is empty (room count = 0)
3. Reads default rooms from XML file
4. Seeds rooms into database
5. Fallback to hardcoded defaults if XML fails

### 5. **Benefits**
- **Configurable**: Room types can be changed via XML without code changes
- **Version Control**: XML files can be versioned and deployed
- **Environment-Specific**: Different XML files for dev/staging/prod
- **Fallback Safety**: Always has hardcoded defaults as backup
- **Performance**: Database operations remain fast

### 6. **Files Involved**
- `Data/default-rooms.xml` - Default room configurations
- `Helpers/XmlRoomReader.cs` - XML reading logic
- `Helpers/RoomSeedHelper.cs` - Database seeding with XML support
- `Program.cs` - Startup seeding orchestration

## Usage Examples

### Adding New Room Types
1. Edit `Data/default-rooms.xml`
2. Add new `<RoomCardViewModel>` entry
3. Restart service (will only seed if database is empty)

### Environment-Specific Rooms
- `appsettings.json`: `"DefaultRoomsXmlPath": "Data/production-rooms.xml"`
- `appsettings.Development.json`: `"DefaultRoomsXmlPath": "Data/dev-rooms.xml"`

### Reset and Re-seed
1. Delete `rooms.db` file
2. Restart service
3. Will re-seed from XML automatically
