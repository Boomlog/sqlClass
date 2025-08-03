using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
//using Microsoft.SqlServer.Management.Smo;
using System.Configuration;
using System.Reflection;

namespace sqlClass
{
    public partial class Form1 : Form
    {

        // Developed by: Babak  Arjomandi @ babakarjomandi.com || picme.ir
        // Copyright by: VIRAWARE™ SMART SOLUTION 2010-2025

        SqlConnection sqlCon = new SqlConnection("integrated security=SSPI;persist security info=False;SERVER=localhost;Pooling=false;Connect Timeout=45;");
        SqlConnection sqlCon2 = new SqlConnection("integrated security=SSPI;persist security info=False;SERVER=localhost;Pooling=false;Connect Timeout=45;");
               
        string deepLevel = "";
        SqlCommand sqlCom;
        SqlDataReader sqlRed;
        string dbName = "";

        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {

            /*
            Assembly assembly = Assembly.GetExecutingAssembly();
            string version = assembly.GetName().Version.ToString();
            string buildDescription = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute))).Description;
            label2.Text = version + " | " + buildDescription;

            DataTable dtttt = Microsoft.SqlServer.Management.Smo.SmoApplication.EnumAvailableSqlServers();
            foreach (DataRow dataRow in dtttt.Rows)
            {
                if ((bool)dataRow["IsLocal"])
                {
                    comboBox1.Items.Add(dataRow["Server"]);
                }
            }
             */

            sqlCon.Open();

            sqlCom = new SqlCommand("select * from sys.sysdatabases ORDER BY name ;", sqlCon);
            //SqlCommand sqlCom = new SqlCommand("SELECT countrySHORT , countryLONG FROM tblIPCountry WHERE (ipFROM <= @ipFROM) AND (ipTO >= @ipTO) ;", sqlCon);
            //sqlCom.Parameters.Add(new SqlParameter("@ipFROM", x));
            //sqlCom.Parameters.Add(new SqlParameter("@ipTO", x));

            sqlRed = sqlCom.ExecuteReader();
            while (sqlRed.Read())
            {
                // while (sqlRed.HasRows)
                //  {
                checkedListBox1.Items.Add(sqlRed.GetValue(0).ToString());
                //  }
            }
            sqlRed.Close();
            sqlCon.Close();

            deepLevel = "db";
            
        }


        private void checkedListBox1_DoubleClick(object sender, EventArgs e)
        {
            switch (deepLevel)
            {
                case "db":
                    dbName = checkedListBox1.Items[checkedListBox1.SelectedIndex].ToString();
                    sqlCon.Open();
                    sqlCom = new SqlCommand("select * from " + dbName + ".sys.tables where [type] = 'U' ORDER BY name;", sqlCon);

                    sqlRed = sqlCom.ExecuteReader();
                    checkedListBox1.Items.Clear();
                    while (sqlRed.Read())
                    {
                        checkedListBox1.Items.Add(sqlRed.GetValue(0).ToString());
                    }
                    sqlRed.Close();
                    sqlCon.Close();
                    deepLevel = "table";
                    break;
                /*
            case "table":
                sqlCon.Open();
                sqlCom = new SqlCommand("SELECT c.[name] AS [columnName] , c.[type] AS [columnType] , t.[name] AS [columnTypeName] , c.[length] AS [columnLength] , c.[isnullable] AS [columnIsNullable] " +
                                        "FROM " + dbName + ".sys.syscolumns c INNER JOIN " + dbName + ".sys.systypes t ON t.[xtype] = c.[xtype] " +
                                        "WHERE id = (SELECT id  FROM " + dbName + ".sys.sysobjects WHERE [xtype] = 'U' AND [NAME] = '" + checkedListBox1.Items[checkedListBox1.SelectedIndex].ToString() + "') AND t.[name] <> 'sysname' " +
                                        "ORDER BY c.[colid] ;", sqlCon);

                sqlRed = sqlCom.ExecuteReader();
                checkedListBox1.Items.Clear();
                while (sqlRed.Read())
                {
                    checkedListBox1.Items.Add(sqlRed.GetValue(0).ToString() + " (" + sqlRed.GetValue(2).ToString()+")");
                }
                sqlRed.Close();
                sqlCon.Close();
                deepLevel = "field";
                break;
                 */
            }
        }


        private bool isPrimaryKey(string tableName, string FieldName)
        {
            bool result = false;
            sqlCon2.Open();
            SqlCommand sqlCom2 = new SqlCommand("SELECT cu.* " +
                                                "FROM " + dbName + ".INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu WHERE EXISTS ( SELECT tc.* FROM " + dbName + ".INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc WHERE tc.CONSTRAINT_CATALOG = '" + dbName + "' AND tc.TABLE_NAME = '" + tableName + "' AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY' AND tc.CONSTRAINT_NAME = cu.CONSTRAINT_NAME ) AND COLUMN_NAME = '" + FieldName + "' " +
                                                "ORDER BY ORDINAL_POSITION;", sqlCon2);
            SqlDataReader sqlRed2 = sqlCom2.ExecuteReader();
            if (sqlRed2.Read())
            {
                if (sqlRed2.HasRows)
                {
                    result = true;
                }
            }
            sqlRed2.Close();
            sqlCon2.Close();
            return result;
        }


        private bool isIdentityColumn(string tableName, string FieldName)
        {
            bool result = false;
            sqlCon2.Open();
            SqlCommand sqlCom2 = new SqlCommand("USE [" + dbName + "]  select * from sys.columns as c where c.is_identity=1 and object_id=(select object_id from sys.tables where name='" + tableName + "') and [name]='" + FieldName + "';", sqlCon2);
            SqlDataReader sqlRed2 = sqlCom2.ExecuteReader();
            if (sqlRed2.Read())
            {
                if (sqlRed2.HasRows)
                {
                    result = true;
                }
            }
            sqlRed2.Close();
            sqlCon2.Close();
            return result;
        }


        private string getPrimaryKey(string tableName, string FieldName)
        {
            if (isPrimaryKey(tableName, FieldName))
            {
                return "PrimaryKey";
            }
            else
            {
                return "";
            }
        }


        private string isNullable(string FieldVal)
        {
            string result = "";

            switch (FieldVal)
            {
                case "0":
                    result = "";
                    break;
                case "1":
                    result = "isNullable";
                    break;
            }
            return result;
        }


        private string sqlTypeToCShaprType(string sqlType, string getType)
        {
            //string[] sqlTypes = new string[26] { "image", "text", "uniqueidentifier", "tinyint", "smallint", "int", "smalldatetime", "real", "money", "datetime", "float", "sql_variant", "ntext", "bit", "decimal", "numeric", "smallmoney", "bigint", "varbinary", "varchar", "binary", "char", "timestamp", "nvarchar", "nchar", "xml" };
            //string[] cSharpTypes = new string[26] { "image", "string", "Int64", "byte", "Int16", "Int32", "DateTime", "decimal", "money", "DateTime", "float", "sql_variant", "string", "bool", "decimal", "numeric", "smallmoney", "Int64", "varbinary", "string", "binary", "string", "timestamp", "string", "string", "xml" };
            string[] sqlTypes = new string[26] { "Bit", "binary", "Bigint", "Char", "datetime", "decimal", "Float", "image", "Int", "Money", "nchar", "Ntext", "nvarchar", "Numeric", "Real", "smalldatetime", "smallint", "smallmoney", "sql_variant", "sysname", "text", "timestamp", "tinyint", "varbinary", "varchar", "uniqueidentifier" };

            //string[] cSharpTypes = new string[26] { "bool", "SqlBinary", "Int64", "String", "DateTime", "Decimal", "Double", "SqlBinary", "Int32", "Decimal", "String", "String", "String", "Decimal", "Decimal", "DateTime", "Int16", "Decimal", "Object", "String", "String", "SqlBinary", "Byte", "SqlBinary", "String", "Guid" };
            string[] cSharpTypes = new string[26] { "bool", "SqlBinary", "Int64", "String", "DateTime", "Decimal", "Double", "SqlBinary", "Int32", "Decimal", "String", "String", "String", "Decimal", "Single", "DateTime", "Int16", "Decimal", "Object", "String", "String", "SqlBinary", "Byte", "SqlBinary", "String", "Guid" };
            //string[] cSharpTypes_Get = new string[26] { "Boolean", "SqlBinary", "Int64", "String", "DateTime", "Decimal", "Double", "SqlBinary", "Int32", "Decimal", "String", "String", "String", "Decimal", "Decimal", "DateTime", "Int16", "Decimal", "Object", "String", "String", "SqlBinary", "Byte", "SqlBinary", "String", "Guid" };
            string[] cSharpTypes_Get = new string[26] { "Boolean", "SqlBinary", "Int64", "String", "DateTime", "Decimal", "Double", "SqlBinary", "Int32", "Decimal", "String", "String", "String", "Decimal", "Single", "DateTime", "Int16", "Decimal", "Object", "String", "String", "SqlBinary", "Byte", "SqlBinary", "String", "Guid" };
            string[] sqlTypesNet = new string[26] { "SqlBit", "SqlBinary", "SqlInt64", "SqlString", "SqlDateTime", "SqlDecimal", "SqlDouble", "SqlBinary", "SqlInt32", "SqlMoney", "SqlString", "SqlString", "SqlString", "SqlDecimal", "SqlSingle", "SqlDateTime", "SqlInt16", "SqlMoney", "Object", "SqlString", "SqlString", "SqlBinary", "SqlByte", "SqlBinary", "SqlString", "SqlGuid" };
            string[] sqlTypesDefValue = new string[26] { "false", "null", "0", "\"\"", "DateTime.MinValue.AddYears(1754)", "0", "0", "null", "0", "0", "\"\"", "\"\"", "\"\"", "0", "0", "DateTime.MinValue.AddYears(1754)", "0", "0", "null", "null", "\"\"", "TimeSpan.MinValue.AddYears(1754)", "0", "null", "\"\"", "Guid.NewGuid()" };
            string[] sqlDBTypess = new string[26] { "Bit", "Binary", "BigInt", "Char", "DateTime", "Decimal", "Float", "Image", "Int", "Money", "NChar", "NText", "NVarChar", "Numeric", "Real", "SmallDateTime", "SmallInt", "SmallMoney", "Variant", "VarChar", "Text", "TimeStamp", "TinyInt", "VarBinary", "VarChar", "UniqueIdentifier" };

            string[] htmlControls = new string[26] { "<!--Bit--><asp:CheckBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\"/>", 
                                                     "<!--binary-->", 
                                                     "<!--Bigint--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"20\"></asp:TextBox>", 
                                                     "<!--Char--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"lenghtControl\"></asp:TextBox>", 
                                                     "<!--datetime--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"100\"></asp:TextBox>", 
                                                     "<!--decimal-->", 
                                                     "<!--Float-->", 
                                                     "<!--image-->", 
                                                     "<!--Int--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"10\"></asp:TextBox>", 
                                                     "<!--Money--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"10\"></asp:TextBox>", 
                                                     "<!--nchar--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"lenghtControl\"></asp:TextBox>", 
                                                     "<!--Ntext--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"lenghtControl\" TextMode=\"MultiLine\" Rows=\"2\"></asp:TextBox>", 
                                                     "<!--nvarchar--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"lenghtControl\"></asp:TextBox>", 
                                                     "<!--Numeric-->", 
                                                     "<!--Real--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"10\"></asp:TextBox>", 
                                                     "<!--smalldatetime-->", 
                                                     "<!--smallint--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"5\"></asp:TextBox>", 
                                                     "<!--smallmoney--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"10\"></asp:TextBox>", 
                                                     "<!--sql_variant-->", 
                                                     "<!--sysname-->", 
                                                     "<!--text--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"lenghtControl\" TextMode=\"MultiLine\" Rows=\"2\"></asp:TextBox>", 
                                                     "<!--timestamp-->", 
                                                     "<!--tinyint--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"3\"></asp:TextBox>", 
                                                     "<!--varbinary-->", 
                                                     "<!--varchar--><asp:TextBox ID=\"idControl\" runat=\"server\" CssClass=\"cssControl\" MaxLength=\"lenghtControl\"></asp:TextBox>", 
                                                     "<!--uniqueidentifier-->" };

            string[] htmlControlPro = new string[26] { "Checked", 
                                                       "binary", 
                                                       "Bigint", 
                                                       "Text", 
                                                       "datetime", 
                                                       "decimal", 
                                                       "Float", 
                                                       "image", 
                                                       "Int", 
                                                       "Money", 
                                                       "Text", 
                                                       "Text", 
                                                       "Text", 
                                                       "Numeric", 
                                                       "Real", 
                                                       "smalldatetime", 
                                                       "smallint", 
                                                       "smallmoney", 
                                                       "sql_variant", 
                                                       "sysname", 
                                                       "text", 
                                                       "timestamp", 
                                                       "tinyint", 
                                                       "varbinary", 
                                                       "Text", 
                                                       "uniqueidentifier" };

            string result = "NA";

            for (int i = 0; i < 26; i++)
            {
                if (sqlType.ToLower() == sqlTypes[i].ToLower())
                {
                    switch (getType)
                    {
                        case "c#":
                            result = cSharpTypes[i];
                            break;
                        case "getc#":
                            result = cSharpTypes_Get[i];
                            break;
                        case "defValue":
                            result = sqlTypesDefValue[i];
                            break;
                        case "sqlDBType":
                            result = sqlDBTypess[i];
                            break;
                        case "htmlControl":
                            result = htmlControls[i];
                            break;
                        case "htmlControlPro":
                            result = htmlControlPro[i];
                            break;
                    }
                    break;
                }
            }
            return result;
        }


        private void DatabaseLayer(string path)
        {
            if (path != "" && path != null)
            {
                foreach (object item in this.checkedListBox1.CheckedItems)
                {
                    bool itHasPrimaryKey = false;
                    //checkedListBox1.Items.Add(sqlRed.GetValue(0).ToString());
                    sqlCon.Open();
                    sqlCom = new SqlCommand("SELECT c.[name] AS [columnName] , c.[type] AS [columnType] , t.[name] AS [columnTypeName] , c.[length] AS [columnLength] , c.[isnullable] AS [columnIsNullable] " +
                                             "FROM " + dbName + ".sys.syscolumns c INNER JOIN " + dbName + ".sys.systypes t ON t.[xtype] = c.[xtype] " +
                                             "WHERE id = (SELECT id  FROM " + dbName + ".sys.sysobjects WHERE [xtype] = 'U' AND [NAME] = '" + item.ToString() + "') AND t.[name] <> 'sysname' " +
                                             "ORDER BY c.[colid] ;", sqlCon);

                    sqlRed = sqlCom.ExecuteReader();

                    FileInfo MyFile = new FileInfo(path + "\\" + item.ToString() + ".cs");
                    StreamWriter sw = MyFile.CreateText();

                    string sql_where = "";
                    string sql_insert = "";
                    string sql_insert_value = "";
                    string sql_update = "";


                    sw.WriteLine("/// <summary>");
                    sw.WriteLine("/// Database Layer Class / Business Logic Layer Class / [" + dbName + "]  " + DateTime.Now);
                    sw.WriteLine("/// Copyright © " + DateTime.Now.Year.ToString() + " VIRAWARE INC. , Author: Babak Arjomandi");
                    sw.WriteLine("/// </summary>");
                    sw.WriteLine();
                    sw.WriteLine("namespace " + dbName);
                    sw.WriteLine("{");
                    sw.WriteLine();
                    sw.WriteLine("  using System;");
                    sw.WriteLine("  using System.Collections;");
                    sw.WriteLine("  using System.Collections.Generic;");
                    sw.WriteLine("  using System.ComponentModel;");
                    sw.WriteLine("  using System.Data;");
                    sw.WriteLine("  using System.Data.SqlClient;");
                    sw.WriteLine("  using System.Data.SqlTypes;");
                    sw.WriteLine("  using System.Text;");
                    sw.WriteLine();
                    sw.WriteLine();


                    #region Type Struct

                    sw.WriteLine();
                    sw.WriteLine("  public struct " + item.ToString() + "Type");
                    sw.WriteLine("  {");
                    sw.WriteLine();
                    sw.WriteLine("      #region Type Struct");
                    sw.WriteLine();

                    while (sqlRed.Read())
                    {
                        if (isPrimaryKey(item.ToString(), sqlRed.GetValue(0).ToString()))
                        {
                            itHasPrimaryKey = true;
                            // sw.WriteLine("      [Category(\"Primary Key\")]");
                            sql_where += " [" + sqlRed.GetValue(0).ToString() + "]=@" + sqlRed.GetValue(0).ToString() + ",";
                            if (!isIdentityColumn(item.ToString(), sqlRed.GetValue(0).ToString()))
                            {
                                sql_insert += " [" + sqlRed.GetValue(0).ToString() + "],";
                                sql_insert_value += " @" + sqlRed.GetValue(0).ToString() + ",";
                                sql_update += " [" + sqlRed.GetValue(0).ToString() + "]=@" + sqlRed.GetValue(0).ToString() + ",";
                            }
                        }
                        else
                        {
                            //sw.WriteLine("      [Category(\"Column\")]");
                            sql_insert += " [" + sqlRed.GetValue(0).ToString() + "],";
                            sql_insert_value += " @" + sqlRed.GetValue(0).ToString() + ",";
                            sql_update += " [" + sqlRed.GetValue(0).ToString() + "]=@" + sqlRed.GetValue(0).ToString() + ",";
                        }

                        sw.WriteLine("      /// <summary>");
                        int ncharLength = Convert.ToInt32(sqlRed.GetValue(3).ToString());
                        if ((sqlRed.GetValue(2).ToString().IndexOf("nchar") > -1) || (sqlRed.GetValue(2).ToString().IndexOf("nvarchar") > -1))
                        {
                            ncharLength = ncharLength / 2;
                        }
                        string _ncharLength=ncharLength.ToString();
                        if (_ncharLength == "0")
                        {
                            _ncharLength = "MAX";
                        }
                        sw.WriteLine("      /// " + sqlRed.GetValue(0).ToString() + " " + sqlRed.GetValue(2).ToString() + "(" + _ncharLength + ") " + getPrimaryKey(item.ToString(), sqlRed.GetValue(0).ToString()) + isNullable(sqlRed.GetValue(4).ToString()));
                        sw.WriteLine("      /// </summary>");
                        sw.WriteLine("      public " + sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "c#") + " " + sqlRed.GetValue(0).ToString() + ";");
                        sw.WriteLine();

                    }

                    if (sql_where != "")
                    {
                        sql_where = sql_where.TrimEnd(',').Trim(); 
                        sql_where = sql_where.Replace(",", " AND ");
                    }
                    sql_insert = sql_insert.TrimEnd(',').Trim(); 
                    sql_insert_value = sql_insert_value.TrimEnd(',').Trim();
                    sql_update = sql_update.TrimEnd(',').Trim(); 

                    sw.WriteLine();
                    sw.WriteLine("      #endregion");
                    sw.WriteLine();
                    sw.WriteLine("  }");
                    sw.WriteLine();
                    #endregion


                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine("  public static class " + item.ToString() + "Static");
                    sw.WriteLine("  {");
                    sw.WriteLine();


                    #region Const String Memebers
                    sw.WriteLine("      #region Const String Memebers");


                    sw.WriteLine();
                    sw.WriteLine("      public static " + item.ToString() + "Type defaultValue");
                    sw.WriteLine("      {");
                    sw.WriteLine("          get");
                    sw.WriteLine("          {");
                    sw.WriteLine("              " + item.ToString() + "Type result;");
                    sqlRed.Close();
                    sqlRed = sqlCom.ExecuteReader();
                    while (sqlRed.Read())
                    {
                        sw.WriteLine("              result." + sqlRed.GetValue(0).ToString() + " = " + sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "defValue") + ";");
                    }
                    sqlRed.Close();
                    sw.WriteLine("              return result;");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");



                    sw.WriteLine();
                    //sw.WriteLine("      public string sqlSelect = \"SELECT * FROM " + item.ToString() + " WHERE " + sql_where + " \";");
                    sw.WriteLine("      public static string sqlSelect");
                    sw.WriteLine("      {");
                    sw.WriteLine("          get");
                    sw.WriteLine("          {");
                    sw.WriteLine("              return \"SELECT * FROM " + item.ToString() + " WHERE " + sql_where + " \";");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");
                    sw.WriteLine();

                    //sw.WriteLine("      public string sqlInsert = \"INSERT INTO " + item.ToString() + " (" + sql_insert + ") VALUES (" + sql_insert_value + ") \";");
                    sw.WriteLine("      public static string sqlInsert");
                    sw.WriteLine("      {");
                    sw.WriteLine("          get");
                    sw.WriteLine("          {");
                    sw.WriteLine("              return \"INSERT INTO " + item.ToString() + " (" + sql_insert + ") VALUES (" + sql_insert_value + ") \";");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");
                    sw.WriteLine();

                    //sw.WriteLine("      public string sqlUpdate = \"UPDATE " + item.ToString() + " SET " + sql_update + " WHERE " + sql_where + " \";");
                    sw.WriteLine("      public static string sqlUpdate");
                    sw.WriteLine("      {");
                    sw.WriteLine("          get");
                    sw.WriteLine("          {");
                    sw.WriteLine("              return \"UPDATE " + item.ToString() + " SET " + sql_update + " WHERE " + sql_where + " \";");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");
                    sw.WriteLine();

                    //sw.WriteLine("      public string sqlDelete = \"DELETE FROM " + item.ToString() + " WHERE " + sql_where + " \";");
                    sw.WriteLine("      public static string sqlDelete");
                    sw.WriteLine("      {");
                    sw.WriteLine("          get");
                    sw.WriteLine("          {");
                    sw.WriteLine("              return \"DELETE FROM " + item.ToString() + " WHERE " + sql_where + " \";");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");

                    sw.WriteLine();
                    sw.WriteLine("      #endregion");
                    sw.WriteLine();
                    #endregion


                    #region Private Methods
                    /*
                    sw.WriteLine();
                    sw.WriteLine("      #region Private Methods");
                    sw.WriteLine();
                    sw.WriteLine("      private SqlParameter AddSqlParam(string ParamName, object Value, SqlDbType SqlType)");
                    sw.WriteLine("      {");
                    sw.WriteLine("          SqlParameter SqlParam = new SqlParameter(ParamName, SqlType);");
                    sw.WriteLine("          SqlParam.Value = Value;");
                    sw.WriteLine("          return SqlParam;");
                    sw.WriteLine("      }");
                    sw.WriteLine();
                    sw.WriteLine("      #endregion");
                    sw.WriteLine();
                    */
                    #endregion


                    #region Public Methods
                    sw.WriteLine();
                    sw.WriteLine("      #region Public Methods");

                    sw.WriteLine();
                    sw.WriteLine("      public static SqlParameter[] GetSqlParameters(" + item.ToString() + "Type sqlParams)");
                    sw.WriteLine("      {");
                    sw.WriteLine("          List<SqlParameter> SqlParmColl = new List<SqlParameter>();");
                    sw.WriteLine("          try");
                    sw.WriteLine("          {");
                    sqlRed.Close();
                    sqlRed = sqlCom.ExecuteReader();
                    while (sqlRed.Read())
                    {
                        //sw.WriteLine("              SqlParmColl.Add(AddSqlParm(\"@" + sqlRed.GetValue(0).ToString() + "\", _" + item.ToString() + "." + sqlRed.GetValue(0).ToString() + ", SqlDbType." + sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "sqlDBType") + "));");
                        sw.WriteLine("              SqlParmColl.Add(dbTools.AddSqlParam(\"@" + sqlRed.GetValue(0).ToString() + "\", sqlParams." + sqlRed.GetValue(0).ToString() + ", SqlDbType." + sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "sqlDBType") + "));");
                    }
                    sw.WriteLine("              return SqlParmColl.ToArray();");
                    sw.WriteLine("          }");
                    sw.WriteLine("          catch (Exception Exc)");
                    sw.WriteLine("          {");
                    sw.WriteLine("              throw Exc;");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");
                    sw.WriteLine();

                    /*
                    sw.WriteLine("      public SqlParameter[] GetSqlParameters_ByKey(" + item.ToString() + "Type sqlParams)");
                    sw.WriteLine("      {");
                    sw.WriteLine("          List<SqlParameter> SqlParmColl = new List<SqlParameter>();");
                    sw.WriteLine("          try");
                    sw.WriteLine("          {");
                    sqlRed.Close();
                    sqlRed = sqlCom.ExecuteReader();
                    while (sqlRed.Read())
                    {
                        if (isPrimaryKey(item.ToString(), sqlRed.GetValue(0).ToString()))
                        {
                            sw.WriteLine("              SqlParmColl.Add(AddSqlParam(\"@" + sqlRed.GetValue(0).ToString() + "\", sqlParams." + sqlRed.GetValue(0).ToString() + ", SqlDbType." + sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "sqlDBType") + "));");
                        }
                    }
                    sw.WriteLine("              return SqlParmColl.ToArray();");
                    sw.WriteLine("          }");
                    sw.WriteLine("          catch (Exception Exc)");
                    sw.WriteLine("          {");
                    sw.WriteLine("              throw Exc;");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");
                    sw.WriteLine();
                    */


                    sw.WriteLine("      public static " + item.ToString() + "Type ReadFromDataRow(DataRow dr)");
                    sw.WriteLine("      {");
                    sw.WriteLine("          try");
                    sw.WriteLine("          {");
                    sw.WriteLine("              " + item.ToString() + "Type result = defaultValue;");
                    sqlRed.Close();
                    sqlRed = sqlCom.ExecuteReader();
                    while (sqlRed.Read())
                    {
                        sw.WriteLine("              if (!dr.IsNull(\"" + sqlRed.GetValue(0).ToString() + "\"))");
                        sw.WriteLine("              {");
                        sw.WriteLine("                  result." + sqlRed.GetValue(0).ToString() + " = (" + sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "c#") + ")dr[\"" + sqlRed.GetValue(0).ToString() + "\"];");
                        sw.WriteLine("              }");
                    }
                    sw.WriteLine("              return result;");
                    sw.WriteLine("          }");
                    sw.WriteLine("          catch (Exception Exc)");
                    sw.WriteLine("          {");
                    sw.WriteLine("              throw Exc;");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");
                    sw.WriteLine();


                    sw.WriteLine("      #endregion");
                    sw.WriteLine();

                    #endregion


                    #region Enumerations

                    sw.WriteLine();
                    sw.WriteLine("      #region Enumerations");
                    sw.WriteLine();
                    sw.WriteLine("      public enum " + item.ToString() + "_enum");
                    sw.WriteLine("      {");
                    sqlRed.Close();
                    sqlRed = sqlCom.ExecuteReader();
                    while (sqlRed.Read())
                    {
                        sw.WriteLine("          " + sqlRed.GetValue(0).ToString() + ",");
                    }
                    sw.WriteLine("      }");
                    sw.WriteLine();
                    sw.WriteLine("      #endregion");
                    sw.WriteLine();

                    #endregion


                    sw.WriteLine("  }");


                    //Business Logic Layer Class////////////////////////////////////////////////////////////
                    if (checkBox2.Checked)
                    {

                        sw.WriteLine();
                        sw.WriteLine("  public class " + item.ToString() + " : Object");
                        sw.WriteLine("  {");



                        #region  Declarations
                        sw.WriteLine();
                        sw.WriteLine("      #region  Declarations");
                        sw.WriteLine();
                        sw.WriteLine("      private dbConnection _dbConnection;");
                        sw.WriteLine();
                        sw.WriteLine("      #endregion");
                        sw.WriteLine();
                        #endregion


                        #region  Constructure/Destructure
                        sw.WriteLine();
                        sw.WriteLine("      #region Constructure/Destructure");
                        sw.WriteLine();
                        sw.WriteLine("      public " + item.ToString() + "(string connectionString)");
                        sw.WriteLine("      {");
                        sw.WriteLine("          _dbConnection = new dbConnection(connectionString);");
                        sw.WriteLine("      }");


                        sw.WriteLine();
                        sw.WriteLine("      public virtual void Dispose()");
                        sw.WriteLine("      {");
                        sw.WriteLine("      }");
                        sw.WriteLine();
                        sw.WriteLine("      #endregion");
                        sw.WriteLine();
                        #endregion


                        #region Business Methods
                        sw.WriteLine();
                        sw.WriteLine("      #region Business Methods");

                        if (itHasPrimaryKey)
                        {
                            sw.WriteLine();
                            sw.WriteLine("      public bool FindByPrimaryKey(ref " + item.ToString() + "Type Value)");
                            sw.WriteLine("      {");
                            sw.WriteLine("          bool Result = false;");
                            sw.WriteLine("          DataTable dt = _dbConnection.SelectQuery(" + item.ToString() + "Static.sqlSelect, " + item.ToString() + "Static.GetSqlParameters(Value));");
                            sw.WriteLine("          if (dt != null)");
                            sw.WriteLine("          {");
                            sw.WriteLine("              if (dt.Rows.Count == 1)");
                            sw.WriteLine("              {");
                            sw.WriteLine("                  Value = " + item.ToString() + "Static.ReadFromDataRow(dt.Rows[0]);");
                            sw.WriteLine("                  Result = true;");
                            sw.WriteLine("              }");
                            sw.WriteLine("          }");
                            sw.WriteLine("          return Result;");
                            sw.WriteLine("      }");
                        }

                        sw.WriteLine();
                        sw.WriteLine("      public bool FindByUniqueField(String _query, ref " + item.ToString() + "Type Value)");
                        sw.WriteLine("      {");
                        sw.WriteLine("          bool Result = false;");
                        sw.WriteLine("          DataTable dt = _dbConnection.SelectQuery(_query, " + item.ToString() + "Static.GetSqlParameters(Value));");
                        sw.WriteLine("          if (dt != null)");
                        sw.WriteLine("          {");
                        sw.WriteLine("              if (dt.Rows.Count == 1)");
                        sw.WriteLine("              {");
                        sw.WriteLine("                  Value = " + item.ToString() + "Static.ReadFromDataRow(dt.Rows[0]);");
                        sw.WriteLine("                  Result = true;");
                        sw.WriteLine("              }");
                        sw.WriteLine("          }");
                        sw.WriteLine("          return Result;");
                        sw.WriteLine("      }");
                        sw.WriteLine();

                        sw.WriteLine("      public DataTable SelectQuery(String _query, " + item.ToString() + "Type Value)");
                        sw.WriteLine("      {");
                        sw.WriteLine("          return _dbConnection.SelectQuery(_query, " + item.ToString() + "Static.GetSqlParameters(Value));");
                        sw.WriteLine("      }");
                        sw.WriteLine();

                        sw.WriteLine("      public DataTable StoredProcedure(String _query, " + item.ToString() + "Type Value)");
                        sw.WriteLine("      {");
                        sw.WriteLine("          return _dbConnection.StoredProcedure(_query, " + item.ToString() + "Static.GetSqlParameters(Value));");
                        sw.WriteLine("      }");
                        sw.WriteLine();

                        sw.WriteLine("      public int ExecuteNonQuery(String _query, " + item.ToString() + "Type Value)");
                        sw.WriteLine("      {");
                        sw.WriteLine("          return _dbConnection.ExecuteNonQuery(_query, " + item.ToString() + "Static.GetSqlParameters(Value));");
                        sw.WriteLine("      }");
                        sw.WriteLine();

                        /*
                        sw.WriteLine("      public int insert(String _query, " + item.ToString() + "Type Value)");
                        sw.WriteLine("      {");
                        sw.WriteLine("          return _dbConnection.InsertQuery(_query, " + item.ToString() + "Static.GetSqlParameters(Value));");
                        sw.WriteLine("      }");
                        sw.WriteLine();

                        sw.WriteLine("      public int delete(String _query, " + item.ToString() + "Type Value)");
                        sw.WriteLine("      {");
                        sw.WriteLine("          return _dbConnection.DeleteQuery(_query, " + item.ToString() + "Static.GetSqlParameters(Value));");
                        sw.WriteLine("      }");
                        sw.WriteLine();

                        sw.WriteLine("      public int update(String _query, " + item.ToString() + "Type Value)");
                        sw.WriteLine("      {");
                        sw.WriteLine("          return _dbConnection.UpdateQuery(_query, " + item.ToString() + "Static.GetSqlParameters(Value));");
                        sw.WriteLine("      }");
                        sw.WriteLine();
                        */

                        sw.WriteLine("      #endregion");
                        sw.WriteLine();

                        #endregion


                        sw.WriteLine("  }");
                        sw.WriteLine();
                    }







                    sw.WriteLine();
                    sw.WriteLine("}");

                    sw.Close();
                    sqlRed.Close();
                    sqlCon.Close();

                }
            }
        }


        private void StructExternalUse(string path)
        {
            if (path != "" && path != null)
            {
                foreach (object item in this.checkedListBox1.CheckedItems)
                {
                    //checkedListBox1.Items.Add(sqlRed.GetValue(0).ToString());
                    sqlCon.Open();
                    sqlCom = new SqlCommand("SELECT c.[name] AS [columnName] , c.[type] AS [columnType] , t.[name] AS [columnTypeName] , c.[length] AS [columnLength] , c.[isnullable] AS [columnIsNullable] " +
                                             "FROM " + dbName + ".sys.syscolumns c INNER JOIN " + dbName + ".sys.systypes t ON t.[xtype] = c.[xtype] " +
                                             "WHERE id = (SELECT id  FROM " + dbName + ".sys.sysobjects WHERE [xtype] = 'U' AND [NAME] = '" + item.ToString() + "') AND t.[name] <> 'sysname' " +
                                             "ORDER BY c.[colid] ;", sqlCon);

                    sqlRed = sqlCom.ExecuteReader();

                    FileInfo MyFile = new FileInfo(path + "\\" + dbName + "_" + item.ToString() + ".cs");
                    StreamWriter sw = MyFile.CreateText();

                    string sql_where = "";
                    string sql_insert = "";
                    string sql_insert_value = "";
                    string sql_update = "";


                    sw.WriteLine("/// <summary>");
                    sw.WriteLine("/// Database Table Struct Class / For External Use / [" + dbName + "]  " + DateTime.Now);
                    sw.WriteLine("/// Copyright © " + DateTime.Now.Year.ToString() + " VIRAWARE INC. , Author: Babak Arjomandi");
                    sw.WriteLine("/// </summary>");
                    sw.WriteLine();
                    sw.WriteLine("namespace " + dbName);
                    sw.WriteLine("{");
                    sw.WriteLine();
                    sw.WriteLine("  using System;");
                    sw.WriteLine("  using System.Collections;");
                    sw.WriteLine("  using System.Collections.Generic;");
                    sw.WriteLine("  using System.ComponentModel;");
                    sw.WriteLine("  using System.Data;");
                    sw.WriteLine("  using System.Data.SqlClient;");
                    sw.WriteLine("  using System.Data.SqlTypes;");
                    sw.WriteLine("  using System.Text;");
                    sw.WriteLine();
                    sw.WriteLine();


                    #region Type Struct

                    sw.WriteLine();
                    sw.WriteLine("  public struct " + item.ToString() + "Type");
                    sw.WriteLine("  {");
                    sw.WriteLine();
                    sw.WriteLine("      #region Type Struct");
                    sw.WriteLine();

                    while (sqlRed.Read())
                    {
                        if (isPrimaryKey(item.ToString(), sqlRed.GetValue(0).ToString()))
                        {
                            // sw.WriteLine("      [Category(\"Primary Key\")]");
                            sql_where += " [" + sqlRed.GetValue(0).ToString() + "]=@" + sqlRed.GetValue(0).ToString() + ",";
                            if (!isIdentityColumn(item.ToString(), sqlRed.GetValue(0).ToString()))
                            {
                                sql_insert += " [" + sqlRed.GetValue(0).ToString() + "],";
                                sql_insert_value += " @" + sqlRed.GetValue(0).ToString() + ",";
                                sql_update += " [" + sqlRed.GetValue(0).ToString() + "]=@" + sqlRed.GetValue(0).ToString() + ",";
                            }
                        }
                        else
                        {
                            //sw.WriteLine("      [Category(\"Column\")]");
                            sql_insert += " [" + sqlRed.GetValue(0).ToString() + "],";
                            sql_insert_value += " @" + sqlRed.GetValue(0).ToString() + ",";
                            sql_update += " [" + sqlRed.GetValue(0).ToString() + "]=@" + sqlRed.GetValue(0).ToString() + ",";
                        }

                        sw.WriteLine("      /// <summary>");
                        int ncharLength = Convert.ToInt32(sqlRed.GetValue(3).ToString());
                        if ((sqlRed.GetValue(2).ToString().IndexOf("nchar") > -1) || (sqlRed.GetValue(2).ToString().IndexOf("nvarchar") > -1))
                        {
                            ncharLength = ncharLength / 2;
                        }
                        sw.WriteLine("      /// " + sqlRed.GetValue(0).ToString() + " " + sqlRed.GetValue(2).ToString() + "(" + ncharLength.ToString() + ") " + getPrimaryKey(item.ToString(), sqlRed.GetValue(0).ToString()) + isNullable(sqlRed.GetValue(4).ToString()));
                        sw.WriteLine("      /// </summary>");
                        sw.WriteLine("      public " + sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "c#") + " " + sqlRed.GetValue(0).ToString() + ";");
                        sw.WriteLine();

                    }

                    if (sql_where != "")
                    {
                        sql_where = sql_where.TrimEnd(',').Trim();
                        sql_where = sql_where.Replace(",", " AND ");
                    }
                    sql_insert = sql_insert.TrimEnd(',').Trim();
                    sql_insert_value = sql_insert_value.TrimEnd(',').Trim();
                    sql_update = sql_update.TrimEnd(',').Trim();

                    sw.WriteLine();
                    sw.WriteLine("      #endregion");
                    sw.WriteLine();
                    sw.WriteLine("  }");
                    sw.WriteLine();
                    #endregion


                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine("  public static class " + item.ToString() + "Static");
                    sw.WriteLine("  {");
                    sw.WriteLine();


                    #region Const String Memebers
                    sw.WriteLine("      #region Const String Memebers");


                    sw.WriteLine();
                    sw.WriteLine("      public static " + item.ToString() + "Type defaultValue");
                    sw.WriteLine("      {");
                    sw.WriteLine("          get");
                    sw.WriteLine("          {");
                    sw.WriteLine("              " + item.ToString() + "Type result;");
                    sqlRed.Close();
                    sqlRed = sqlCom.ExecuteReader();
                    while (sqlRed.Read())
                    {
                        sw.WriteLine("              result." + sqlRed.GetValue(0).ToString() + " = " + sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "defValue") + ";");
                    }
                    sqlRed.Close();
                    sw.WriteLine("              return result;");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");



                    sw.WriteLine();
                    //sw.WriteLine("      public string sqlSelect = \"SELECT * FROM " + item.ToString() + " WHERE " + sql_where + " \";");
                    sw.WriteLine("      public static string sqlSelect");
                    sw.WriteLine("      {");
                    sw.WriteLine("          get");
                    sw.WriteLine("          {");
                    sw.WriteLine("              return \"SELECT * FROM " + item.ToString() + " WHERE " + sql_where + " \";");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");
                    sw.WriteLine();

                    //sw.WriteLine("      public string sqlInsert = \"INSERT INTO " + item.ToString() + " (" + sql_insert + ") VALUES (" + sql_insert_value + ") \";");
                    sw.WriteLine("      public static string sqlInsert");
                    sw.WriteLine("      {");
                    sw.WriteLine("          get");
                    sw.WriteLine("          {");
                    sw.WriteLine("              return \"INSERT INTO " + item.ToString() + " (" + sql_insert + ") VALUES (" + sql_insert_value + ") \";");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");
                    sw.WriteLine();

                    //sw.WriteLine("      public string sqlUpdate = \"UPDATE " + item.ToString() + " SET " + sql_update + " WHERE " + sql_where + " \";");
                    sw.WriteLine("      public static string sqlUpdate");
                    sw.WriteLine("      {");
                    sw.WriteLine("          get");
                    sw.WriteLine("          {");
                    sw.WriteLine("              return \"UPDATE " + item.ToString() + " SET " + sql_update + " WHERE " + sql_where + " \";");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");
                    sw.WriteLine();

                    //sw.WriteLine("      public string sqlDelete = \"DELETE FROM " + item.ToString() + " WHERE " + sql_where + " \";");
                    sw.WriteLine("      public static string sqlDelete");
                    sw.WriteLine("      {");
                    sw.WriteLine("          get");
                    sw.WriteLine("          {");
                    sw.WriteLine("              return \"DELETE FROM " + item.ToString() + " WHERE " + sql_where + " \";");
                    sw.WriteLine("          }");
                    sw.WriteLine("      }");

                    sw.WriteLine();
                    sw.WriteLine("      #endregion");
                    sw.WriteLine();
                    #endregion
                                       


                    #region Enumerations

                    sw.WriteLine();
                    sw.WriteLine("      #region Enumerations");
                    sw.WriteLine();
                    sw.WriteLine("      public enum " + item.ToString() + "_enum");
                    sw.WriteLine("      {");
                    sqlRed.Close();
                    sqlRed = sqlCom.ExecuteReader();
                    while (sqlRed.Read())
                    {
                        sw.WriteLine("          " + sqlRed.GetValue(0).ToString() + ",");
                    }
                    sw.WriteLine("      }");
                    sw.WriteLine();
                    sw.WriteLine("      #endregion");
                    sw.WriteLine();

                    #endregion


                    sw.WriteLine("  }");


                    sw.WriteLine();
                    sw.WriteLine("}");

                    sw.Close();
                    sqlRed.Close();
                    sqlCon.Close();

                }
            }
        }


        private void BusinessLayer(string path)
        {
            if (path != "" && path != null)
            {
                foreach (object item in this.checkedListBox1.CheckedItems)
                {
                    //checkedListBox1.Items.Add(sqlRed.GetValue(0).ToString());
                    sqlCon.Open();
                    sqlCom = new SqlCommand("SELECT c.[name] AS [columnName] , c.[type] AS [columnType] , t.[name] AS [columnTypeName] , c.[length] AS [columnLength] , c.[isnullable] AS [columnIsNullable] " +
                                             "FROM " + dbName + ".sys.syscolumns c INNER JOIN " + dbName + ".sys.systypes t ON t.[xtype] = c.[xtype] " +
                                             "WHERE id = (SELECT id  FROM " + dbName + ".sys.sysobjects WHERE [xtype] = 'U' AND [NAME] = '" + item.ToString() + "') AND t.[name] <> 'sysname' " +
                                             "ORDER BY c.[colid] ;", sqlCon);

                    sqlRed = sqlCom.ExecuteReader();

                    FileInfo MyFile = new FileInfo(path + "\\" + item.ToString() + "Business.cs");
                    StreamWriter sw = MyFile.CreateText();



                    sw.WriteLine("/// <summary>");
                    sw.WriteLine("/// Business Logic Layer Class :: [" + dbName + "]  " + DateTime.Now);
                    sw.WriteLine("/// Copyright © 2013 VIRAWARE INC. , Author: Babak Arjomandi");
                    sw.WriteLine("/// </summary>");
                    sw.WriteLine();

                    sw.WriteLine("#region Fields Info");
                    sw.WriteLine();
                    while (sqlRed.Read())
                    {
                        sw.WriteLine("/// " + sqlRed.GetValue(0).ToString() + " " + sqlRed.GetValue(2).ToString() + "(" + sqlRed.GetValue(3).ToString() + ") " + getPrimaryKey(item.ToString(), sqlRed.GetValue(0).ToString()) + isNullable(sqlRed.GetValue(4).ToString()));
                    }
                    sw.WriteLine();
                    sw.WriteLine("#endregion");
                    sw.WriteLine();
                    sw.WriteLine();


                    sw.WriteLine("namespace " + dbName + "_db");
                    sw.WriteLine("{");
                    sw.WriteLine();
                    sw.WriteLine("  using System;");
                    sw.WriteLine("  using System.Collections;");
                    sw.WriteLine("  using System.Collections.Generic;");
                    sw.WriteLine("  using System.ComponentModel;");
                    sw.WriteLine("  using System.Data;");
                    sw.WriteLine("  using System.Data.SqlClient;");
                    sw.WriteLine("  using System.Data.SqlTypes;");
                    sw.WriteLine("  using System.Text;");
                    sw.WriteLine();
                    sw.WriteLine();

                    sw.WriteLine();
                    sw.WriteLine("  public class " + item.ToString() + " : Object");
                    sw.WriteLine("  {");
                    sw.WriteLine();



                    #region  Declarations
                    sw.WriteLine();
                    sw.WriteLine("      #region  Declarations");
                    //sw.WriteLine();
                    //sw.WriteLine("      private string _connectionString;");
                    sw.WriteLine();
                    sw.WriteLine("      private dbConnection _dbConnection;");
                    sw.WriteLine();
                    sw.WriteLine("      private " + item.ToString() + " _" + item.ToString() + " = new " + item.ToString() + "();");
                    sw.WriteLine();
                    sw.WriteLine("      private " + item.ToString() + "Type _" + item.ToString() + "Type ;");
                    sw.WriteLine();
                    sw.WriteLine("      #endregion");
                    sw.WriteLine();
                    sw.WriteLine();
                    #endregion


                    #region  Constructure/Destructure
                    sw.WriteLine();
                    sw.WriteLine("      #region  Constructure/Destructure");
                    sw.WriteLine();
                    sw.WriteLine("      public " + item.ToString() + "Business(string connectionString)");
                    sw.WriteLine("      {");
                    //sw.WriteLine("          _connectionString = connectionString;");
                    sw.WriteLine("          _dbConnection = new dbConnection(connectionString);");
                    sw.WriteLine("          _" + item.ToString() + "Type = _" + item.ToString() + ".defaultValue;");
                    sw.WriteLine("      }");


                    sw.WriteLine();
                    sw.WriteLine("      public virtual void Dispose()");
                    sw.WriteLine("      {");
                    sw.WriteLine("      }");
                    sw.WriteLine();
                    sw.WriteLine("      #endregion");
                    sw.WriteLine();
                    #endregion


                    #region  Business Methods
                    sw.WriteLine();
                    sw.WriteLine();
                    sw.WriteLine("      #region  Business Methods");
                    sw.WriteLine();
                    sw.WriteLine("      public " + item.ToString() + "Type findByKey(" + item.ToString() + "Type keys)");
                    sw.WriteLine("      {");
                    sw.WriteLine("          DataTable dt = _dbConnection.SelectQuery(_" + item.ToString() + ".sqlSelect, _" + item.ToString() + ".GetSqlParameters(keys));");
                    sw.WriteLine("          if (dt != null)");
                    sw.WriteLine("          {");
                    sw.WriteLine("              if (dt.Rows.Count > 0)");
                    sw.WriteLine("              {");
                    sw.WriteLine("                  return _" + item.ToString() + ".ReadFromDataRow(dt.Rows[0]);");
                    sw.WriteLine("              }");
                    sw.WriteLine("          }");
                    sw.WriteLine("          return _" + item.ToString() + ".defaultValue;");
                    sw.WriteLine("");
                    sw.WriteLine("      }");
                    sw.WriteLine();

                    sw.WriteLine("      public DataTable GetList(" + item.ToString() + "Type keys)");
                    sw.WriteLine("      {");
                    sw.WriteLine("");
                    sw.WriteLine("          return _dbConnection.SelectQuery(_" + item.ToString() + ".sqlSelect, _" + item.ToString() + ".GetSqlParameters(keys));");
                    sw.WriteLine("");
                    sw.WriteLine("      }");
                    sw.WriteLine();

                    sw.WriteLine("      public int insert(" + item.ToString() + "Type val)");
                    sw.WriteLine("      {");
                    sw.WriteLine("");
                    sw.WriteLine("          return _dbConnection.InsertQuery(_" + item.ToString() + ".sqlInsert, _" + item.ToString() + ".GetSqlParameters(val));");
                    sw.WriteLine("");

                    sw.WriteLine("      }");
                    sw.WriteLine();

                    sw.WriteLine("      public int delete(" + item.ToString() + "Type keys)");
                    sw.WriteLine("      {");
                    sw.WriteLine("");
                    sw.WriteLine("          return _dbConnection.DeleteQuery(_" + item.ToString() + ".sqlDelete, _" + item.ToString() + ".GetSqlParameters(keys));");
                    sw.WriteLine("");
                    sw.WriteLine("      }");
                    sw.WriteLine();

                    sw.WriteLine("      public int update(" + item.ToString() + "Type val)");
                    sw.WriteLine("      {");
                    sw.WriteLine("");
                    sw.WriteLine("          return _dbConnection.UpdateQuery(_" + item.ToString() + ".sqlUpdate, _" + item.ToString() + ".GetSqlParameters(val));");
                    sw.WriteLine("");
                    sw.WriteLine("      }");
                    sw.WriteLine();

                    sw.WriteLine("      #endregion");
                    sw.WriteLine();

                    #endregion


                    sw.WriteLine("  }");


                    sw.WriteLine();

                    sw.WriteLine("}");

                    sw.Close();
                    sqlRed.Close();
                    sqlCon.Close();

                }
            }

        }


        private void sqlStoreProcedure(string path)
        {
            if (path != "" && path != null)
            {
                foreach (object item in this.checkedListBox1.CheckedItems)
                {
                    sqlCon.Open();
                    sqlCom = new SqlCommand("SELECT c.[name] AS [columnName] , c.[type] AS [columnType] , t.[name] AS [columnTypeName] , c.[length] AS [columnLength] , c.[isnullable] AS [columnIsNullable] , c.[colstat] AS [isIdentity]" +
                                             "FROM " + dbName + ".sys.syscolumns c INNER JOIN " + dbName + ".sys.systypes t ON t.[xtype] = c.[xtype] " +
                                             "WHERE id = (SELECT id  FROM " + dbName + ".sys.sysobjects WHERE [xtype] = 'U' AND [NAME] = '" + item.ToString() + "') AND t.[name] <> 'sysname' " +
                                             "ORDER BY c.[colid] ;", sqlCon);

                    sqlRed = sqlCom.ExecuteReader();

                    FileInfo MyFile = new FileInfo(path + "\\" + item.ToString() + "_spTemp.sql");
                    StreamWriter sw = MyFile.CreateText();

                    bool itHasPrimaryKey = false;
                    string sql_where = "";
                    string sql_insert = "";
                    string sql_insert_value = "";
                    string sql_update = "";

                    sw.WriteLine("USE [" + dbName + "]");
                    sw.WriteLine("GO");
                    sw.WriteLine("SET ANSI_NULLS ON");
                    sw.WriteLine("GO");
                    sw.WriteLine("SET QUOTED_IDENTIFIER ON");
                    sw.WriteLine("GO");
                    sw.WriteLine("-- =============================================");
                    sw.WriteLine("-- Author:      <VIRAWARE INC. , Babak Arjomandi>");
                    sw.WriteLine("-- Create date: <" + DateTime.Now + ">");
                    sw.WriteLine("-- Description: <This ia a templeate store procedure for using " + item.ToString() + ">");
                    sw.WriteLine("-- =============================================");
                    sw.WriteLine();

                    sw.WriteLine("CREATE PROCEDURE [dbo].[storeProcedure_" + item.ToString() + "]");
                    while (sqlRed.Read())
                    {
                        if (isPrimaryKey(item.ToString(), sqlRed.GetValue(0).ToString()))
                        {
                            itHasPrimaryKey = true;
                            sql_where += " [" + sqlRed.GetValue(0).ToString() + "]=@" + sqlRed.GetValue(0).ToString() + ",";
                            if (sqlRed["isIdentity"].ToString() != "1")
                            {
                                sql_insert += " [" + sqlRed.GetValue(0).ToString() + "],";
                                sql_insert_value += " @" + sqlRed.GetValue(0).ToString() + ",";
                                sql_update += " [" + sqlRed.GetValue(0).ToString() + "]=@" + sqlRed.GetValue(0).ToString() + ",";
                            }
                        }
                        else
                        {
                            if (sqlRed["isIdentity"].ToString() != "1")
                            {
                                sql_insert += " [" + sqlRed.GetValue(0).ToString() + "],";
                                sql_insert_value += " @" + sqlRed.GetValue(0).ToString() + ",";
                                sql_update += " [" + sqlRed.GetValue(0).ToString() + "]=@" + sqlRed.GetValue(0).ToString() + ",";
                            }
                        }
                        
                                                
                        if (sqlRed.GetValue(2).ToString().IndexOf("char") > -1)
                        {
                            if ((sqlRed.GetValue(2).ToString().IndexOf("nchar") > -1) || (sqlRed.GetValue(2).ToString().IndexOf("nvarchar") > -1))
                            {
                                int ncharLength = Convert.ToInt32(sqlRed.GetValue(3).ToString()) / 2;
                                sw.WriteLine("      @" + sqlRed.GetValue(0).ToString() + " " + sqlRed.GetValue(2).ToString() + "(" + ncharLength.ToString() + "), ");
                            }
                            else
                            {
                                sw.WriteLine("      @" + sqlRed.GetValue(0).ToString() + " " + sqlRed.GetValue(2).ToString() + "(" + sqlRed.GetValue(3).ToString() + "), ");
                            }
                        }
                        else
                        {
                            sw.WriteLine("      @" + sqlRed.GetValue(0).ToString() + " " + sqlRed.GetValue(2).ToString() + ", ");
                        }
                    }

                    if (sql_where != "")
                    {
                        if (sql_where.Substring(sql_where.Length - 1, 1) == ",") { sql_where = sql_where.Substring(0, sql_where.Length - 1); }
                        sql_where = sql_where.Replace(",", " AND ");
                    }
                    if (sql_insert.Substring(sql_insert.Length - 1, 1) == ",") { sql_insert = sql_insert.Substring(0, sql_insert.Length - 1); }
                    if (sql_insert_value.Substring(sql_insert_value.Length - 1, 1) == ",") { sql_insert_value = sql_insert_value.Substring(0, sql_insert_value.Length - 1); }
                    if (sql_update.Substring(sql_update.Length - 1, 1) == ",") { sql_update = sql_update.Substring(0, sql_update.Length - 1); }

                    sw.WriteLine("AS");
                    sw.WriteLine("BEGIN");
                    sw.WriteLine("      DECLARE @result int");
                    sw.WriteLine("      SET @result = 0");
                    sw.WriteLine();
                    sw.WriteLine("      DECLARE @now datetime");
                    sw.WriteLine("      SET @now = GETDATE()");
                    sw.WriteLine();
                    sw.WriteLine("      SELECT * FROM " + item.ToString() + " WHERE " + sql_where);
                    sw.WriteLine("      INSERT INTO " + item.ToString() + " (" + sql_insert + ") VALUES (" + sql_insert_value + ")");
                    sw.WriteLine("      UPDATE " + item.ToString() + " SET " + sql_update + " WHERE " + sql_where + "");
                    sw.WriteLine("      DELETE FROM " + item.ToString() + " WHERE " + sql_where + "");
                    sw.WriteLine();
                    sw.WriteLine("      SELECT @result, SCOPE_IDENTITY()");
                    sw.WriteLine();
                    sw.WriteLine("END");

                    sw.Close();
                    sqlRed.Close();
                    sqlCon.Close();

                }
            }

        }


        private void htmlCode(string path)
        {
            if (path != "" && path != null)
            {
                foreach (object item in this.checkedListBox1.CheckedItems)
                {
                    sqlCon.Open();
                    sqlCom = new SqlCommand("SELECT c.[name] AS [columnName] , c.[type] AS [columnType] , t.[name] AS [columnTypeName] , c.[length] AS [columnLength] , c.[isnullable] AS [columnIsNullable] " +
                                             "FROM " + dbName + ".sys.syscolumns c INNER JOIN " + dbName + ".sys.systypes t ON t.[xtype] = c.[xtype] " +
                                             "WHERE id = (SELECT id  FROM " + dbName + ".sys.sysobjects WHERE [xtype] = 'U' AND [NAME] = '" + item.ToString() + "') AND t.[name] <> 'sysname' " +
                                             "ORDER BY c.[colid] ;", sqlCon);

                    sqlRed = sqlCom.ExecuteReader();

                    FileInfo MyFile = new FileInfo(path + "\\" + item.ToString() + "_htmlTemp.txt");
                    StreamWriter sw = MyFile.CreateText();

                    sw.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
                    sw.WriteLine("<html xmlns=\"http://www.w3.org/1999/xhtml\" >");
                    sw.WriteLine("<head runat=\"server\">");
                    sw.WriteLine("  <title>" + dbName + "." + item.ToString() + "</title>");
                    sw.WriteLine("</head>");
                    sw.WriteLine("<body>");
                    sw.WriteLine("  <form id=\"form1\" runat=\"server\">");
                    sw.WriteLine("  <ul>");

                    while (sqlRed.Read())
                    {
                        string tempID = item.ToString() + "_" + sqlRed.GetValue(0).ToString() + "Ctrl";
                        string tempTag = sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "htmlControl").Replace("idControl", tempID);
                        tempTag = tempTag.Replace("hintControl", HumanizeString(sqlRed.GetValue(0).ToString(),true));
                        if (sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "c#").ToLower() == "string")
                        {
                            int ncharLength = Convert.ToInt32(sqlRed.GetValue(3).ToString());
                            if ((sqlRed.GetValue(2).ToString().IndexOf("nchar") > -1) || (sqlRed.GetValue(2).ToString().IndexOf("nvarchar") > -1))
                            {
                                ncharLength = Convert.ToInt32(ncharLength) / 2;
                            }

                            tempTag = tempTag.Replace("lenghtControl", ncharLength.ToString());
                        }

                        //sw.WriteLine("      <div id=\"" + sqlRed.GetValue(2).ToString() + "div\" class=\"cssControlCont\"><span>" + HumanizeString(sqlRed.GetValue(0).ToString(), true) + "</span>" + tempTag + "</div>");
                        //sw.WriteLine("      <div class=\"cssControlCont\"><span>" + HumanizeString(sqlRed.GetValue(0).ToString(), true) + "</span>" + tempTag + "</div>");
                        sw.WriteLine("      <li><label for=\"" + tempID + "\">" + HumanizeString(sqlRed.GetValue(0).ToString(), true) + "</label><div class=\"formcontrols\">" + tempTag + "</div></li>");
                    }

                    sw.WriteLine("  </ul>");
                    sw.WriteLine("  </form>");
                    sw.WriteLine("</body>");
                    sw.WriteLine("</html>");

                    // Javascript Validator
                    sw.WriteLine("");
                    sw.WriteLine("  <script type=\"text/javascript\">");
                    sw.WriteLine("      //Range Expression Validator");
                    sw.WriteLine("      function " + item.ToString() + "Validator() {");
                    sqlRed.Close();
                    sqlRed = sqlCom.ExecuteReader();
                    while (sqlRed.Read())
                    {
                        if (sqlRed.GetValue(4).ToString() == "0")
                        {
                            sw.WriteLine("          if (!checkValidate(document.getElementById(\"" + item.ToString() + "_" + sqlRed.GetValue(0).ToString() + "Ctrl\").value)) { " + item.ToString() + "_" + sqlRed.GetValue(0).ToString() + "Ctrl.focus(); break;} // TO DO MORE ");
                        }
                    }
                    sw.WriteLine("      }");
                    sw.WriteLine("  </script>");

                    // c# code
                    sw.WriteLine("");
                    sw.WriteLine("      <!-- c# Code start");
                    sw.WriteLine("      //Set");
                    sw.WriteLine("      " + dbName + "." + item.ToString() + "Type _" + item.ToString() + " = new " + dbName + "." + item.ToString() + "Type();");
                    sqlRed.Close();
                    sqlRed = sqlCom.ExecuteReader();
                    while (sqlRed.Read())
                    {
                        sw.WriteLine("      _" + item.ToString() + "." + sqlRed.GetValue(0).ToString() + " = " + item.ToString() + "_" + sqlRed.GetValue(0).ToString() + "Ctrl." + sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "htmlControlPro") + ";");
                    }
                    sw.WriteLine("");
                    sw.WriteLine("      //Get");
                    sqlRed.Close();
                    sqlRed = sqlCom.ExecuteReader();
                    while (sqlRed.Read())
                    {
                        sw.WriteLine("      " + item.ToString() + "_" + sqlRed.GetValue(0).ToString() + "Ctrl." + sqlTypeToCShaprType(sqlRed.GetValue(2).ToString(), "htmlControlPro") + " = _" + item.ToString() + "." + sqlRed.GetValue(0).ToString() + ";");
                    }
                    sw.WriteLine("      -->");
                    sw.WriteLine("");

                    sw.Close();
                    sqlRed.Close();
                    sqlCon.Close();
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                DateTime dtStart = DateTime.Now;
                DatabaseLayer(folderBrowserDialog1.SelectedPath);
                if (checkBox3.Checked) { sqlStoreProcedure(folderBrowserDialog1.SelectedPath); }
                if (checkBox4.Checked) { htmlCode(folderBrowserDialog1.SelectedPath); }
                if (checkBox6.Checked) { StructExternalUse(folderBrowserDialog1.SelectedPath); }
                DateTime dtEnd = DateTime.Now;
                TimeSpan diffResult = dtEnd.Subtract(dtStart);
                MessageBox.Show("Done! [Process Duration]: " + diffResult.ToString());
            }
        }


        private void checkBox5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, checkBox5.Checked);
            }
        }


        private string HumanizeString(string source, bool ConvertToUpper)
        {
            StringBuilder sb = new StringBuilder();
            char last = char.MinValue;
            foreach (char c in source)
            {
                if ((char.IsLower(last) && char.IsUpper(c)) || (char.IsLetterOrDigit(last) && !char.IsLetterOrDigit(c)) || (char.IsNumber(last) && char.IsLetter(c)) || (char.IsLetter(last) && char.IsDigit(c)))
                {
                    sb.Append(' ');

                    if (ConvertToUpper && char.IsLetter(c))
                    {
                        sb.Append(c.ToString().ToUpper());
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (last == char.MinValue && char.IsLetter(c) && ConvertToUpper)
                    {
                        sb.Append(c.ToString().ToUpper());
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }

                last = c;
            }
            return sb.ToString();
        }


    }
}
