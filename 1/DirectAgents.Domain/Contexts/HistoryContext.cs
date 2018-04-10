using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;

//namespace DirectAgents.Domain.Contexts
//{
//    public class MyConfiguration : DbConfiguration
//    {
//        public MyConfiguration()
//        {
//            this.SetHistoryContext("System.Data.SqlClient",
//                (connection, defaultSchema) => new MyHistoryContext(connection, defaultSchema));
//        }
//    }

//    public class MyHistoryContext : HistoryContext
//    {
//        public MyHistoryContext(DbConnection dbConnection, string defaultSchema)
//            : base(dbConnection, defaultSchema)
//        {
//        }

//        protected override void OnModelCreating(DbModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);
//            modelBuilder.Entity<HistoryRow>().ToTable("__MigrationHistory", "dbo");
//        }
//    }
//}
