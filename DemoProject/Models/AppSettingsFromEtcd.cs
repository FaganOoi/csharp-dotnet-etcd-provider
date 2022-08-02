namespace DemoProject.Models;

public class AppSettingsFromEtcd
{
    public string Name { get; set; }

    public int IntegerValue { get; set; }

    public double DoubleValue { get; set; }

    public AppSettingsFromEtcdObject ObjectItem { get; set; }

    public ModelValue ModelValueA { get; set; }

    public int[] ListInt { get; set; }
}

public class AppSettingsFromEtcdObject
{
    public string Username { get; set; }

    public string IndexName { get; set; }
}

public class ModelValue
{
    public string StringValue { get; set; }
    public string StringValue2 { get; set; }
    public ModelValueB ModelValueB { get; set; }
}

public class ModelValueB
{
    public int IntValue { get; set; }
}