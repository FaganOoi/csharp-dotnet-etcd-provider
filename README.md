# DotnetEtcdProvider

It is custom configuration provider that use `dotnet-etcd` library to load the configuration and update it from time to time. It help to provide alternative ways for us to change our appsettings without re-deploy the application. For now, it just support basic login and authentication.

## Dependency

- [dotnet-etcd](https://github.com/shubhamranjan/dotnet-etcd)

## UI Tool for etcd

The UI Tool supported and tested to confirm it is working well with the package

- `etcdkeeper`
- `etcd-manager`

## Getting Started

### Step 1: AppSettings

In our appsettings.json, we need to put configuration settings to access Etcd.
By default, we will use `OnChangeReload` Reload Mode which it will watch prefix based on `PrefixListUsedToWatch` and made changes accordinly. If we would like to reload it after fixed duration, we can change to `ScheduledReload` reload mode and provide seconds in fields of `SecondsToReload`.

```json
"EtcdSettings": {
    "URL":  "http://localhost:2379",
    "Name": "Username(Optional)",
    "Password": "****(Optional)",
    "ReloadMode": "OnChangeReload",
    "SecondsToReload": 0,
    "PrefixListUsedToWatch": [
        "Array of string to watch prefixc"
    ],

}
```

Reload Mode Available

```csharp
public enum ReloadMode
{
    ScheduledReload,
    OnChangeReload
}
```

### Step 2: Setup in Program

```csharp
builder.Configuration.AddEtcdConfiguration(<It can be any value depend on the name we used to setup in appsettings.json>);

eg.
builder.Configuration.AddEtcdConfiguration("EtcdSettings");
```

### Step 3: Do Dependency Injection

```csharp
eg.
builder.Services.Configure<AppSettingsFromEtcd>(builder.Configuration.GetSection("AppSettingsFromEtcd"));
```

### Startup

```csharp
//Demo how to get data from etcd in startup
AppSettingsFromEtcd tmp = new();
builder.Configuration.GetSection("AppSettingsFromEtcd").Bind(tmp);
```

## Data Setup in Different Tools

### etcdkeeper

It is the tool most suggested to access etcd as the UI more friendly and we can insert JSON/XML value easily.

### etcd-manager

Compare to etcdkeepr, we need to take more concern about the key pattern when creat etcd value
As it does not have directory concept, we need to cater the concept manually besides it is hard for us to insert value such as JSON, XML and etc.
For the key pattern need to follow, we can refer to [here](./DotnetEtcdProvider/README.md)
