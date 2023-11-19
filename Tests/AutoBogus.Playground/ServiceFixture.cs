using AutoBogus.Conventions;
using AutoBogus.Playground.Model;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace AutoBogus.Playground;

public abstract class ServiceFixture
{
    private readonly IAutoFaker _faker;

    private readonly Item              _item;
    private readonly IEnumerable<Item> _items;

    private readonly IRepository _repository;
    private readonly Service     _service;

    protected ServiceFixture(ITestOutputHelper output, IAutoBinder binder)
    {
        _faker = AutoFaker.Create(builder =>
        {
            builder.WithOverride(new ProductGeneratorOverride());
            builder.WithBinder(binder);
        });

        // Setup
        var id = _faker.Generate<Guid>();
        var generator = new AutoFaker<Item>(null, binder)
            .RuleFor(item => item.Id,   () => id)
            .RuleFor(item => item.Name, f => f.Person.FullName)
            .RuleFor(item => item.Amendments, _ => new HashSet<string>
            {
                "1",
                "2",
                "3",
            });

        _item  = generator;
        _items = generator.Generate(5);

        _repository = Substitute.For<IRepository>();

        _repository.Get(id).Returns(_item);
        _repository.GetAll().Returns(_items);

        _service = new Service(_repository);
    }

    [Fact]
    public void Should_Set_Timestamp()
    {
        Assert.NotNull(_item.Timestamp);
    }

    [Fact]
    public void Should_Set_Product_Notes()
    {
        var product = AutoFaker.Generate<TestProduct>();
        Assert.NotEmpty(product.GetNotes());
    }

    [Fact]
    public void Should_Not_Set_Product_Code()
    {
        var product = AutoFaker.Generate<TestProduct>(builder =>
        {
            builder.WithSkip<Exception>();
        });

        Assert.Null(product.Error);
    }

    [Fact]
    public void Should_Not_Set_Product_Notes()
    {
        var product = AutoFaker.Generate<TestProduct>(builder =>
        {
            builder.WithSkip<TestProduct>("Notes");
        });

        Assert.Empty(product.GetNotes());
    }

    [Fact]
    public void Service_Get_Should_Call_Repository_Get()
    {
        _service.Get(_item.Id);

        _repository.Received().Get(_item.Id);
    }

    [Fact]
    public void Service_Get_Should_Return_Item()
    {
        Assert.Equal(_item, _service.Get(_item.Id));
    }

    [Fact]
    public void Service_GetAll_Should_Call_Repository_GetAll()
    {
        _service.GetAll();

        _repository.Received().GetAll();
    }

    [Fact]
    public void Service_GetAll_Should_Return_Items()
    {
        Assert.Same(_items, _service.GetAll());
    }

    [Fact]
    public void Service_GetPending_Should_Call_Repository_GetFiltered()
    {
        _service.GetPending();

        _repository.Received().GetFiltered(Service.PendingFilter);
    }

    [Fact]
    public void Service_GetPending_Should_Return_Items()
    {
        var id            = _faker.Generate<Guid>();
        var item          = AutoFaker.Generate<Item>();
        var item1Override = new ProductCodeOverride();
        var items = new List<Item>
        {
            new ItemFaker(id).Configure(builder => builder.WithOverride(item1Override)),
            AutoFaker.Generate<Item>(builder => builder.WithSkip<Item>(i => i.ProcessedBy)),
            AutoFaker.Generate<Item>(builder => builder.WithConventions(c => c.Email.Aliases("SupplierEmail"))),
        };

        item.Status = ItemStatus.Pending;
        items.Add(item);

        _repository.GetFiltered(Service.PendingFilter).Returns(items);

        Assert.Same(items, _service.GetPending());

        Assert.DoesNotContain("@", items[0].ProcessedBy.Email, StringComparison.Ordinal);
        Assert.DoesNotContain("@", items[0].SupplierEmail,     StringComparison.Ordinal);
        Assert.Equal(item1Override.Code, items[0].ProductInt.Code.SerialNumber);
        Assert.Equal(item1Override.Code, items[0].ProductString.Code.SerialNumber);

        Assert.Null(items[1].ProcessedBy);
        Assert.DoesNotContain("@", items[1].SupplierEmail, StringComparison.Ordinal);
        Assert.Null(items[1].ProductInt.Code.SerialNumber);
        Assert.Null(items[1].ProductString.Code.SerialNumber);

        Assert.Contains("@", items[2].ProcessedBy.Email, StringComparison.Ordinal);
        Assert.Contains("@", items[2].SupplierEmail,     StringComparison.Ordinal);
        Assert.Null(items[2].ProductInt.Code.SerialNumber);
        Assert.Null(items[2].ProductString.Code.SerialNumber);

        Assert.DoesNotContain("@", items[3].ProcessedBy.Email, StringComparison.Ordinal);
        Assert.DoesNotContain("@", items[3].SupplierEmail,     StringComparison.Ordinal);
        Assert.Null(items[3].ProductInt.Code.SerialNumber);
        Assert.Null(items[3].ProductString.Code.SerialNumber);
    }

    private class TestProduct : Product<int>
    {
        public Exception Error { get; set; } = null!;

        public IEnumerable<string> GetNotes()
        {
            return Notes;
        }
    }

    private class ProductGeneratorOverride : AutoGeneratorOverride
    {
        public override bool CanOverride(AutoGenerateContext context)
        {
            return context.GenerateType.IsGenericType &&
                   context.GenerateType.GetGenericTypeDefinition() == typeof(Product<>);
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            // Get the code and apply a serial number value
            var serialNumber         = AutoFaker.Generate<string>();
            var codeProperty         = context.GenerateType.GetProperty("Code")!;
            var codeInstance         = codeProperty.GetValue(context.Instance);
            var serialNumberProperty = codeProperty.PropertyType.GetProperty("SerialNumber")!;

            serialNumberProperty.SetValue(codeInstance, serialNumber);
        }
    }

    private class ProductCodeOverride : AutoGeneratorOverride
    {
        public string Code { get; set; } = AutoFaker.Generate<string>();

        public override bool CanOverride(AutoGenerateContext context)
        {
            return context.GenerateType.IsGenericType &&
                   context.GenerateType.GetGenericTypeDefinition() == typeof(Product<>);
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            var type                = typeof(ProductCode);
            var productCodeProperty = context.GenerateType.GetProperty("Code")!;
            var productCode         = productCodeProperty.GetValue(context.Instance);
            var serialNoProperty    = type.GetProperty("SerialNumber")!;

            serialNoProperty.SetValue(productCode, Code);
        }
    }
}
