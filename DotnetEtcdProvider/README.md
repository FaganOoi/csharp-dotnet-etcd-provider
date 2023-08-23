# DotnetEtcdProvider

## Data Setup Pattern required for Configuration Provider

### Normal Data

For naming convention, we just need to follow `<Main model name>:<Variable name>` pattern.

```csharp
public class DemoCls{
    public int IntegerValue { get; set; }
    public double DoubleValue { get; set; }
    public string StringValue { get; set; }
}
```

`Key`

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

`Key`

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

`Key`

```
DemoCls:ModelValueA:StringValue
DemoCls:ModelValueA:StringValue2
DemoCls:ModelValueA:ModelValueB:IntValue
```
