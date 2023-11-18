using System.Data;
using System.Diagnostics.CodeAnalysis;
using AutoBogus.Generators;
using Xunit;
using Xunit.Sdk;

namespace AutoBogus.Tests;

public partial class AutoGeneratorsFixture
{
    internal static Type? ResolveType(string fullTypeName, bool throwOnError)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetType(fullTypeName) is Type type)
            {
                return type;
            }
        }

        if (throwOnError)
        {
            throw new XunitException($"Unable to resolve type: {fullTypeName}");
        }

        return null;
    }

    [SuppressMessage("Design", "CA1024:Use properties where appropriate")]
    public class DataSetGeneratorFacet
        : AutoGeneratorsFixture
    {
        public static IEnumerable<object[]> GetTryCreateGeneratorTestCases()
        {
            yield return new object[] { typeof(DataSet), true, };
            yield return new object[] { typeof(TypedDataSet), true, };
            yield return new object[] { typeof(object), false, };
        }

        public static IEnumerable<object[]> GetGenerateTestCases()
        {
            yield return new object[] { typeof(DataSet), };
            yield return new object[] { typeof(TypedDataSet), };
            yield return new object[] { typeof(TypedDataSetWithRelations), };
            yield return new object[] { typeof(TypedDataSetWithSelfReferencingTable), };
        }

        [Theory]
        [MemberData(nameof(GetTryCreateGeneratorTestCases))]
        public void TryCreateGenerator_Should_Create_Generator(Type dataSetType, bool shouldSucceed)
        {
            // Act
            var success = DataSetGenerator.TryCreateGenerator(dataSetType, out var generator);

            // Assert
            if (shouldSucceed)
            {
                Assert.True(success);
                Assert.NotNull(generator);
            }
            else
            {
                Assert.False(success);
            }
        }

        [SkippableTheory]
        [MemberData(nameof(GetGenerateTestCases))]
        public void Generate_Should_Return_DataSet(Type dataSetType)
        {
            ArgumentNullException.ThrowIfNull(dataSetType);

            // Arrange
            var context = CreateContext(dataSetType);

            var success = DataSetGenerator.TryCreateGenerator(context.GenerateType!, out var generator);

            Skip.IfNot(success, $"couldn't create generator for {dataSetType.Name}");

            // Act
            var result = generator!.Generate(context);

            // Assert
            Assert.IsType(dataSetType, result);

            var dataSet = (DataSet)result;

            Assert.NotEmpty(dataSet.Tables);

            foreach (var table in dataSet.Tables.OfType<DataTable>())
            {
                Assert.NotEmpty(table.Columns);
                Assert.NotEmpty(table.Rows);
            }
        }

        [SkippableTheory]
        [MemberData(nameof(GetGenerateTestCases))]
        public void Generate_Should_Return_DataSet_With_Specified_DataTable_Row_Counts(Type dataSetType)
        {
            ArgumentNullException.ThrowIfNull(dataSetType);

            // Arrange
            var rowCountByTable = new Dictionary<DataTable, int>();

            Func<AutoGenerateContext, int> rowCountFunctor =
                ctx =>
                {
                    var dataTable = (DataTable)ctx.Instance!;

                    if (!rowCountByTable.TryGetValue(dataTable, out var count))
                    {
                        // Because Table2.RecordID is a Primary Key and this Unique, Table2
                        // must not have more records than Table1 otherwise it is impossible
                        // to have unique values. So, assuming that the dependent tables
                        // come last in the DataSet, we have a decreasing count as we progress
                        // through the list.
                        count = 100 - ((rowCountByTable.Count + 1) * 10);

                        rowCountByTable[dataTable] = count;
                    }

                    return count;
                };

            var context = CreateContext(dataSetType, dataTableRowCountFunctor: rowCountFunctor);

            var success = DataSetGenerator.TryCreateGenerator(context.GenerateType!, out var generator);

            Skip.IfNot(success, $"couldn't create generator for {dataSetType.Name}");

            // Act
            var result = generator!.Generate(context);

            // Assert
            Assert.IsType(dataSetType, result);

            var dataSet = (DataSet)result;

            Assert.NotEmpty(dataSet.Tables);

            foreach (var table in dataSet.Tables.OfType<DataTable>())
            {
                Assert.NotEmpty(table.Columns);

                Assert.Equal(rowCountByTable[table], table.Rows.Count);
            }
        }

        [SkippableFact]
        public void Generate_Should_Fail_If_Requested_Row_Count_Is_Impossible_Due_To_Foreign_Key_Constraint()
        {
            // Arrange
            var dataSetType = typeof(TypedDataSetWithRelations);

            var rowCountByTable = new Dictionary<DataTable, int>();

            Func<AutoGenerateContext, int> rowCountFunctor =
                ctx =>
                {
                    var dataTable = (DataTable)ctx.Instance!;

                    if (!rowCountByTable.TryGetValue(dataTable, out var count))
                    {
                        // Because Table2.RecordID is a Primary Key and this Unique, Table2
                        // must not have more records than Table1 otherwise it is impossible
                        // to have unique values. So, assuming that the dependent tables
                        // come last in the DataSet, having an increasing count creates an
                        // impossible situation where there aren't enough related records
                        // to produce unique values.
                        count = 100 + ((rowCountByTable.Count + 1) * 10);

                        rowCountByTable[dataTable] = count;
                    }

                    return count;
                };

            var context = CreateContext(dataSetType, dataTableRowCountFunctor: rowCountFunctor);

            var success = DataSetGenerator.TryCreateGenerator(context.GenerateType!, out var generator);

            Skip.IfNot(success, $"couldn't create generator for {dataSetType.Name}");

            // Act
            Action action = () => generator!.Generate(context);

            // Assert
            Assert.Throws<ArgumentException>(action);
        }

        internal class TypedDataSet : DataSet
        {
            public TypedDataSet()
            {
                Tables.Add(new DataTableGeneratorFacet.TypedDataTable1());
                Tables.Add(new DataTableGeneratorFacet.TypedDataTable2());
            }
        }

        internal class TypedDataSetWithRelations : DataSet
        {
            public TypedDataSetWithRelations()
            {
                var table1 = new DataTableGeneratorFacet.TypedDataTable1();
                var table2 = new DataTableGeneratorFacet.TypedDataTable2();
                var table3 = new DataTableGeneratorFacet.TypedDataTable3();

                Tables.Add(table3);
                Tables.Add(table2);
                Tables.Add(table1);

                Relations.Add(new DataRelation(
                    "Relation1", table1.Columns["RecordID"]!, table2.Columns["RecordID"]!, true));
                Relations.Add(new DataRelation(
                    "Relation2", table2.Columns["IntColumn"]!, table3.Columns["ParentIntColumn"]!, true));
            }
        }

        internal class TypedDataSetWithSelfReferencingTable : DataSet
        {
            public TypedDataSetWithSelfReferencingTable()
            {
                var table = new DataTableGeneratorFacet.TypedDataTable3();

                Tables.Add(table);

                Relations.Add(
                    new DataRelation("Relation", table.Columns["RecordID"]!, table.Columns["ParentIntColumn"]!, true));
            }
        }
    }

    [SuppressMessage("Design", "CA1024:Use properties where appropriate")]
    public class DataTableGeneratorFacet
        : AutoGeneratorsFixture
    {
        public static IEnumerable<object[]> GetTryCreateGeneratorTestCases()
        {
            yield return new object[] { typeof(DataTable), true, };
            yield return new object[] { typeof(TypedDataTable1), true, };
            yield return new object[] { typeof(TypedDataTable2), true, };
            yield return new object[] { typeof(object), false, };
        }

        public static IEnumerable<object[]> GetGenerateTestCases()
        {
            yield return new object[] { typeof(DataTable), };
            yield return new object[] { typeof(TypedDataTable1), };
            yield return new object[] { typeof(TypedDataTable2), };
        }

        [Theory]
        [MemberData(nameof(GetTryCreateGeneratorTestCases))]
        public void TryCreateGenerator_Should_Create_Generator(Type dataTableType, bool shouldSucceed)
        {
            // Act
            var success = DataTableGenerator.TryCreateGenerator(dataTableType, out var generator);

            // Assert
            if (shouldSucceed)
            {
                Assert.True(success);
                Assert.NotNull(generator);
            }
            else
            {
                Assert.False(success);
            }
        }

        [SkippableTheory]
        [MemberData(nameof(GetGenerateTestCases))]
        public void Generate_Should_Return_DataTable(Type dataTableType)
        {
            ArgumentNullException.ThrowIfNull(dataTableType);

            // Arrange
            var context = CreateContext(dataTableType);

            var success = DataTableGenerator.TryCreateGenerator(context.GenerateType!, out var generator);

            Skip.IfNot(success, $"couldn't create generator for {dataTableType.Name}");

            // Act
            var result = generator!.Generate(context);

            // Assert
            Assert.IsType(dataTableType, result);

            var dataTable = (DataTable)result;

            Assert.NotEmpty(dataTable.Columns);
            Assert.NotEmpty(dataTable.Rows);
        }

        [SkippableTheory]
        [MemberData(nameof(GetGenerateTestCases))]
        public void Generate_Should_Return_DataTable_With_Specified_Row_Count(Type dataTableType)
        {
            ArgumentNullException.ThrowIfNull(dataTableType);

            // Arrange
            const int RowCount = 100;

            Func<AutoGenerateContext, int> rowCountFunctor = _ => RowCount;

            var context = CreateContext(dataTableType, dataTableRowCountFunctor: rowCountFunctor);

            var success = DataTableGenerator.TryCreateGenerator(context.GenerateType!, out var generator);

            Skip.IfNot(success, $"couldn't create generator for {dataTableType.Name}");

            // Act
            var result = generator!.Generate(context);

            // Assert
            Assert.IsType(dataTableType, result);

            var dataTable = (DataTable)result;

            Assert.NotEmpty(dataTable.Columns);
            Assert.Equal(RowCount, dataTable.Rows.Count);
        }

        internal class TypedDataTable1 : TypedTableBase<TypedDataRow1>
        {
            public TypedDataTable1()
            {
                TableName = "TypedDataTable1";

                Columns.Add(new DataColumn("RecordID",            typeof(int)));
                Columns.Add(new DataColumn("BoolColumn",          typeof(bool)));
                Columns.Add(new DataColumn("CharColumn",          typeof(char)));
                Columns.Add(new DataColumn("SignedByteColumn",    typeof(sbyte)));
                Columns.Add(new DataColumn("ByteColumn",          typeof(byte)));
                Columns.Add(new DataColumn("ShortColumn",         typeof(short)));
                Columns.Add(new DataColumn("UnsignedShortColumn", typeof(ushort)));
                Columns.Add(new DataColumn("IntColumn",           typeof(int)));
                Columns.Add(new DataColumn("GuidColumn",          typeof(Guid)));

                PrimaryKey = new[] { Columns[0], };
            }
        }

        internal class TypedDataRow1 : DataRow
        {
            public TypedDataRow1(DataRowBuilder builder)
                : base(builder)
            {
            }
        }

        internal class TypedDataTable2 : TypedTableBase<TypedDataRow2>
        {
            public TypedDataTable2()
            {
                TableName = "TypedDataTable2";

                Columns.Add(new DataColumn("RecordID",             typeof(int)));
                Columns.Add(new DataColumn("IntColumn",            typeof(int)));
                Columns.Add(new DataColumn("UnsignedIntColumn",    typeof(uint)));
                Columns.Add(new DataColumn("LongColumn",           typeof(long)));
                Columns.Add(new DataColumn("UnsignedLongColumn",   typeof(ulong)));
                Columns.Add(new DataColumn("SingleColumn",         typeof(float)));
                Columns.Add(new DataColumn("DoubleColumn",         typeof(double)));
                Columns.Add(new DataColumn("DecimalColumn",        typeof(decimal)));
                Columns.Add(new DataColumn("DateTimeColumn",       typeof(DateTime)));
                Columns.Add(new DataColumn("DateTimeOffsetColumn", typeof(DateTimeOffset)));
                Columns.Add(new DataColumn("TimeSpanColumn",       typeof(TimeSpan)));
                Columns.Add(new DataColumn("StringColumn",         typeof(string)));

                PrimaryKey = new[] { Columns[0], };
            }
        }

        internal class TypedDataRow2 : DataRow
        {
            public TypedDataRow2(DataRowBuilder builder)
                : base(builder)
            {
            }
        }

        internal class TypedDataTable3 : TypedTableBase<TypedDataRow3>
        {
            public TypedDataTable3()
            {
                TableName = "TypedDataTable3";

                Columns.Add(new DataColumn("RecordID",        typeof(int)));
                Columns.Add(new DataColumn("ParentIntColumn", typeof(int)));
                Columns.Add(new DataColumn("Value",           typeof(string)));
            }
        }

        internal class TypedDataRow3 : DataRow
        {
            public TypedDataRow3(DataRowBuilder builder)
                : base(builder)
            {
            }
        }
    }
}
