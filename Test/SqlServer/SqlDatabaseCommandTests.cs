﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.SqlClient;
using Apps72.Dev.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Data.Tests
{
    [TestClass]
    public class SqlDatabaseCommandTests
    {
        #region INITIALIZATION

        public static readonly string CONNECTION_STRING = System.Configuration.ConfigurationManager.ConnectionStrings["Scott"].ConnectionString;
        private SqlConnection _connection;

        [TestInitialize]
        public void Initialization()
        {
            _connection = new SqlConnection(CONNECTION_STRING);
            _connection.Open();
        }

        #endregion

        #region TEMPORARY CONNECTION

        [TestMethod]
        public void OpenTemporaryConnection_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(CONNECTION_STRING, string.Empty, -1))
            {
                cmd.Log = (message) =>
                {
                    Console.WriteLine(message);
                };

                cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
                DataTable data = cmd.ExecuteTable();

                Assert.AreEqual(14, data.Rows.Count);
                Assert.AreEqual(8, data.Columns.Count);
            }
        }

        #endregion

        #region EXECUTE METHODS

        [TestMethod]
        public void Execute_Constructor1_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP ");
                int count = cmd.ExecuteScalar<int>();

                Assert.AreEqual(14, count);
            }
        }

        [TestMethod]
        public void Execute_Constructor2_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection, "SELECT COUNT(*) FROM EMP"))
            {
                cmd.Log = Console.WriteLine;
                int count = cmd.ExecuteScalar<int>();

                Assert.AreEqual(14, count);
            }
        }

        [TestMethod]
        public void Execute_Constructor3_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(CONNECTION_STRING, "SELECT COUNT(*) FROM EMP"))
            {
                cmd.Log = Console.WriteLine;
                int count = cmd.ExecuteScalar<int>();

                Assert.AreEqual(14, count);
            }
        }

        [TestMethod]
        public void ExecuteTable_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
                DataTable data = cmd.ExecuteTable();

                Assert.AreEqual(14, data.Rows.Count);
                Assert.AreEqual(8, data.Columns.Count);
            }
        }

        [TestMethod]
        public void ExecuteRow_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
                DataRow row = cmd.ExecuteRow();

                Assert.AreEqual(8, row.ItemArray.Length);
            }
        }

        [TestMethod]
        public void ExecuteRowTyped_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT * FROM EMP WHERE EMPNO = 7369");
                EMP emp = cmd.ExecuteRow<EMP>();

                Assert.AreEqual(7369, emp.EmpNo);
            }
        }

        [TestMethod]
        public void ExecuteRowAnonymousTyped_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME, HIREDATE FROM EMP WHERE EMPNO = 7369");
                var emp = cmd.ExecuteRow(new
                {
                    EmpNo = 0,
                    Ename = string.Empty,
                    HireDate = (DateTime?)null
                });

                Assert.AreEqual(7369, emp.EmpNo);
            }
        }

        [TestMethod]
        public void ExecuteRowDynamic_Test()
        {
            using (var cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME, HIREDATE, COMM FROM EMP WHERE EMPNO = 7369");
                var emp = cmd.ExecuteRow<dynamic>();

                Assert.AreEqual(7369, emp.EMPNO);
                Assert.AreEqual("SMITH", emp.ENAME);
                Assert.AreEqual(new DateTime(1980, 12, 17), emp.HIREDATE);
                Assert.AreEqual(null, emp.COMM);
            }
        }

        [TestMethod]
        public void ExecuteTwoRowsDynamic_Test()
        {
            using (var cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME, HIREDATE, COMM FROM EMP WHERE EMPNO = 7369");
                var emp1 = cmd.ExecuteRow<dynamic>();

                cmd.Clear();
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME, HIREDATE, COMM FROM EMP WHERE EMPNO = 7499");
                var emp2 = cmd.ExecuteRow<dynamic>();

                Assert.AreEqual(7369, emp1.EMPNO);
                Assert.AreEqual("SMITH", emp1.ENAME);
                Assert.AreEqual(new DateTime(1980, 12, 17), emp1.HIREDATE);
                Assert.AreEqual(null, emp1.COMM);

                Assert.AreEqual(7499, emp2.EMPNO);
                Assert.AreEqual("ALLEN", emp2.ENAME);
                Assert.AreEqual(new DateTime(1981, 02, 20), emp2.HIREDATE);
                Assert.AreEqual(300, emp2.COMM);
            }
        }

        [TestMethod]
        public void ExecuteTableDynamic_Test()
        {
            using (var cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME, HIREDATE, COMM FROM EMP ORDER BY EMPNO");
                var emp = cmd.ExecuteTable<dynamic>();

                Assert.AreEqual(14, emp.Count());
                Assert.AreEqual("SMITH", emp.First().ENAME);
                Assert.AreEqual(new DateTime(1980, 12, 17), emp.First().HIREDATE);
                Assert.AreEqual(null, emp.First().COMM);
            }
        }

        [TestMethod]
        public void ExecuteScalarDynamic_Test()
        {
            using (var cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP ");
                var count = cmd.ExecuteScalar<dynamic>();

                Assert.AreEqual(14, count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MissingMethodException), "Properties of your anonymous class must be in the same type and same order of your SQL Query.")]
        public void ExecuteRowAnonymousTypedWithExtraProperty_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME, HIREDATE FROM EMP WHERE EMPNO = 7369");
                var emp = cmd.ExecuteRow(new
                {
                    EmpNo = 0,
                    Ename = string.Empty,
                    HireDate = (DateTime?)null,
                    UnknowProperty = 0
                });

                Assert.AreEqual(7369, emp.EmpNo);
            }
        }

        [TestMethod]
        public void ExecuteRowPrimitive_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO FROM EMP WHERE EMPNO = 7369");
                int empno = cmd.ExecuteRow<int>();

                Assert.AreEqual(7369, empno);
            }
        }

        [TestMethod]
        public void ExecuteRowPrimitiveNullable_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT COMM FROM EMP WHERE EMPNO = 7369");
                int? comm = cmd.ExecuteRow<int?>();

                Assert.AreEqual(null, comm);
            }
        }

        [TestMethod]
        public void ExecuteRowWithDelegate_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
                EMP emp = cmd.ExecuteRow<EMP>((row) => new EMP() { EmpNo = Convert.ToInt32(row["EMPNO"]) });

                Assert.AreEqual(7369, emp.EmpNo);
            }
        }

        [TestMethod]
        public void ExecuteScalar_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP ");
                object data = cmd.ExecuteScalar();

                Assert.AreEqual(14, data);
            }
        }

        [TestMethod]
        public void ExecuteScalarWithParameter_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT ENAME ")
                               .AppendLine("  FROM EMP ")
                               .AppendLine(" WHERE EMPNO = @EmpNo ")
                               .AppendLine("   AND HIREDATE = @HireDate ")
                               .AppendLine("   AND JOB = @Job ");

                cmd.Parameters.AddWithValue("@EMPNO", 7369);                            // Parameter in Upper Case
                cmd.Parameters.AddWithValue("HireDate", new DateTime(1980, 12, 17));    // Parameter without @
                cmd.Parameters.AddWithValue("@Job", "CLERK");                           // Parameter in normal mode

                object data = cmd.ExecuteScalar();

                Assert.AreEqual("SMITH", data);
            }
        }

        [TestMethod]
        public void ExecuteScalarWithAnonymousParameters_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT ENAME ")
                               .AppendLine("  FROM EMP ")
                               .AppendLine(" WHERE EMPNO = @EmpNo ")
                               .AppendLine("   AND HIREDATE = @HireDate ")
                               .AppendLine("   AND JOB = @Job ")
                               .AppendLine("   AND 1 = @NotDeleted ");

                cmd.Parameters.AddWithValue("@EMPNO", 1234);                            // Parameter in Upper Case
                cmd.Parameters.AddWithValue("HireDate", new DateTime(1980, 1, 1));      // Parameter without @
                cmd.Parameters.AddWithValue("@Job", "FAKE");                            // Parameter in normal mode
                cmd.Parameters.AddWithValue("@NotDeleted", true);                       // Parameter not replaced by .AddValues

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
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT ENAME ")
                               .AppendLine("  FROM EMP ")
                               .AppendLine(" WHERE EMPNO = @EmpNo ")
                               .AppendLine("   AND HIREDATE = @HireDate ")
                               .AppendLine("   AND JOB = @Job ");

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
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
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
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLineFormat(" SELECT COUNT(*) FROM EMP ");
                int data = cmd.ExecuteScalar<int>();

                Assert.AreEqual(14, data);
            }
        }

        [TestMethod]
        public void ExecuteScalarTypedNull_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLineFormat(" SELECT COMM FROM EMP WHERE EMPNO = {0} ", 7369);
                int? data = cmd.ExecuteScalar<int?>();

                Assert.AreEqual(null, data);
            }
        }

        [TestMethod]
        public void ExecuteScalarWhereNoDataFound_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLineFormat(" SELECT COMM FROM EMP WHERE EMPNO = 99999 ");
                int? data = cmd.ExecuteScalar<int?>();

                Assert.AreEqual(null, data);
            }
        }

        [TestMethod]
        public void ExecuteTableTyped_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
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

        [TestMethod]
        public void ExecuteTableCustomedTyped_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME, HIREDATE, COMM, MGR FROM EMP ");
                var data = cmd.ExecuteTable<EMP>((row) =>
                {
                    return new EMP()
                    {
                        EmpNo = row.Field<int>("EMPNO"),
                        EName = row.Field<string>("ENAME"),
                        HireDate = row.Field<DateTime>("HIREDATE"),
                        Comm = row.Field<int?>("COMM")
                    };
                });
                EMP smith = data.FirstOrDefault(i => i.EmpNo == 7369);

                Assert.AreEqual(EMP.Smith.EmpNo, smith.EmpNo);
                Assert.AreEqual(EMP.Smith.EName, smith.EName);
                Assert.AreEqual(EMP.Smith.HireDate, smith.HireDate);
                Assert.AreEqual(EMP.Smith.Comm, smith.Comm);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MissingMethodException), "Properties of your anonymous class must be in the same type and same order of your SQL Query.")]
        public void ExecuteTableCustomedAnonymousTyped_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME, HIREDATE FROM EMP ");
                var data = cmd.ExecuteTable(new
                    {
                        EmpNo = 0,
                        EName = String.Empty,
                        HireDate = DateTime.Today,
                        MyVar = ""
                    });
                var smith = data.FirstOrDefault(i => i.EmpNo == 7369);

                Assert.AreEqual(EMP.Smith.EmpNo, smith.EmpNo);
                Assert.AreEqual(EMP.Smith.EName, smith.EName);
                Assert.AreEqual(EMP.Smith.HireDate, smith.HireDate);
            }
        }

        [TestMethod]
        public void ExecuteTableTypedWithColumnAttribute_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
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
        public void ExecuteDataSetTyped_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME FROM EMP ");
                cmd.CommandText.AppendLine(" SELECT * FROM DEPT ");

                var data = cmd.ExecuteDataSet<EMP, DEPT>();
                var smith = data.Item1.FirstOrDefault(i => i.EmpNo == 7369);
                var accounting = data.Item2.FirstOrDefault(i => i.DeptNo == 10);

                Assert.AreEqual(EMP.Smith.EmpNo, smith.EmpNo);
                Assert.AreEqual(EMP.Smith.EName, smith.EName);
                Assert.AreEqual(DEPT.Accounting.DName, accounting.DName);
                Assert.AreEqual(DEPT.Accounting.Loc, accounting.Loc);
            }
        }

        [TestMethod]
        public void ExecuteDataSetCustomedTyped_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME FROM EMP ");
                cmd.CommandText.AppendLine(" SELECT DEPTNO, DNAME FROM DEPT ");

                var data = cmd.ExecuteDataSet
                    (
                    new { EmpNo = 0, EName = ""},
                    new { DeptNo = 0, DName = ""}
                    );
                var smith = data.Item1.FirstOrDefault(i => i.EmpNo == 7369);
                var accounting = data.Item2.FirstOrDefault(i => i.DeptNo == 10);

                Assert.AreEqual(EMP.Smith.EmpNo, smith.EmpNo);
                Assert.AreEqual(EMP.Smith.EName, smith.EName);
                Assert.AreEqual(DEPT.Accounting.DeptNo, accounting.DeptNo);
                Assert.AreEqual(DEPT.Accounting.DName, accounting.DName);
            }
        }

        [TestMethod]
        public void ExecuteDataSetTyped_WithSimpleType_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SELECT EMPNO, ENAME FROM EMP ");
                cmd.CommandText.AppendLine(" SELECT * FROM DEPT ");
                cmd.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP ");

                var data = cmd.ExecuteDataSet<EMP, DEPT, int>();
                var smith = data.Item1.FirstOrDefault(i => i.EmpNo == 7369);
                var accounting = data.Item2.FirstOrDefault(i => i.DeptNo == 10);
                var NbOfEmp = data.Item3;

                Assert.AreEqual(EMP.Smith.EmpNo, smith.EmpNo);
                Assert.AreEqual(EMP.Smith.EName, smith.EName);
                Assert.AreEqual(DEPT.Accounting.DName, accounting.DName);
                Assert.AreEqual(DEPT.Accounting.Loc, accounting.Loc);
                Assert.AreEqual(14, NbOfEmp.First());
            }
        }

        [TestMethod]
        public void ExecuteNonQuery_Transaction_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
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
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection, transaction))
                {
                    cmd.Log = Console.WriteLine;
                    cmd.CommandText.AppendLine(" INSERT INTO EMP (EMPNO, ENAME) VALUES (1234, 'ABC') ");
                    cmd.ExecuteNonQuery();
                }

                using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection, transaction))
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
            SqlTransaction currentTransaction;

            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
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
            using (SqlDatabaseCommand cmd1 = new SqlDatabaseCommand(_connection))
            {
                cmd1.Log = Console.WriteLine;
                cmd1.CommandText.AppendLine(" DELETE FROM EMP ");
                cmd1.TransactionBegin();
                cmd1.ExecuteNonQuery();

                using (SqlDatabaseCommand cmd2 = new SqlDatabaseCommand(_connection, cmd1.Transaction))
                {
                    cmd2.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP ");
                    int count = cmd2.ExecuteScalar<int>();
                }

                cmd1.TransactionRollback();
            }
        }


        [TestMethod]
        [ExpectedException(typeof(SqlException))]
        public void ExecuteTableTyped_ThrowingException_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.ThrowException = true;
                cmd.CommandText.AppendLine(" XXX ");
                var data = cmd.ExecuteTable<EMP>();
            }
        }

        [TestMethod]
        public void ExecuteTableTyped_WithoutThrowingException_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.ThrowException = false;
                cmd.CommandText.AppendLine(" XXX ");
                var data = cmd.ExecuteTable<EMP>();

                Assert.AreEqual(null, data);
            }
        }

        [TestMethod]
        public void ExecuteRowTyped_WithoutThrowingException_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.ThrowException = false;
                cmd.CommandText.AppendLine(" XXX ");
                var data = cmd.ExecuteRow<EMP>();

                Assert.AreEqual(null, data);
            }
        }

        [TestMethod]
        public void ExecuteScalarTyped_WithoutThrowingException_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.ThrowException = false;
                cmd.CommandText.AppendLine(" XXX ");
                var data = cmd.ExecuteScalar<EMP>();

                Assert.AreEqual(null, data);
            }
        }

        #endregion

        #region DEADLOCKS

        [TestMethod()]
        public void RaiseDeadLock_Test()
        {
            SqlException ex = this.RaiseSqlDeadLock(false);

            Assert.IsNotNull(ex);
            Assert.AreEqual(ex.Number, 1205);
        }

        [TestMethod()]
        public void RetryWhenDeadLockOccured_Test()
        {
            SqlException ex = this.RaiseSqlDeadLock(true);

            Assert.IsNull(ex);
        }

        private SqlException RaiseSqlDeadLock(bool withRetry)
        {
            // See: http://stackoverflow.com/questions/22825147/how-to-simulate-deadlock-on-sql-server

            SqlConnection connection2 = new SqlConnection(CONNECTION_STRING);
            connection2.Open();
            SqlException exToReturn = null;

            try
            {
                using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
                {
                    cmd.Log = Console.WriteLine;

                    cmd.CommandText.AppendLine(" CREATE TABLE ##Employees ( ");
                    cmd.CommandText.AppendLine("     EmpId INT IDENTITY, ");
                    cmd.CommandText.AppendLine("     EmpName VARCHAR(16), ");
                    cmd.CommandText.AppendLine("     Phone VARCHAR(16) ");
                    cmd.CommandText.AppendLine(" ) ");

                    cmd.CommandText.AppendLine(" INSERT INTO ##Employees (EmpName, Phone) ");
                    cmd.CommandText.AppendLine(" VALUES('Martha', '800-555-1212'), ('Jimmy', '619-555-8080') ");

                    cmd.CommandText.AppendLine(" CREATE TABLE ##Suppliers( ");
                    cmd.CommandText.AppendLine("     SupplierId INT IDENTITY, ");
                    cmd.CommandText.AppendLine("     SupplierName VARCHAR(64), ");
                    cmd.CommandText.AppendLine("     Fax VARCHAR(16) ");
                    cmd.CommandText.AppendLine(" ) ");

                    cmd.CommandText.AppendLine(" INSERT INTO ##Suppliers (SupplierName, Fax) ");
                    cmd.CommandText.AppendLine(" VALUES ('Acme', '877-555-6060'), ('Rockwell', '800-257-1234') ");

                    cmd.ExecuteNonQuery();

                }

                using (SqlDatabaseCommand cmd1 = new SqlDatabaseCommand(_connection))
                {
                    using (SqlDatabaseCommand cmd2 = new SqlDatabaseCommand(connection2))
                    {
                        cmd1.Log = Console.WriteLine;
                        cmd2.Log = Console.WriteLine;

                        cmd1.TransactionBegin();
                        cmd2.TransactionBegin();

                        cmd1.Clear();
                        cmd1.CommandText.AppendLine(" UPDATE ##Employees SET EmpName = 'Mary'    WHERE empid = 1 ");
                        cmd1.ExecuteNonQuery();

                        cmd2.Clear();
                        cmd2.CommandText.AppendLine(" UPDATE ##Suppliers SET Fax = N'555-1212'   WHERE supplierid = 1 ");
                        cmd2.ExecuteNonQuery();

                        // Start and when cmd2.ExecuteNonQuery command will be executed, an DeadLock exception will be raised.
                        Task task1 = Task.Factory.StartNew(() =>
                        {
                            cmd1.Clear();
                            cmd1.ThrowException = false;
                            if (withRetry)
                            {
                                cmd1.RetryIfExceptionsOccured.SetDeadLockCodes();
                            }
                            cmd1.CommandText.AppendLine(" UPDATE ##Suppliers SET Fax = N'555-1212'   WHERE supplierid = 1 ");
                            cmd1.ExecuteNonQuery();
                        });

                        System.Threading.Thread.Sleep(1000);

                        cmd2.Clear();
                        cmd2.CommandText.AppendLine(" UPDATE ##Employees SET phone = N'555-9999' WHERE empid = 1 ");
                        cmd2.ExecuteNonQuery();

                        cmd2.Dispose();
                        connection2.Close();

                        // Wait cmd1 finished (and raised an Exception)
                        task1.Wait();

                        exToReturn = cmd1.Exception;
                    }
                }

                using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
                {
                    cmd.Log = Console.WriteLine;

                    cmd.CommandText.AppendLine(" DROP TABLE ##Employees ");
                    cmd.CommandText.AppendLine(" DROP TABLE ##Suppliers ");
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                connection2.Close();
                connection2.Dispose();
                connection2 = null;
            }

            return exToReturn;
        }

        #endregion

        #region QUERY FORMATTED

        [TestMethod]
        public void GetFormattedAsText_Simple_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.CommandText.Append(" SELECT * FROM EMP ");
                string formatted = cmd.GetCommandTextFormatted(QueryFormat.Text);

                Assert.AreEqual(formatted, " SELECT * FROM EMP ");
            }
        }

        [TestMethod]
        public void GetFormattedAsText_Parameters_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.CommandText.Append(" SELECT *, @MyGuid FROM EMP WHERE EMPNO = @EmpNo AND ENAME LIKE @Ename AND HIREDATE > @Hire AND COMM = @Comm ");
                cmd.Parameters.AddWithValue("@EmpNo", 7369);                                                  // Parameter normal
                cmd.Parameters.AddWithValue("@ENAME", "%SM%");                                                // Parameter in Upper Case
                cmd.Parameters.AddWithValue("Hire", new DateTime(1970, 05, 04, 14, 15, 16));                  // Parameter without @
                cmd.Parameters.AddWithValueOrDBNull("@Comm", null);                                           // Parameter NULL
                cmd.Parameters.AddWithValue("@MyGuid", new Guid("2fff1b89-b5f9-4a33-ac5b-a3ffee3e8b82"));     // Parameter GUID

                string formatted = cmd.GetCommandTextFormatted(QueryFormat.Text);

                Assert.AreEqual(" SELECT *, '2fff1b89-b5f9-4a33-ac5b-a3ffee3e8b82' FROM EMP WHERE EMPNO = 7369 AND ENAME LIKE '%SM%' AND HIREDATE > '1970-05-04 14:15:16' AND COMM = NULL ", formatted);
            }
        }

        [TestMethod]
        public void GetFormattedAsHtml_Parameters_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
                cmd.CommandText.AppendLine("  WHERE EMPNO = @EmpNo AND ENAME LIKE @Ename AND HIREDATE > @Hire AND COMM = @Comm ");
                cmd.Parameters.AddWithValue("@EmpNo", 7369);                                    // Parameter normal
                cmd.Parameters.AddWithValue("@ENAME", "%SM%");                                  // Parameter in Upper Case
                cmd.Parameters.AddWithValue("Hire", new DateTime(1970, 05, 04, 14, 15, 16));    // Parameter without @
                cmd.Parameters.AddWithValueOrDBNull("@Comm", null);                             // Parameter NULL

                string formatted = cmd.GetCommandTextFormatted(QueryFormat.Html);

                Assert.AreEqual(formatted, @" <span style=""color: #33f; font-weight: bold;"">SELECT</span> * <span style=""color: #33f; font-weight: bold;"">FROM</span> EMP <br/>  <span style=""color: #33f; font-weight: bold;"">WHERE</span> EMPNO = <span style=""color: #FF3F00;"">7369</span> <span style=""color: #33f; font-weight: bold;"">AND</span> ENAME <span style=""color: #33f; font-weight: bold;"">LIKE</span> <span style=""color: #FF3F00;"">'%SM%'</span> <span style=""color: #33f; font-weight: bold;"">AND</span> HIREDATE &gt; <span style=""color: #FF3F00;"">'<span style=""color: #FF3F00;"">1970</span><span style=""color: #FF3F00;"">-05</span><span style=""color: #FF3F00;"">-04</span> <span style=""color: #FF3F00;"">14</span>:<span style=""color: #FF3F00;"">15</span>:<span style=""color: #FF3F00;"">16</span>'</span> <span style=""color: #33f; font-weight: bold;"">AND</span> COMM = <span style=""color: #33f; font-weight: bold;"">NULL</span> <br/>");
            }
        }

        #endregion

        #region EXTENSIONS

        [TestMethod]
        public void Parameter_AddWithValueOrDBNull_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SET ANSI_NULLS OFF ");
                cmd.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP WHERE COMM = @Comm ");
                cmd.Parameters.AddWithValueOrDBNull("@Comm", null);

                Assert.AreEqual(cmd.ExecuteScalar<int>(), 10);
            }
        }

        [TestMethod]
        public void Parameter_ConvertToDBNull_Test()
        {
            using (SqlDatabaseCommand cmd = new SqlDatabaseCommand(_connection))
            {
                cmd.Log = Console.WriteLine;
                cmd.CommandText.AppendLine(" SET ANSI_NULLS OFF ");
                cmd.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP WHERE COMM = @Comm ");
                cmd.Parameters.AddWithValue("@Comm", null).ConvertToDBNull();

                Assert.AreEqual(cmd.ExecuteScalar<int>(), 10);
            }
        }

        #endregion
    }
}
