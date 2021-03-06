﻿using Apps72.Dev.Data;
using Apps72.Dev.Data.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Oracle.Tests
{
    /// <summary>
    /// Need to install http://www.oracle.com/technetwork/database/features/instant-client/index-097480.html
    /// </summary>
    [TestClass]
    public class OracleDatabaseCommandTests
    {
        #region INITIALIZATION

        public static readonly string CONNECTION_STRING = System.Configuration.ConfigurationManager.ConnectionStrings["Scott"].ConnectionString;
        private OracleConnection _connection;

        [TestInitialize]
        public void Initialization()
        {
            _connection = new OracleConnection(CONNECTION_STRING);
            _connection.Open();
        }

        #endregion

        #region TEMPORARY CONNECTION

        //[TestMethod]
        //public void OpenTemporaryConnection_Test()
        //{
        //    using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(CONNECTION_STRING, string.Empty, -1))
        //    {
        //        cmd.Log = (message) =>
        //        {
        //            Console.WriteLine(message);
        //        };

        //        cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
        //        DataTable data = cmd.ExecuteTable();

        //        Assert.AreEqual(14, data.Rows.Count);
        //        Assert.AreEqual(8, data.Columns.Count);
        //    }
        //}

        #endregion

        #region EXECUTE METHODS

        //[TestMethod]
        //public void ExecuteTable_Test()
        //{
        //    using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
        //    {
        //        cmd.Log = Console.WriteLine;
        //        cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
        //        DataTable data = cmd.ExecuteTable();

        //        Assert.AreEqual(14, data.Rows.Count);
        //        Assert.AreEqual(8, data.Columns.Count);
        //    }
        //}

        //[TestMethod]
        //public void ExecuteRow_Test()
        //{
        //    using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
        //    {
        //        cmd.Log = Console.WriteLine;
        //        cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
        //        DataRow row = cmd.ExecuteRow();

        //        Assert.AreEqual(8, row.ItemArray.Length);
        //    }
        //}

        [TestMethod]
        public void ExecuteRowTyped_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT * FROM EMP WHERE EMPNO = 7369");
                EMP emp = cmd.ExecuteRow<EMP>();

                Assert.AreEqual(7369, emp.EmpNo);
            }
        }

        [TestMethod]
        public void ExecuteRowPrimitive_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO FROM EMP WHERE EMPNO = 7369");
                short empno = cmd.ExecuteRow<short>();

                Assert.AreEqual(7369, empno);
            }
        }

        [TestMethod]
        public void ExecuteRowPrimitiveNullable_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT COMM FROM EMP WHERE EMPNO = 7369");
                int? comm = cmd.ExecuteRow<int?>();

                Assert.AreEqual(null, comm);
            }
        }

        //[TestMethod]
        //public void ExecuteRowWithDelegate_Test()
        //{
        //    using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
        //    {
        //        cmd.Log = Console.WriteLine;
        //        cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
        //        EMP emp = cmd.ExecuteRow<EMP>((row) => new EMP() { EmpNo = Convert.ToInt32(row["EMPNO"]) });

        //        Assert.AreEqual(7369, emp.EmpNo);
        //    }
        //}

        [TestMethod]
        public void ExecuteScalar_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP ");
                object data = cmd.ExecuteScalar();

                Assert.AreEqual(14m, data);     // Decimal
            }
        }

        [TestMethod]
        public void ExecuteScalarWithParameter_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT ENAME ")
                               .AppendLine("  FROM EMP ")
                               .AppendLine(" WHERE EMPNO = :EmpNo ")
                               .AppendLine("   AND HIREDATE = :HireDate ")
                               .AppendLine("   AND JOB = :Job ");

                cmd.Parameters.AddWithValue(":EMPNO", 7369);                            // Parameter in Upper Case
                cmd.Parameters.AddWithValue("HireDate", new DateTime(1980, 12, 17));    // Parameter without :
                cmd.Parameters.AddWithValue(":Job", "CLERK");                           // Parameter in normal mode

                object data = cmd.ExecuteScalar();

                Assert.AreEqual("SMITH", data);
            }
        }

        [TestMethod]
        public void ExecuteScalarWithAnonymousParameters_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT ENAME ")
                               .AppendLine("  FROM EMP ")
                               .AppendLine(" WHERE EMPNO = :EmpNo ")
                               .AppendLine("   AND HIREDATE = :HireDate ")
                               .AppendLine("   AND 1 = :NotDeleted ")
                               .AppendLine("   AND JOB = :Job ");

                // Parameters order is invariant if Command.BindByName = true
                cmd.Parameters.AddWithValue(":EMPNO", 1234);                            // Parameter in Upper Case
                cmd.Parameters.AddWithValue("HireDate", new DateTime(1980, 1, 1));      // Parameter without :
                cmd.Parameters.AddWithValue(":Job", "FAKE");                            // Parameter in normal mode
                cmd.Parameters.AddWithValue(":NotDeleted", 1);                          // Parameter not replaced by .AddValues

                // Replace previous values wiht these new propery values
                cmd.Parameters.AddValues(new
                {
                    EmpNo = 7369,
                    HireDate = new DateTime(1980, 12, 17),
                    Job = "CLERK"
                });

                object data = cmd.ExecuteScalar();

                Assert.AreEqual("SMITH", data);
            }
        }

        [TestMethod]
        public void ExecuteScalarWithAnonymousOnlyParameters_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT ENAME ")
                               .AppendLine("  FROM EMP ")
                               .AppendLine(" WHERE EMPNO = :EmpNo ")
                               .AppendLine("   AND HIREDATE = :HireDate ")
                               .AppendLine("   AND JOB = :Job ");

                // Replace previous values wiht these new propery values
                cmd.Parameters.AddValues(new
                {
                    EmpNo = 7369,
                    HireDate = new DateTime(1980, 12, 17),
                    Job = "CLERK"
                });

                object data = cmd.ExecuteScalar();

                Assert.AreEqual("SMITH", data);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExecuteScalarWithSimpleParameters_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT ENAME ")
                               .AppendLine("  FROM EMP ");


                // Simple value are not autorized
                cmd.Parameters.AddValues(123);

                object data = cmd.ExecuteScalar();

                Assert.Fail();
            }
        }

        [TestMethod]
        public void ExecuteScalarTyped_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLineFormat(" SELECT COUNT(*) FROM EMP ");
                var data = cmd.ExecuteScalar<decimal>();

                Assert.AreEqual(14m, data);
            }
        }

        [TestMethod]
        public void ExecuteScalarTypedNull_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLineFormat(" SELECT COMM FROM EMP WHERE EMPNO = {0} ", 7369);
                int? data = cmd.ExecuteScalar<int?>();

                Assert.AreEqual(null, data);
            }
        }

        [TestMethod]
        public void ExecuteTableTyped_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME, HIREDATE, COMM, MGR FROM EMP ");
                EMP[] data = cmd.ExecuteTable<EMP>().ToArray();
                EMP smith = data.FirstOrDefault(i => i.EmpNo == 7369);

                Assert.AreEqual(EMP.Smith.EmpNo, smith.EmpNo);
                Assert.AreEqual(EMP.Smith.EName, smith.EName);
                Assert.AreEqual(EMP.Smith.HireDate, smith.HireDate);
                Assert.AreEqual(EMP.Smith.Comm, smith.Comm);
            }
        }

        //[TestMethod]
        //public void ExecuteTableCustomedTyped_Test()
        //{
        //    using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
        //    {
        //        cmd.Log = Console.WriteLine;
        //        cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME, HIREDATE, COMM, MGR FROM EMP ");
        //        var data = cmd.ExecuteTable<EMP>((row) =>
        //        {
        //            return new EMP()
        //            {
        //                EmpNo = row.Field<int>("EMPNO"),
        //                EName = row.Field<string>("ENAME"),
        //                HireDate = row.Field<DateTime>("HIREDATE"),
        //                Comm = row.Field<int?>("COMM")
        //            };
        //        });
        //        EMP smith = data.FirstOrDefault(i => i.EmpNo == 7369);

        //        Assert.AreEqual(EMP.Smith.EmpNo, smith.EmpNo);
        //        Assert.AreEqual(EMP.Smith.EName, smith.EName);
        //        Assert.AreEqual(EMP.Smith.HireDate, smith.HireDate);
        //        Assert.AreEqual(EMP.Smith.Comm, smith.Comm);
        //    }
        //}

        [TestMethod]
        public void ExecuteTableTypedWithColumnAttribute_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME, SAL, HIREDATE, COMM, MGR FROM EMP ");
                var data = cmd.ExecuteTable<EMP>();
                EMP smith = data.FirstOrDefault(i => i.EmpNo == 7369);

                Assert.AreEqual(smith.EmpNo, EMP.Smith.EmpNo);
                Assert.AreEqual(smith.EName, EMP.Smith.EName);
                Assert.AreEqual(smith.HireDate, EMP.Smith.HireDate);
                Assert.AreEqual(smith.Comm, EMP.Smith.Comm);
                Assert.AreEqual(smith.Salary, EMP.Smith.Salary);
            }
        }

        [TestMethod]
        public void ExecuteNonQuery_Transaction_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" DELETE FROM EMP ");

                cmd.TransactionBegin();
                cmd.ExecuteNonQuery();
                cmd.TransactionRollback();

                Assert.AreEqual(EMP.GetEmployeesCount(_connection), 14);
            }
        }

        [TestMethod]
        public void ExecuteNonQuery_DefineTransactionBefore_Test()
        {
            using (OracleTransaction transaction = _connection.BeginTransaction())
            {
                using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection, transaction))
                {
                    cmd.Log = Console.WriteLine;
                    cmd.CommandText.AppendLine(" INSERT INTO EMP (EMPNO, ENAME) VALUES (1234, 'ABC') ");
                    cmd.ExecuteNonQuery();
                }

                using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection, transaction))
                {
                    cmd.Log = Console.WriteLine;
                    cmd.CommandText.AppendLine(" INSERT INTO EMP (EMPNO, ENAME) VALUES (9876, 'XYZ') ");
                    cmd.ExecuteNonQuery();
                }

                transaction.Rollback();

                Assert.AreEqual(EMP.GetEmployeesCount(_connection, transaction), 14);
            }
        }

        [TestMethod]
        public void ExecuteNonQuery_TransactionForTwoCommands_Test()
        {
            OracleTransaction currentTransaction;

            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" DELETE FROM EMP ");

                currentTransaction = cmd.TransactionBegin();
                cmd.ExecuteNonQuery();

                Assert.AreEqual(EMP.GetEmployeesCount(_connection, currentTransaction), 0);     // Inside the transaction

                cmd.TransactionRollback();

                Assert.AreEqual(EMP.GetEmployeesCount(_connection, currentTransaction), 14);    // Inside the transaction
                Assert.AreEqual(EMP.GetEmployeesCount(_connection), 14);                      // Ouside the transaction
            }
        }

        [TestMethod]
        public void ExecuteNonQuery_TransactionForTwoIncludedCommands_Test()
        {
            using (OracleDatabaseCommand cmd1 = new OracleDatabaseCommand(_connection))
            {
                cmd1.Log = Console.WriteLine;
                cmd1.CommandText.AppendLine(" DELETE FROM EMP ");
                cmd1.TransactionBegin();
                cmd1.ExecuteNonQuery();

                using (OracleDatabaseCommand cmd2 = new OracleDatabaseCommand(_connection, cmd1.Transaction))
                {
                    cmd2.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP ");
                    var count = cmd2.ExecuteScalar<decimal>();
                }

                cmd1.TransactionRollback();
            }
        }

        #endregion

        #region QUERY FORMATTED

        [TestMethod]
        public void GetFormattedAsText_Simple_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.CommandText.Append(" SELECT * FROM EMP ");
                string formatted = cmd.GetCommandTextFormatted(QueryFormat.Text);

                Assert.AreEqual(formatted, " SELECT * FROM EMP ");
            }
        }

        [TestMethod]
        public void GetFormattedAsText_Parameters_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.CommandText.Append(" SELECT * FROM EMP WHERE EMPNO = :EmpNo AND ENAME LIKE :Ename AND HIREDATE > :Hire AND COMM = :Comm ");
                cmd.Parameters.AddWithValue(":EmpNo", 7369);                                    // Parameter normal
                cmd.Parameters.AddWithValue(":ENAME", "%SM%");                                  // Parameter in Upper Case
                cmd.Parameters.AddWithValue("Hire", new DateTime(1970, 05, 04, 14, 15, 16));    // Parameter without :
                cmd.Parameters.AddWithValueOrDBNull(":Comm", null);                             // Parameter NULL

                string formatted = cmd.GetCommandTextFormatted(QueryFormat.Text);

                Assert.AreEqual(formatted, " SELECT * FROM EMP WHERE EMPNO = 7369 AND ENAME LIKE '%SM%' AND HIREDATE > '1970-05-04 14:15:16' AND COMM = NULL ");
            }
        }

        [TestMethod]
        public void GetFormattedAsHtml_Parameters_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
                cmd.CommandText.AppendLine("  WHERE EMPNO = :EmpNo AND ENAME LIKE :Ename AND HIREDATE > :Hire AND COMM = :Comm ");
                cmd.Parameters.AddWithValue(":EmpNo", 7369);                                    // Parameter normal
                cmd.Parameters.AddWithValue(":ENAME", "%SM%");                                  // Parameter in Upper Case
                cmd.Parameters.AddWithValue("Hire", new DateTime(1970, 05, 04, 14, 15, 16));    // Parameter without :
                cmd.Parameters.AddWithValueOrDBNull(":Comm", null);                             // Parameter NULL

                string formatted = cmd.GetCommandTextFormatted(QueryFormat.Html);

                Assert.AreEqual(formatted, @" <span style=""color: #33f; font-weight: bold;"">SELECT</span> * <span style=""color: #33f; font-weight: bold;"">FROM</span> EMP <br/>  <span style=""color: #33f; font-weight: bold;"">WHERE</span> EMPNO = <span style=""color: #FF3F00;"">7369</span> <span style=""color: #33f; font-weight: bold;"">AND</span> ENAME <span style=""color: #33f; font-weight: bold;"">LIKE</span> <span style=""color: #FF3F00;"">'%SM%'</span> <span style=""color: #33f; font-weight: bold;"">AND</span> HIREDATE &gt; <span style=""color: #FF3F00;"">'<span style=""color: #FF3F00;"">1970</span><span style=""color: #FF3F00;"">-05</span><span style=""color: #FF3F00;"">-04</span> <span style=""color: #FF3F00;"">14</span>:<span style=""color: #FF3F00;"">15</span>:<span style=""color: #FF3F00;"">16</span>'</span> <span style=""color: #33f; font-weight: bold;"">AND</span> COMM = <span style=""color: #33f; font-weight: bold;"">NULL</span> <br/>");
            }
        }

        #endregion

        #region EXTENSIONS

        [TestMethod]
        public void Parameter_AddWithValueOrDBNull_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP WHERE COMM = :Comm ");
                cmd.Parameters.AddWithValueOrDBNull(":Comm", null);

                Assert.AreEqual(cmd.ExecuteScalar<decimal>(), 0);
            }
        }

        [TestMethod]
        public void Parameter_ConvertToDBNull_Test()
        {
            using (OracleDatabaseCommand cmd = new OracleDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP WHERE COMM = :Comm ");
                cmd.Parameters.AddWithValue(":Comm", null).ConvertToDBNull();

                Assert.AreEqual(cmd.ExecuteScalar<decimal>(), 0);
            }
        }

        #endregion
    }
}
