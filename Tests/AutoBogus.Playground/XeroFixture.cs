namespace AutoBogus.Playground;

public class XeroFixture
{
    public class TestCreationPerformance
    {
        [Fact]
        public void TestAutoFakerXeroInvoice()
        {
            //previously this took > 45 seconds unless a lot of types were skipped
            var fake = new AutoFaker<Invoice>()
                .Configure(builder =>
                {
                    builder.WithTreeDepth(1);
                });
            var created = fake.Generate();

            created.Should().NotBeNull();
        }
    }
}
