using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using dotnet_etcd;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DotnetEtcdProvider.Test;

public class UnitTest : IAsyncLifetime
{
    private readonly TestcontainersContainer testcontainer = new TestcontainersBuilder<TestcontainersContainer>()
        .WithImage("bitnami/etcd:latest")
        .WithName("bitnami-etcd-test")
        .WithEnvironment("ALLOW_NONE_AUTHENTICATION", "yes")
        .WithEnvironment("ETCD_ADVERTISE_CLIENT_URLS", "http://etcd-server:2379")
        .WithPortBinding(2379)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(2379))
        .WithPortBinding(2380)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(2380))
        .Build();

    private DotnetEtcdProviderTest _dotnetEtcdProviderTest = new();

    public UnitTest()
    {
        _dotnetEtcdProviderTest = new DotnetEtcdProviderTest
        {
            IntegerValue = 1,
            DoubleValue = 2.1,
            DoubleArrayValue = new double[] { 2.2, 2.3 },
            StringValue = "msg",
            StringArrayValue = new string[] { "String02", "String03" },
            ObjectModel = new DotnetEtcdProviderTestObject
            {
                MyProperty = "Property From Etcd",
                DecimalValue = 3.01m
            }
        };
    }

    public async Task DisposeAsync()
    {

        await this.testcontainer.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await this.testcontainer.StartAsync();

        EtcdClient etcdClient = new EtcdClient("http://localhost:2379");
        await etcdClient.PutAsync("DotnetEtcdProviderTest:IntegerValue", _dotnetEtcdProviderTest.IntegerValue.ToString());
        await etcdClient.PutAsync("DotnetEtcdProviderTest:DoubleValue", _dotnetEtcdProviderTest.DoubleValue.ToString());
        await etcdClient.PutAsync("DotnetEtcdProviderTest:DoubleArrayValue:0", _dotnetEtcdProviderTest.DoubleArrayValue[0].ToString());
        await etcdClient.PutAsync("DotnetEtcdProviderTest:DoubleArrayValue:1", _dotnetEtcdProviderTest.DoubleArrayValue[1].ToString());
        await etcdClient.PutAsync("DotnetEtcdProviderTest:StringValue", _dotnetEtcdProviderTest.StringValue);
        await etcdClient.PutAsync("DotnetEtcdProviderTest:StringArrayValue:0", _dotnetEtcdProviderTest.StringArrayValue[0]);
        await etcdClient.PutAsync("DotnetEtcdProviderTest:StringArrayValue:1", _dotnetEtcdProviderTest.StringArrayValue[1]);
        await etcdClient.PutAsync("DotnetEtcdProviderTest:ObjectModel:MyProperty", _dotnetEtcdProviderTest.ObjectModel.MyProperty);
        await etcdClient.PutAsync("DotnetEtcdProviderTest:ObjectModel:DecimalValue", _dotnetEtcdProviderTest.ObjectModel.DecimalValue.ToString());

    }

    [Fact]
    public void TestData_ReturnCorrectData()
    {
        string[] args = Array.Empty<string>();
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.Sources.Clear();
                configuration.SetBasePath(AppContext.BaseDirectory)
                            .AddJsonFile("appsettings.json", false, true);
                configuration.AddEtcdConfiguration("Etcd");
            })
            .ConfigureServices((context, services) =>
                services.Configure<DotnetEtcdProviderTest>(
                    context.Configuration.GetSection("DotnetEtcdProviderTest")))
            .Build();

        var options = host.Services.GetRequiredService<IOptions<DotnetEtcdProviderTest>>().Value;

        Assert.Equal(_dotnetEtcdProviderTest.IntegerValue, options.IntegerValue);
        Assert.Equal(_dotnetEtcdProviderTest.DoubleValue, options.DoubleValue);
        Assert.Equal(_dotnetEtcdProviderTest.StringArrayValue, options.StringArrayValue);
        Assert.Equal(_dotnetEtcdProviderTest.DoubleArrayValue, options.DoubleArrayValue);
        Assert.Equal(_dotnetEtcdProviderTest.ObjectModel.MyProperty, options.ObjectModel.MyProperty);
        Assert.Equal(_dotnetEtcdProviderTest.ObjectModel.DecimalValue, options.ObjectModel.DecimalValue);
    }
}

public class DotnetEtcdProviderTest
{
    public int IntegerValue { get; set; }

    public string StringValue { get; set; }

    public double DoubleValue { get; set; }

    public string[] StringArrayValue { get; set; }

    public double[] DoubleArrayValue { get; set; }

    public DotnetEtcdProviderTestObject ObjectModel { get; set; }
}

public class DotnetEtcdProviderTestObject
{
    public string MyProperty { get; set; }

    public decimal DecimalValue { get; set; }
}