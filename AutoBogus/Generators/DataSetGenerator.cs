using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace AutoBogus.Generators;

[SuppressMessage(
    "IDisposableAnalyzers.Correctness",
    "IDISP005:Return type should indicate that the value should be disposed")]
internal abstract class DataSetGenerator
    : IAutoGenerator
{
    public static bool TryCreateGenerator(Type dataSetType, [NotNullWhen(true)] out DataSetGenerator? generator)
    {
        generator = default;

        if (dataSetType == typeof(DataSet))
        {
            generator = new UntypedDataSetGenerator();
        }
        else if (typeof(DataSet).IsAssignableFrom(dataSetType))
        {
            generator = new TypedDataSetGenerator(dataSetType);
        }

        return generator != null;
    }

    public abstract object Generate(AutoGenerateContext context);

    private sealed class UntypedDataSetGenerator : DataSetGenerator
    {
        public override object Generate(AutoGenerateContext context)
        {
            var dataSet = new DataSet();

            if (!DataTableGenerator.TryCreateGenerator(typeof(DataTable), out var tableGenerator))
            {
                throw new InvalidOperationException("Internal error: Couldn't create generator for DataTable");
            }

            for (var tableCount = context.Faker.Random.Int(2, 6); tableCount > 0; tableCount--)
            {
                dataSet.Tables.Add((DataTable)tableGenerator.Generate(context));
            }

            return dataSet;
        }
    }

    private sealed class TypedDataSetGenerator : DataSetGenerator
    {
        private readonly Type _dataSetType;

        public TypedDataSetGenerator(Type dataSetType)
        {
            _dataSetType = dataSetType;
        }

        public override object Generate(AutoGenerateContext context)
        {
            var dataSet = (DataSet)Activator.CreateInstance(_dataSetType)!;

            var allTables       = dataSet.Tables.OfType<DataTable>().ToList();
            var populatedTables = new HashSet<DataTable>();

            while (allTables.Count > 0)
            {
                var madeProgress = false;

                for (var i = 0; i < allTables.Count; i++)
                {
                    var table = allTables[i];

                    var referencedTables = table.Constraints
                        .OfType<ForeignKeyConstraint>()
                        .Select(constraint => constraint.RelatedTable);

                    if (!referencedTables.Where(referencedTable => referencedTable != table)
                            .All(populatedTables.Contains))
                    {
                        continue;
                    }

                    if (!DataTableGenerator.TryCreateGenerator(table.GetType(), out var tableGenerator))
                    {
                        throw new InvalidOperationException(
                            $"Couldn't create generator for typed table type {table.GetType()}");
                    }

                    populatedTables.Add(table);

                    context.Instance = table;

                    tableGenerator.PopulateRows(table, context);

                    madeProgress = true;

                    allTables.RemoveAt(i);
                    i--;
                }

                if (!madeProgress)
                {
                    throw new InvalidOperationException(
                        "Couldn't generate data for all tables in data set because there are constraints that can't be satisfied");
                }
            }

            return dataSet;
        }
    }
}
