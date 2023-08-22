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

## Data Setup in Etcd
There are some naming conventions we need follow for `key` in Etcd. The naming will be vary depend on the data structure we wanted.

### Normal Data
For naming convention, we just need to follow `<Main model name>:<Variable name>` pattern when we create key for Etcd.

```csharp
public class DemoCls{
    public int IntegerValue { get; set; }
    public double DoubleValue { get; set; }
    public string StringValue { get; set; }
}
```

`Etcd Key`
``` 
<Main model name>:<Variable name>
DemoCls:IntegerValue
DemoCls:DoubleValue
DemoCls:StringValue
```


### Array/List
For array/list, we just need to add index at behind of the key. We can use any number and C# will auto rearrange for it based on index we provided. 

```csharp
public class DemoCls{
    public int[] ArrayObject { get; set; }
}

public class DemoCls{
    public List<int> ArrayObject { get; set; }
}
```

`Etcd Key`
``` 
<Main model name>:<Variable name>:<Numeric Index>
DemoCls:ArrayObject:8
DemoCls:ArrayObject:10
DemoCls:ArrayObject:3
```

Based on above example, it will create array of ArrayObject with length of 3 in sequence such as below:
```
[
    `Value of DemoCls:ArrayObject:3`,
    `Value of DemoCls:ArrayObject:8`,
    `Value of DemoCls:ArrayObject:10`,
]
```

### Object


```csharp
public class DemoCls{
    public ModelValue ModelValueA { get; set; }
}

public class ModelValue{
    public string StringValue { get; set; }
    public string StringValue2 { get; set; }
    public ModelValueB ModelValueB { get; set; }
}

public class ModelValueB{
    public int IntValue { get; set; }
}
```

`Etcd Key`
``` 
DemoCls:ModelValueA:StringValue
DemoCls:ModelValueA:StringValue2
DemoCls:ModelValueA:ModelValueB:IntValue
```
