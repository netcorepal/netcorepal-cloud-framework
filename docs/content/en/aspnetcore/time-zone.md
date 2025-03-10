# Time Zone Handling

To ensure the system runs correctly in different time zones, time zone handling needs to be considered. Here are some principles to ensure the correctness of time zone handling.

+ Use `DateTimeOffset` instead of `DateTime` to avoid issues with inconsistent application time zone environments;
+ Use `DateTimeOffset.UtcNow` instead of `DateTime.Now`;
+ Store UTC time in the database. For MySQL databases, `DateTimeOffset` will be automatically converted to UTC time for storage;
+ Do not rely on the database to generate time. All time generation should be handled by the application to avoid issues with inconsistent database server time zones;
