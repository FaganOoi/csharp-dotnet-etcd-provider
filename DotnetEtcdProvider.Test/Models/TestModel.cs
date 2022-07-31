namespace DotnetEtcdProvider.Test.Models;

public class TestModel
{
    public int IntegerValue { get; set; }

    public string StringValue { get; set; }

    public double DoubleValue { get; set; }

    public string[] StringArrayValue { get; set; }

    public double[] DoubleArrayValue { get; set; }

    public TestObject ObjectModel { get; set; }
}

public class TestObject
{
    public string MyProperty { get; set; }

    public decimal DecimalValue { get; set; }
}
