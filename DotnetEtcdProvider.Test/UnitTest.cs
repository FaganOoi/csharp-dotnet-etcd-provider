using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using dotnet_etcd;
using DotnetEtcdProvider.Extensions;
using DotnetEtcdProvider.Test.Models;
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
        // 2379: Client Request
        .WithPortBinding(2379)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(2379))
        .Build();

    private TestModel _dotnetEtcdProviderTest = new();

    public UnitTest()
    {
        _dotnetEtcdProviderTest = new TestModel
        {
            IntegerValue = 1,
            DoubleValue = 2.1,
            DoubleArrayValue = new double[] { 2.2, 2.3 },
            StringValue = "msg",
            StringArrayValue = new string[] { "String02", "String03" },
            ObjectModel = new TestObject
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
        Dictionary<string, string> keyValuePairs = GetReflectionModel(_dotnetEtcdProviderTest, "");
        foreach (var keyPair in keyValuePairs)
        {
            await etcdClient.PutAsync(keyPair.Key, keyPair.Value);
        }

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
                services.Configure<TestModel>(
                    context.Configuration.GetSection("TestModel")))
            .Build();

        var options = host.Services.GetRequiredService<IOptions<TestModel>>().Value;

        Assert.Equal(_dotnetEtcdProviderTest.IntegerValue, options.IntegerValue);
        Assert.Equal(_dotnetEtcdProviderTest.DoubleValue, options.DoubleValue);
        Assert.Equal(_dotnetEtcdProviderTest.StringArrayValue, options.StringArrayValue);
        Assert.Equal(_dotnetEtcdProviderTest.DoubleArrayValue, options.DoubleArrayValue);
        Assert.Equal(_dotnetEtcdProviderTest.ObjectModel.MyProperty, options.ObjectModel.MyProperty);
        Assert.Equal(_dotnetEtcdProviderTest.ObjectModel.DecimalValue, options.ObjectModel.DecimalValue);
    }

    private Dictionary<string, string> GetReflectionModel(object model, string parentName, string currentName = "")
    {
        Dictionary<string, string> keyValuePairs = new();
        string dataModelName = "";
        if (parentName.HasData())
        {

            dataModelName = parentName + ":" + currentName;
        }
        else
        {
            dataModelName += model.GetType().Name;
        }

        var propertiesInfo = model.GetType().GetProperties();
        foreach (var propertyInfo in propertiesInfo)
        {
            if (propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType == typeof(String))
            {
                keyValuePairs.Add(
                    $"{dataModelName}:{propertyInfo.Name}",
                    propertyInfo.GetValue(model).ToString()
                );
            }
            else if (propertyInfo.PropertyType.IsValueType && propertyInfo.PropertyType.IsArray)
            {
                Array a = (Array)propertyInfo.GetValue(model);
                for (int i = 0; i < a.Length; i++)
                {
                    keyValuePairs.Add(
                        $"{dataModelName}:{propertyInfo.Name}:{i}",
                        a.GetValue(i).ToString()
                    );
                }
            }
            else if (propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType.IsArray)
            {
                Array a = (Array)propertyInfo.GetValue(model);
                for (int i = 0; i < a.Length; i++)
                {
                    if (a.GetValue(i).GetType().IsValueType || a.GetValue(i).GetType() == typeof(String))
                    {
                        keyValuePairs.Add(
                            $"{dataModelName}:{propertyInfo.Name}:{i}",
                            a.GetValue(i).ToString()
                        );
                    }
                    else
                    {
                        var dat = GetReflectionModel(a.GetValue(i), dataModelName, propertyInfo.Name);
                        foreach (var tmp in dat)
                        {
                            keyValuePairs.Add(tmp.Key, tmp.Value);
                        }
                    }
                }
            }
            else if (propertyInfo.PropertyType.IsClass)
            {
                var dat = GetReflectionModel(propertyInfo.GetValue(model), dataModelName, propertyInfo.Name);
                foreach (var tmp in dat)
                {
                    keyValuePairs.Add(tmp.Key, tmp.Value);
                }
            }
        }

        return keyValuePairs;
    }
}