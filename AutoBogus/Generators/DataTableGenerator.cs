using System.Data;
using System.Diagnostics.CodeAnalysis;
using AutoBogus.Internal;

namespace AutoBogus.Generators;

internal abstract class DataTableGenerator : IAutoGenerator
{
    public static bool IsTypedDataTableType(Type? type, [NotNullWhen(true)] out Type? rowType)
    {
        rowType = default;

        while (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(TypedTableBase<>))
            {
                rowType = type.GetGenericArguments()[0];
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    public static bool TryCreateGenerator(Type tableType, [NotNullWhen(true)] out DataTableGenerator? generator)
    {
        generator = default;

        if (tableType == typeof(DataTable))
        {
            generator = new UntypedDataTableGenerator();
        }
        else if (IsTypedDataTableType(tableType, out var rowType))
        {
            var generatorType = typeof(TypedDataTableGenerator<,>).MakeGenericType(tableType, rowType);
            generator = (DataTableGenerator)Activator.CreateInstance(generatorType)!;
        }

        return generator != null;
    }

    public object Generate(AutoGenerateContext context)
    {
        var table = CreateTable(context);
        context.Instance = table;
        PopulateRows(table, context);
        return table;
    }

    public void PopulateRows(DataTable table, AutoGenerateContext context)
    {
        var rowCountIsSpecified = false;

        var rowCount = -1;

        if (context.Config.DataTableRowCount != null)
        {
            rowCountIsSpecified = true;
            rowCount            = context.Config.DataTableRowCount(context);
        }

        if (rowCount < 0)
        {
            rowCount = context.Faker.Random.Number(1, 20);
        }

        var constrainedColumns         = new Dictionary<DataColumn, ConstrainedColumnInformation>();
        var constraintHasUniqueColumns = new HashSet<ForeignKeyConstraint>();
        var referencedRowByConstraint  = new Dictionary<ForeignKeyConstraint, DataRow?>();

        foreach (var foreignKey in table.Constraints.OfType<ForeignKeyConstraint>())
        {
            var containsUniqueColumns = Array.Exists(foreignKey.Columns, col =>
                col.Unique
                || table.Constraints.OfType<UniqueConstraint>().Any(constraint => constraint.Columns.Contains(col)));

            for (var i = 0; i < foreignKey.Columns.Length; i++)
            {
                var column        = foreignKey.Columns[i];
                var relatedColumn = foreignKey.RelatedColumns[i];

                if (constrainedColumns.ContainsKey(column))
                {
                    throw new InvalidOperationException(
                        $"Column is constrained in multiple foreign key relationships simultaneously: {column.ColumnName} in DataTable {table.TableName}");
                }

                constrainedColumns[column] =
                    new ConstrainedColumnInformation
                    {
                        Constraint    = foreignKey,
                        RelatedColumn = relatedColumn,
                    };
            }

            if (foreignKey.RelatedTable == table && Array.Exists(foreignKey.Columns, col => !col.AllowDBNull))
            {
                throw new InvalidOperationException(
                    $"Self-reference columns must be nullable so that at least one record can be added when the table is initially empty: DataTable {table.TableName}");
            }

            if (containsUniqueColumns)
            {
                constraintHasUniqueColumns.Add(foreignKey);
            }

            // Prepare a slot to be filled per-row.
            referencedRowByConstraint[foreignKey] = null;

            if (containsUniqueColumns
                && foreignKey.RelatedTable != table
                && foreignKey.RelatedTable.Rows.Count < rowCount)
            {
                if (rowCountIsSpecified)
                {
                    var remoteSubject = foreignKey.RelatedTable.TableName;

                    if (string.IsNullOrEmpty(remoteSubject))
                    {
                        remoteSubject = "another DataTable";
                    }

                    throw new ArgumentException(
                        $"Unable to satisfy the requested row count of {rowCount} because this table has a foreign key constraint on {remoteSubject} that must be unique, and that table only has {foreignKey.RelatedTable.Rows.Count} row(s).");
                }

                rowCount = foreignKey.RelatedTable.Rows.Count;
            }
        }

        var allConstraints = referencedRowByConstraint.Keys.ToList();

        while (rowCount > 0)
        {
            var rowIndex = table.Rows.Count;

            foreach (var foreignKey in allConstraints)
            {
                if (constraintHasUniqueColumns.Contains(foreignKey))
                {
                    referencedRowByConstraint[foreignKey] = foreignKey.RelatedTable.Rows[rowIndex];
                }
                else if (foreignKey.RelatedTable.Rows.Count == 0)
                {
                    referencedRowByConstraint[foreignKey] = null;
                }
                else
                {
                    var randomRowIndex = context.Faker.Random.Number(0, foreignKey.RelatedTable.Rows.Count - 1);
                    referencedRowByConstraint[foreignKey] = foreignKey.RelatedTable.Rows[randomRowIndex];
                }
            }

            var columnValues = new object?[table.Columns.Count];

            for (var i = 0; i < table.Columns.Count; i++)
            {
                if (constrainedColumns.TryGetValue(table.Columns[i], out var constraintInfo))
                {
                    columnValues[i] =
                        referencedRowByConstraint[constraintInfo.Constraint]?[constraintInfo.RelatedColumn] ??
                        DBNull.Value;
                }
                else
                {
                    columnValues[i] = GenerateColumnValue(table.Columns[i], context);
                }
            }

            table.Rows.Add(columnValues);

            rowCount--;
        }
    }

    protected abstract DataTable CreateTable(AutoGenerateContext context);

    private object? GenerateColumnValue(DataColumn dataColumn, AutoGenerateContext context)
    {
        var typeCode = Type.GetTypeCode(dataColumn.DataType);
        if (typeCode == TypeCode.Object)
        {
            if (dataColumn.DataType == typeof(TimeSpan))
            {
                var randomDate1 = context.Faker.Date.Future();
                var randomDate2 = context.Faker.Date.Future();
                return randomDate1 - randomDate2;
            }

            if (dataColumn.DataType == typeof(Guid))
            {
                return context.Faker.Random.Guid();
            }

            var proxy = (Proxy)Activator.CreateInstance(typeof(Proxy<>).MakeGenericType(dataColumn.DataType))!;

            return proxy.Generate(context);
        }

        object? value = typeCode switch
        {
            TypeCode.Empty    => null,
            TypeCode.DBNull   => null,
            TypeCode.Boolean  => context.Faker.Random.Bool(),
            TypeCode.Char     => context.Faker.Lorem.Letter().Single(),
            TypeCode.SByte    => context.Faker.Random.SByte(),
            TypeCode.Byte     => context.Faker.Random.Byte(),
            TypeCode.Int16    => context.Faker.Random.Short(),
            TypeCode.UInt16   => context.Faker.Random.UShort(),
            TypeCode.UInt32   => context.Faker.Random.UInt(),
            TypeCode.Int64    => context.Faker.Random.Long(),
            TypeCode.UInt64   => context.Faker.Random.ULong(),
            TypeCode.Single   => context.Faker.Random.Float(),
            TypeCode.Double   => context.Faker.Random.Double(),
            TypeCode.Decimal  => context.Faker.Random.Decimal(),
            TypeCode.DateTime => context.Faker.Date.Between(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(+30)),
            TypeCode.String   => context.Faker.Lorem.Lines(1),
            TypeCode.Int32 =>
                dataColumn.ColumnName.EndsWith("ID", StringComparison.OrdinalIgnoreCase)
                    ? Interlocked.Increment(ref context.Faker.IndexFaker)
                    : context.Faker.Random.Int(),
            _ => null,
        };

        return value;
    }

    private sealed class ConstrainedColumnInformation
    {
        public ForeignKeyConstraint Constraint    { get; set; } = null!;
        public DataColumn           RelatedColumn { get; set; } = null!;
    }

    private abstract class Proxy
    {
        public abstract object? Generate(AutoGenerateContext context);
    }

    private sealed class Proxy<T> : Proxy
    {
        public override object? Generate(AutoGenerateContext context)
        {
            return context.Generate<T>();
        }
    }

    private sealed class UntypedDataTableGenerator
        : DataTableGenerator
    {
        protected override DataTable CreateTable(AutoGenerateContext context)
        {
            var table = new DataTable();

            for (var i = context.Faker.Random.Int(3, 10); i >= 0; i--)
            {
                table.Columns.Add(
                    new DataColumn
                    {
                        ColumnName = context.Faker.Database.Column() + i,
                        DataType = Type.GetType("System." + context.Faker.PickRandom(
                            ((TypeCode[])Enum.GetValues(typeof(TypeCode)))
                            .Except(new[] { TypeCode.Empty, TypeCode.Object, TypeCode.DBNull, })
                            .ToArray())),
                    });
            }

            return table;
        }
    }

    [SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed")]
    private sealed class TypedDataTableGenerator<TTable, TRow>
        : DataTableGenerator
        where TTable : DataTable, new()
    {
        protected override DataTable CreateTable(AutoGenerateContext context)
        {
            return new TTable();
        }
    }
}
