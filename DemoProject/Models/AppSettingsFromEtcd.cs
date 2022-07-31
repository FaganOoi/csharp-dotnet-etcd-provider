namespace DemoProject.Models;

public class AppSettingsFromEtcd
{
    public string Name { get; set; }

    public int IntegerValue { get; set; }

    public double DoubleValue { get; set; }

    public AppSettingsFromEtcdObject ObjectItem { get; set; }
}

public class AppSettingsFromEtcdObject
{
    public string Username { get; set; }

    public string IndexName { get; set; }
}
