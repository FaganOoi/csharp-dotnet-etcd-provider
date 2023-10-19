# DotnetEtcdProvider

It is custom configuration provider that use `dotnet-etcd` library to load the configuration and update it from time to time. It help to provide alternative ways for us to change our appsettings without re-deploy the application.

## Version 1.3

For new version 1.3, it will have breaking changes as following

- Rename `PrefixListUsedToWatch` to `PrefixData`. For now, we will use list to get data and look on the changes based on reload mode. When no data given, we will have hard code value `/` to get all.

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
    "PrefixData": [
        "Array of prefix to get data and load the changes"
    ],

}
```

| Property                       | Description                                                                                                                                                                       | Default Value               |
| ------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------- |
| `URL`                          | Connection string included http/https. We will prioritize on a single endpoint.                                                                                                   |                             |
| `URLs`                         | Connections string included http/https.                                                                                                                                           |                             |
| `Username`                     | Username for authentication.                                                                                                                                                      |                             |
| `Password`                     | Password for authentication.                                                                                                                                                      |                             |
| `SecondsToReload`              | Duration to reload data when `ReloadMode` is ScheduledReload.                                                                                                                     |                             |
| `PrefixData`                   | It is a list of prefixes that will get data from ETCD and look for changes based on the reload mode. If no data is provided, assume getting all data and look for changes at "/". | `new List<string>()`        |
| `PrefixListUsedToRemoveInData` | List of prefixes to remove when keeping data in the provider.                                                                                                                     |                             |
| `ReloadMode`                   | Mode to configure how to update data from time to time or display continuous watching.                                                                                            | `ReloadMode.OnChangeReload` |

Reload Mode Available

```csharp
public enum ReloadMode
{
    None,
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
