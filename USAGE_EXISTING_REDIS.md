# Using Existing Redis Connections with DotNet.RateLimit

This example demonstrates how to use existing Redis connections with the DotNet.RateLimit library instead of creating new ones.

## Problem

Previously, the library always created its own Redis connections, which could cause:
- Connection conflicts with existing Redis infrastructure
- DI registration conflicts
- Issues with clustered Redis configurations
- Resource waste from duplicate connections

## Solution

The library now provides overloaded `AddRateLimitService` methods that accept existing Redis connections:

### Using Existing IConnectionMultiplexer

```csharp
// Setup your existing Redis connection
var multiplexer = ConnectionMultiplexer.Connect("your-redis-connection-string");
services.AddSingleton<IConnectionMultiplexer>(multiplexer);

// Use it with rate limiting
services.AddRateLimitService(configuration, multiplexer);
```

### Using Existing IDatabase

```csharp
// Setup your existing Redis database
var multiplexer = ConnectionMultiplexer.Connect("your-redis-connection-string");
var database = multiplexer.GetDatabase();
services.AddSingleton<IDatabase>(database);

// Use it with rate limiting
services.AddRateLimitService(configuration, database);
```

### Clustered Redis Example

```csharp
// Configure clustered Redis
var config = new ConfigurationOptions
{
    EndPoints = { "node1:6379", "node2:6379", "node3:6379" },
    ClientName = "MyApp"
};
var multiplexer = ConnectionMultiplexer.Connect(config);

// Your application services use this connection
services.AddSingleton<IConnectionMultiplexer>(multiplexer);
services.AddTransient<IDatabase>(provider => provider.GetService<IConnectionMultiplexer>().GetDatabase());

// Rate limiting uses the same connection
services.AddRateLimitService(configuration, multiplexer);
```

## Backward Compatibility

The original method still works as before:

```csharp
// This continues to work and creates new connections
services.AddRateLimitService(configuration);
```

All existing usage patterns remain unchanged and fully supported.