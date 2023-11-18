using AutoBogus.Conventions;
using AutoBogus.Playground.Model;
using FluentAssertions;
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

    protected ServiceFixture(ITestOutputHelper output, IAutoBinder? binder)
    {
        _faker = AutoFaker.Create(builder =>
        {
            builder.WithOverride(new ProductGeneratorOverride());

            if (binder != null)
            {
                builder.WithBinder(binder);
            }
        });

        // Setup
        var id = _faker.Generate<Guid>();
        var generator = new AutoFaker<Item>(null, binder)
            .RuleFor(item => item.Id,   () => id)
            .RuleFor(item => item.Name, faker => faker.Person.FullName)
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
        _item.Timestamp.Should().NotBeNull();
    }

    [Fact]
    public void Should_Set_Product_Notes()
    {
        var product = AutoFaker.Generate<TestProduct>();
        product.GetNotes().Should().NotBeEmpty();
    }

    [Fact]
    public void Should_Not_Set_Product_Code()
    {
        var product = AutoFaker.Generate<TestProduct>(builder =>
        {
            builder.WithSkip<Exception>();
        });

        product.Error.Should().BeNull();
    }

    [Fact]
    public void Should_Not_Set_Product_Notes()
    {
        var product = AutoFaker.Generate<TestProduct>(builder =>
        {
            builder.WithSkip<TestProduct>("Notes");
        });

        product.GetNotes().Should().BeEmpty();
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
        _service.Get(_item.Id).Should().Be(_item);
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
        _service.GetAll().Should().BeSameAs(_items);
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
        var item          = AutoFaker.Generate<Item>();
        var item1Override = new ProductCodeOverride();
        var item2Override = new ProductCodeOverride();
        var items = new List<Item>
        {
            AutoFaker.Generate<Item>(builder => builder.WithSkip<Item>(i => i.ProcessedBy)),
            AutoFaker.Generate<Item>(builder => builder.WithConventions(c => c.Email.Aliases("SupplierEmail"))),
        };

        item.Status = ItemStatus.Pending;
        items.Add(item);

        _repository.GetFiltered(Service.PendingFilter).Returns(items);

        _service.GetPending().Should().BeSameAs(items);

        items[0].ProcessedBy.Email.Should().NotContain("@");
        items[0].SupplierEmail.Should().NotContain("@");
        items[0].ProductInt.Code.SerialNumber.Should().Be(item1Override.Code);
        items[0].ProductString.Code.SerialNumber.Should().Be(item1Override.Code);

        items[1].ProcessedBy.Email.Should().NotContain("@");
        items[1].SupplierEmail.Should().NotContain("@");
        items[1].ProductInt.Code.SerialNumber.Should().Be(item2Override.Code);
        items[1].ProductString.Code.SerialNumber.Should().Be(item2Override.Code);

        items[2].ProcessedBy.Should().BeNull();
        items[2].SupplierEmail.Should().NotContain("@");
        items[2].ProductInt.Code.SerialNumber.Should().BeNull();
        items[2].ProductString.Code.SerialNumber.Should().BeNull();

        items[3].ProcessedBy.Email.Should().Contain("@");
        items[3].SupplierEmail.Should().Contain("@");
        items[3].ProductInt.Code.SerialNumber.Should().BeNull();
        items[3].ProductString.Code.SerialNumber.Should().BeNull();

        items[4].ProcessedBy.Email.Should().NotContain("@");
        items[4].SupplierEmail.Should().NotContain("@");
        items[4].ProductInt.Code.SerialNumber.Should().BeNull();
        items[4].ProductString.Code.SerialNumber.Should().BeNull();
    }

    private class TestProduct
        : Product<int>
    {
        public Exception Error { get; set; } = null!;

        public IEnumerable<string> GetNotes()
        {
            return Notes;
        }
    }

    private class ProductGeneratorOverride
        : AutoGeneratorOverride
    {
        public override bool CanOverride(AutoGenerateContext context)
        {
            return context.GenerateType!.IsGenericType &&
                   context.GenerateType.GetGenericTypeDefinition() == typeof(Product<>);
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            // Get the code and apply a serial number value
            var serialNumber         = AutoFaker.Generate<string>();
            var codeProperty         = context.GenerateType!.GetProperty("Code")!;
            var codeInstance         = codeProperty.GetValue(context.Instance);
            var serialNumberProperty = codeProperty.PropertyType.GetProperty("SerialNumber")!;

            serialNumberProperty.SetValue(codeInstance, serialNumber);
        }
    }

    private class ProductCodeOverride
        : AutoGeneratorOverride
    {
        public string Code { get; set; } = AutoFaker.Generate<string>();

        public override bool CanOverride(AutoGenerateContext context)
        {
            return context.GenerateType!.IsGenericType &&
                   context.GenerateType.GetGenericTypeDefinition() == typeof(Product<>);
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            var type                = typeof(ProductCode);
            var productCodeProperty = context.GenerateType!.GetProperty("Code")!;
            var productCode         = productCodeProperty.GetValue(context.Instance);
            var serialNoProperty    = type.GetProperty("SerialNumber")!;

            serialNoProperty.SetValue(productCode, Code);
        }
    }
}
