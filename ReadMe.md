# DatabaseCommand - Simple Object Mapping

## Introduction

This C# library simplify all SQL Queries to external databases. An implementation for SQL Server is included.
The version 1.5 was rebuild using the CoreCLR runtime (https://dotnet.github.io).

```cs
    int count = cmd.ExecuteScalar<int>();
    var emps  = cmd.ExecuteTable<Employee>();
    var smith = cmd.ExecuteRow<dynamic>();
```

First, you need to add the NuGet packages. https://www.nuget.org/packages?q=Apps72.Dev.Data
Select the correct package for SQL Server, Oracle, SQLite or a generic .NET Core library.

Next, you need to create a SqlConnection or other database connection. 
The SqlConnection will be not closed by this library
The ConnectionString will instanciated a temporary SqlConnection for this query and will be closed after using.

Requirements: Microsoft Framework 4.0 (Client Profile) for desktop applications, or SQL Server 2008 R2 for SQL CLR Stored procedures, or .NET Standard 2.0 for .NET Core library.

## Basic Samples (video)

[![Samples](http://img.youtube.com/vi/DRfM15Paw8k/0.jpg)](http://www.youtube.com/watch?v=DRfM15Paw8k)

## Commands

- [ExecuteTable](#ExecuteTable): Execute a SQL query and retrieve all data to a list of C# objects.
- [AddParameter](#ExecuteTableWithParameters): Execute a SQL query, add some parameters and retrieve all data to a list of C# objects.
- [ExecuteRow](#ExecuteRow): Execute a SQL query and retrieve the first row to one serialized C# object.
- [ExecuteScalar](#ExecuteScalar): Execute a SQL query and retrieve the first value (first row / first column) to a C# data type.
- [TransactionBegin](#TransactionBegin): Manage your SQL Transactions.
- [Logging](#Logging): Trace all SQL queries sent to the server (in Text or HTML format).
- [ThrowException](#ThrowException): Disable the SqlException to avoid application crashes... and catch it via the Exception property or ExceptionOccured event.
- [RetryIfExceptionsOccureds](#RetryIfExceptionsOccured): Avoid DeadLocks with retrying your Execute commands maximum 3 times.
- [Best Practices](#BestPractices): Copy our samples and use it as templates.
- [Entities Generators](#EntitiesGenerator): Generate automatically all classes from your database classes (via a T4 file).


#### <a name="ExecuteTable"></a>ExecuteTable

```cs
    using (var cmd = new SqlDatabaseCommand(_connection))
    {
	    cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
	    var emps = cmd.ExecuteTable<Employee>();
    }
```

Calling an Execute method using a **dynamic** return type.

```cs
    using (var cmd = new SqlDatabaseCommand(_connection))
    {
	    cmd.CommandText.AppendLine(" SELECT * FROM EMP ");
	    var emps = cmd.ExecuteTable<dynamic>();
    }
```

#### ExecuteTable customized

```cs
    using (var cmd = new SqlDatabaseCommand(_connection))
    {
        cmd.CommandText.AppendLine(" SELECT EMPNO, HIREDATE FROM EMP ");
        var data = cmd.ExecuteTable<Employee>((row) =>
        {
            return new Employee()
            {
                EmpNo = row.Field<int>("EMPNO"),
                Age = DateTime.Today.Year - row.Field<DateTime>("HIREDATE").Year
            };
        });
    }
```

#### <a name="ExecuteTableWithParameters"></a>ExecuteTable with parameters

```cs
    using (var cmd = new SqlDatabaseCommand(_connection))
    {
	    cmd.CommandText.AppendLine(" SELECT * ")
                       .AppendLine("   FROM EMP ")
                       .AppendLine("  WHERE EMPNO = @EmpNo ")
                       .AppendLine("    AND HIREDATE = @HireDate ");

        cmd.AddParameter(new
                {
                    EmpNo = 7369,
                    HireDate = new DateTime(1980, 12, 17)
                });

	    var emps = cmd.ExecuteTable<Employee>();
    }
```

#### <a name="ExecuteRow"></a>ExecuteRow

```cs
    using (var cmd = new SqlDatabaseCommand(_connection))
    {
        cmd.CommandText.AppendLine(" SELECT * FROM EMP WHERE EMPNO = 7369 ");
        EMP emp = cmd.ExecuteRow<EMP>();
    }
```

#### ExecuteRow customized

```cs
    using (var cmd = new SqlDatabaseCommand(_connection))
    {
        cmd.CommandText.AppendLine(" SELECT * FROM EMP WHERE EMPNO = 7369 ");
        var emp = cmd.ExecuteRow((row) =>
        {
            return new
            {
                Number = Convert.ToInt32(row["EMPNO"]),
                Name = Convert.ToString(row["ENAME"])
            };
        });
    }
```

#### <a name="ExecuteScalar"></a>ExecuteScalar

```cs
    using (var cmd = new SqlDatabaseCommand(_connection))
    {
        cmd.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP ");
        int data = cmd.ExecuteScalar<int>();
    }
```

#### <a name="TransactionBegin"></a>TransactionBegin

```cs
    using (var cmd = new SqlDatabaseCommand(_connection))
    {
        cmd.CommandText.AppendLine(" DELETE FROM EMP ");

        cmd.TransactionBegin();
        cmd.ExecuteNonQuery();
        cmd.TransactionRollback();
    }
```

Other sample

```cs
    using (var cmd1 = new SqlDatabaseCommand(_connection))
    {
        cmd1.CommandText.AppendLine(" DELETE FROM EMP ");
        cmd1.TransactionBegin();
        cmd1.ExecuteNonQuery();
        using (var cmd2 = new SqlDatabaseCommand(_connection, cmd1.Transaction))
        {
            cmd2.CommandText.AppendLine(" SELECT COUNT(*) FROM EMP ");
            int count = cmd2.ExecuteScalar<int>();
        }
        cmd1.TransactionRollback();
    }
```

#### <a name="Logging"></a>Logging
All SQL queries can be traced via the <b>.log</b> property.

```cs
    using (var cmd = new SqlDatabaseCommand(_connection))
    {
        // Easy
        cmd.Log = Console.WriteLine;
        
        // Lambda expression
        cmd.Log = (query) => 
        {
            Console.WriteLine(cmd.GetCommandTextFormatted(QueryFormat.Html));
        };
    }
```

#### <a name="ThrowException"></a>ThrowException

```cs
    cmd.ThrowException = false;
    cmd1.ExceptionOccured += (sender, e) =>
    {
        // Manage SQL Exceptions
    };
```

#### <a name="RetryIfExceptionsOccured"></a>RetryIfExceptionsOccured

```cs
    using (var cmd = new SqlDatabaseCommand(_connection))
    {
        cmd.RetryIfExceptionsOccured.SetDeadLockCodes();

        cmd.CommandText.AppendLine(" DELETE FROM EMP ");
        cmd.ExecuteNonQuery();
    }
```

#### <a name="BestPractices"></a>Best practices

In you project, create a <b>DataService</b> implementing IDisposable and add a method GetDatabaseCommand.

#####1. Using ConnectionString for all applications or threads (ex. Web Applications, WebAPI, Web Services, ...)

```cs
        public class DataService : IDataService
        {
            public SqlDatabaseCommand GetDatabaseCommand()
            {
                return new SqlDatabaseCommand(CONNECTION_STRING);
            }

            public SqlDatabaseCommand GetDatabaseCommand(SqlTransaction transaction)
            {
                return new SqlDatabaseCommand(transaction.Connection, transaction);
            }
        }
```

#####2. Using One SqlConnection for the application (ex. Desktop Apps, Universal Apps, ...)

```cs
        public class DataService : IDataService, IDisposable
        {
            private SqlConnection _connection = null;

            public DataService()
            {
                _connection = new SqlConnection(CONNECTION_STRING);
                _connection.Open();
            }

            public SqlDatabaseCommand GetDatabaseCommand()
            {
                return new SqlDatabaseCommand(_connection);
            }

            public SqlDatabaseCommand GetDatabaseCommand(SqlTransaction transaction)
            {
                return new SqlDatabaseCommand(_connection, transaction);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_connection.State != ConnectionState.Closed)
                    {
                        _connection.Close();
                        _connection.Dispose();
                        _connection = null;
                    }
                }
            }

            ~DataService()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }   
```

#### <a name="EntitiesGenerator"></a>Entities Generator

You can use a <a href="https://en.wikipedia.org/wiki/Text_Template_Transformation_Toolkit">T4 file</a> to generate all classes associated to your database tables.
Copy this [sample .tt file](https://github.com/Apps72/Dev.Data/blob/master/Test/SqlServer/Entities/Scott.tt) in your project and set your correct **Connection String**. Check if the .tt file properties are **Build Action** = Content and **Custom Tool = TextTemplatingFileGenerator**.

```cs
    // UPDATE THIS CONNECTION STRING
    const string CONNECTION_STRING = @"Server=(localdb)\ProjectsV12;Database=Scott;Integrated Security=true;";
```

Each time you save this .tt file, you create an equivalent .cs file with all classes.

For example:

```cs
    // *********************************************
    // Code Generated with Apps72.Dev.Data.Generator
    // *********************************************
    using System;

    namespace Apps72.Dev.Data.Tests.Entities
    {
        /// <summary />
        public partial class DEPT
        {
            /// <summary />
            public virtual Int32 DEPTNO { get; set; }
            /// <summary />
            public virtual String DNAME { get; set; }
            /// <summary />
            public virtual String LOC { get; set; }
        }
        /// <summary />
        public partial class EMP
        {
            /// <summary />
            public virtual Int32 EMPNO { get; set; }
            /// <summary />
            public virtual String ENAME { get; set; }
            /// <summary />
            public virtual Int32? MGR { get; set; }
            /// <summary />
            public virtual DateTime? HIREDATE { get; set; }
            /// <summary />
            public virtual Int32? SAL { get; set; }
            /// <summary />
            public virtual Int32? DEPTNO { get; set; }
        }
    }
```

## <a name="ReleaseNotes"></a>Release Notes

### Version 1.2

* Initial version with all basic features.

### Version 1.3

* Add a extension method **SqlParameterCollection.AddValues** to simplify the creation of parameters.

### Version 1.4

* Add an **EntitiesGenerator** class to generate all classes associated to an existing Database, via the file **Entities.tt**.

### Version 1.5

* All code reviewed and rebuilt with .NET Core framework (https://dotnet.github.io)
* Fix the Numeric SQL type to Decimal C# type.

### Version 1.5.2

* Fix using a Transaction in constructors: the transaction will be not disposed with the DatabaseCommandBase.

### Version 2.0

* Source code Refactoring.
* Add the **ExecuteTableSet** method to get multiple tables, using multiple SELECT commands in one query.
* Add OracleDatabaseCommand to manage Oracle Server databases (need the Oracle.ManagedDataAccess assembly).

### Version 2.1

* Fix using the constructor with ConnectionString and CommandText parameters (the CommandText was not correctly assigned).

### Version 2.2

* Add a **DotNetCore** version with features based on DbConnection.
* Add the method **AddParameter** in DatabaseCommandBase, usable for all projects (SqlServer, Oracle, Sqlite, ...).
* Remove DataInjection concept. That will be replaced by pre and post execution events.

### Version 2.3

* Fix using Dispose method with AutoDisconnect mode.
* Fix when ThrowException = False: returns the default value and not an exception.

### Version 2.4

* Add **dynamic** return value. Example: *var emps = cmd.ExecuteTable&lt;**dynamic**&gt;();*

### [RoadMap]

* Add events before and after connection or query executions.
* Add Extension methods to configure and execute queries using only one command, like *connection.SqlCmd.Query("SELECT * FROM Emp").Execute();**
* Include Asynchronous methods.
