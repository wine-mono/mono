// ConnectionManager.cs - Singleton ConnectionManager class to manage
// database connections for test cases.
//
// Authors:
//      Sureshkumar T (tsureshkumar@novell.com)
// 
// Copyright Novell Inc., and the individuals listed on the
// ChangeLog entries.
//
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
#if !NO_ODBC
using System.Data.Odbc;
#endif
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace MonoTests.System.Data.Connected
{
	public class ConnectionManager
	{
		private static ConnectionManager instance;
		private ConnectionHolder<SqlConnection> sql;

		private const string OdbcEnvVar = "SYSTEM_DATA_ODBC";
		private const string SqlEnvVar = "SYSTEM_DATA_MSSQL";

		private ConnectionManager ()
		{
			//Environment.SetEnvironmentVariable(OdbcEnvVar, @"mysql-odbc|Driver={MySQL ODBC 5.3 Unicode Driver};server=127.0.0.1;uid=sa;pwd=qwerty123;");
			//Environment.SetEnvironmentVariable(SqlEnvVar, @"sqlserver-tds|server=127.0.0.1;database=master;user id=sa;password=qwerty123");

			// Generate a random db name
			DatabaseName = "monotest" + Guid.NewGuid().ToString().Substring(0, 7);

			sql = ConnectionHolder<SqlConnection>.FromEnvVar(SqlEnvVar);
			if (sql != null)
				CreateMssqlDatabase();
			
#if !NO_ODBC
			odbc = ConnectionHolder<OdbcConnection>.FromEnvVar(OdbcEnvVar);
			if (odbc != null)
				CreateMysqlDatabase();
#endif
		}

		private void CreateMssqlDatabase()
		{
			DBHelper.ExecuteNonQuery(sql.Connection, $"CREATE DATABASE [{DatabaseName}]");
			sql.Connection.ChangeDatabase(DatabaseName);

			string query = File.ReadAllText(@"Test/ProviderTests/sql/sqlserver.sql");

			var queries = SplitSqlStatements(query);
			foreach (var subQuery in queries)
			{
				DBHelper.ExecuteNonQuery(sql.Connection, subQuery);
			}
		}

#if !NO_ODBC
		private void CreateMysqlDatabase()
		{
			DBHelper.ExecuteNonQuery(odbc.Connection, $"CREATE DATABASE {DatabaseName}");
			odbc.Connection.ChangeDatabase(DatabaseName);
			odbc.ConnectionString += $"database={DatabaseName}";

			string query = File.ReadAllText("Test/ProviderTests/sql/MySQL_5.sql");

			var groups = query.Replace("delimiter ", "")
				.Split(new[] { "//\n" }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var subQuery in groups[0].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Concat(groups.Skip(1)))
			{
				DBHelper.ExecuteNonQuery(odbc.Connection, subQuery);
			}
		}
#endif

		private void DropMssqlDatabase()
		{
			sql.Connection.ChangeDatabase("master");
			string query = $"ALTER DATABASE [{DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;\nDROP DATABASE [{DatabaseName}]";
			DBHelper.ExecuteNonQuery(sql.Connection, query);
		}

#if !NO_ODBC
		private void DropMysqlDatabase()
		{
			string query = $"DROP DATABASE [{DatabaseName}]";
			DBHelper.ExecuteNonQuery(odbc.Connection, query);
		}
#endif

		// Split SQL script by "GO" statements
		private static IEnumerable<string> SplitSqlStatements(string sqlScript)
		{
			var statements = Regex.Split(sqlScript,
					$@"^[\t ]*GO[\t ]*\d*[\t ]*(?:--.*)?$",
					RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
			return statements.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim(' ', '\r', '\n'));
		}

		public static ConnectionManager Instance => instance ?? (instance = new ConnectionManager());

		public string DatabaseName { get; }

#if !NO_ODBC

		private ConnectionHolder<OdbcConnection> odbc;

		public ConnectionHolder<OdbcConnection> Odbc
		{
			get
			{
				if (odbc == null)
					Assert.Ignore($"{OdbcEnvVar} environment variable is not set");
				return odbc;
			}
		}
#endif

		public ConnectionHolder<SqlConnection> Sql
		{
			get
			{
				if (sql == null)
					Assert.Ignore($"{SqlEnvVar} environment variable is not set");
				return sql;
			}
		}

		public void Close()
		{
			sql?.CloseConnection();
#if !NO_ODBC			
			odbc?.CloseConnection();
#endif
		}
	}

	public class ConnectionHolder<TConnection> where TConnection : DbConnection
	{
		private TConnection connection;

		public EngineConfig EngineConfig { get; }

		public TConnection Connection
		{
			get
			{
				if (!(connection.State == ConnectionState.Closed || 
					connection.State == ConnectionState.Broken))
					connection.Close();
				connection.ConnectionString = ConnectionString;
				connection.Open();
				return connection;
			}
		}

		public void CloseConnection()
		{
			if (connection != null && connection.State != ConnectionState.Closed)
				connection.Close();
		}

		public string ConnectionString { get; set; }

		public ConnectionHolder(EngineConfig engineConfig, DbProviderFactory dbProviderFactory, string connectionString)
		{
			EngineConfig = engineConfig;
			connection = (TConnection)dbProviderFactory.CreateConnection();
			ConnectionString = connectionString;
		}

		public static ConnectionHolder<TConnection> FromEnvVar(string envVarName)
		{
#if NO_CONFIGURATION
			throw new NotImplementedException ();
#else
			string variable = Environment.GetEnvironmentVariable(envVarName) ?? string.Empty;
			var envParts = variable.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
			if (envParts.Length == 0 || string.IsNullOrEmpty(envParts[0]))
				return null;

			string connectionName = envParts[0];
			string connectionString = variable.Remove(0, envParts[0].Length + 1);

			ConnectionConfig[] connections = null;
			try
			{
				connections = (ConnectionConfig[]) ConfigurationManager.GetSection("providerTests");
			}
			catch
			{
				return null;
			}

			foreach (ConnectionConfig connConfig in connections)
			{
				if (connConfig.Name != connectionName)
					continue;

				DbProviderFactory factory = DbProviderFactories.GetFactory(connConfig.Factory);
				return new ConnectionHolder<TConnection>(connConfig.Engine, factory, connectionString);
			}
			throw new InvalidOperationException($"Connection {connectionName} not found");
#endif
		}
	}
}
