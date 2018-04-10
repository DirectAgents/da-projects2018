using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Builders;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.SqlServer;

// From: http://romiller.com/2013/04/30/ef6-switching-identity-onoff-with-a-custom-migration-operation/

namespace ClientPortal.Data.Migrations
{
    public class ChangeIdentityOperation : MigrationOperation
    {
        public ChangeIdentityOperation()
            : base(null)
        { }

        public IdentityChange Change { get; set; }
        public string PrincipalTable { get; set; }
        public string PrincipalColumn { get; set; }
        public List<DependentColumn> DependentColumns { get; set; }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }

    public enum IdentityChange
    {
        SwitchIdentityOn,
        SwitchIdentityOff
    }

    public class DependentColumn
    {
        public string DependentTable { get; set; }
        public string ForeignKeyColumn { get; set; }
    }

    // ---

    public static class MigrationExtensions
    {
        public static IdentityChangeOperationWrapper ChangeIdentity(
            this DbMigration migration,
            IdentityChange change,
            string principalTable,
            string principalColumn)
        {
            var operation = new ChangeIdentityOperation
            {
                Change = change,
                PrincipalTable = principalTable,
                PrincipalColumn = principalColumn,
                DependentColumns = new List<DependentColumn>()
            };

            ((IDbMigration)migration).AddOperation(operation);

            return new IdentityChangeOperationWrapper(operation);
        }

        public class IdentityChangeOperationWrapper
        {
            private ChangeIdentityOperation _operation;

            public IdentityChangeOperationWrapper(ChangeIdentityOperation operation)
            {
                _operation = operation;
            }

            public IdentityChangeOperationWrapper WithDependentColumn(
                string table,
                string foreignKeyColumn)
            {
                _operation.DependentColumns.Add(new DependentColumn
                {
                    DependentTable = table,
                    ForeignKeyColumn = foreignKeyColumn
                });

                return this;
            }
        }
    }

    // ---

    class MySqlServerMigrationSqlGenerator : SqlServerMigrationSqlGenerator
    {
        protected override void Generate(MigrationOperation migrationOperation)
        {
            var operation = migrationOperation as ChangeIdentityOperation;
            if (operation != null)
            {
                var tempPrincipalColumnName = "old_" + operation.PrincipalColumn;

                // 1. Drop all foreign key constraints that point to the primary key we are changing
                foreach (var item in operation.DependentColumns)
                {
                    Generate(new DropForeignKeyOperation
                    {
                        DependentTable = item.DependentTable,
                        PrincipalTable = operation.PrincipalTable,
                        DependentColumns = { item.ForeignKeyColumn }
                    });
                }

                // 2. Drop the primary key constraint
                Generate(new DropPrimaryKeyOperation { Table = operation.PrincipalTable });

                // 3. Rename the existing column (so that we can re-create the foreign key relationships later)
                Generate(new RenameColumnOperation(
                    operation.PrincipalTable,
                    operation.PrincipalColumn,
                    tempPrincipalColumnName));

                // 4. Add the new primary key column with the new identity setting
                Generate(new AddColumnOperation(
                    operation.PrincipalTable,
                    new ColumnBuilder().Int(
                        name: operation.PrincipalColumn,
                        nullable: false,
                        identity: operation.Change == IdentityChange.SwitchIdentityOn)));

                // 5. Update existing data so that previous foreign key relationships remain
                if (operation.Change == IdentityChange.SwitchIdentityOn)
                {
                    // If the new column is an identity column we need to update all 
                    // foreign key columns with the new values
                    foreach (var item in operation.DependentColumns)
                    {
                        Generate(new SqlOperation(
                            "UPDATE " + item.DependentTable +
                            " SET " + item.ForeignKeyColumn +
                                " = (SELECT TOP 1 " + operation.PrincipalColumn +
                                " FROM " + operation.PrincipalTable +
                                " WHERE " + tempPrincipalColumnName + " = " + item.DependentTable + "." + item.ForeignKeyColumn + ")"));
                    }
                }
                else
                {
                    // If the new column doesn’t have identity on then we can copy the old 
                    // values from the previous identity column
                    Generate(new SqlOperation(
                        "UPDATE " + operation.PrincipalTable +
                        " SET " + operation.PrincipalColumn + " = " + tempPrincipalColumnName + ";"));
                }

                // 6. Drop old primary key column
                Generate(new DropColumnOperation(
                    operation.PrincipalTable,
                    tempPrincipalColumnName));

                // 7. Add primary key constraint
                Generate(new AddPrimaryKeyOperation
                {
                    Table = operation.PrincipalTable,
                    Columns = { operation.PrincipalColumn }
                });

                // 8. Add back foreign key constraints
                foreach (var item in operation.DependentColumns)
                {
                    Generate(new AddForeignKeyOperation
                    {
                        DependentTable = item.DependentTable,
                        DependentColumns = { item.ForeignKeyColumn },
                        PrincipalTable = operation.PrincipalTable,
                        PrincipalColumns = { operation.PrincipalColumn }
                    });
                }
            }
        }
    }

}
