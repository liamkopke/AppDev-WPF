﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Budget;


//
// NOTE: Learning to read/write blobs taken from the following web site
// https://www.akadia.com/services/dotnet_read_write_blob.html
// https://www.codeproject.com/Articles/66271/Working-with-SQL-Server-BLOB-Data-in-NET
//

namespace EnterpriseBudget.Model
{
    /// <summary>
    /// Handles the sqlite files stored in the sql server
    /// </summary>
    public class DepartmentBudgets
    {
        private String sqliteFileName = "deptBudget.db";
        private String sqliteFileNameToSend = "deptBudgetToSend.db";
        private String sqliteFileNameFromServer = "deptBudgetFromServer.db";
        private String appName = "EnterpriseBudget";
        private String sPath;
        private HomeBudget homeBudget;

        /// <summary>
        /// Constructor
        /// </summary>
        public DepartmentBudgets()
        {
            sPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Directory.CreateDirectory($"{sPath}\\{appName}");
        }

        /// <summary>
        /// close all required connections
        /// </summary>
        public void Close()
        {
            if (homeBudget != null)
            {
                homeBudget.CloseDB();
            }
        }

        public HomeBudget GetHomeBudget()
        {
            return homeBudget;
        }

        public string getFilePath()
        {
            // returns the path to the downloaded budget file
            return $"{sPath}\\{appName}\\{sqliteFileName}";
        }

        /// <summary>
        /// Get the sqlite file from sqlserver, and open budget file
        /// </summary>
        /// <param name="departmentID">The ID of the department whose budget we want to open</param>
        /// <returns>true if successful, false otherwise</returns>
        public bool DownLoadAndOpenDepartmentBudgetFile(int departmentID) 
        {
            try {
                var path = $"{sPath}\\{appName}\\{sqliteFileName}";
                if (File.Exists(path))
                {
                    path = $"{sPath}\\{appName}\\{sqliteFileNameFromServer}";
                }
                ReadAndSaveBlobFromSQLServer(Connection.cnn, "deptBudgets", "sqlitefile", $"deptId={departmentID}",path);
                homeBudget = new HomeBudget(path, false);
                return true;
            }
            catch { return false; }
        }

        public bool SaveToSQLServer(int departmentID)
        {
            try
            {
                if (!File.Exists($"{sPath}\\{appName}\\{sqliteFileNameToSend}"))
                {
                    File.Copy($"{sPath}\\{appName}\\{sqliteFileName}", $"{sPath}\\{appName}\\{sqliteFileNameToSend}");
                }
                else
                {
                    File.Delete($"{sPath}\\{appName}\\{sqliteFileNameToSend}");
                    File.Copy($"{sPath}\\{appName}\\{sqliteFileName}", $"{sPath}\\{appName}\\{sqliteFileNameToSend}");
                }

                var path = $"{sPath}\\{appName}\\{sqliteFileNameToSend}";
                WriteBlobToSQLServer(Connection.cnn, path, "deptBudgets", "sqlitefile", $"deptId={departmentID}");
                File.Delete($"{sPath}\\{appName}\\{sqliteFileNameToSend}"); // delete file after for cleanup
                return true;
            }
            catch { return false; }
        }

        // write binary data to SQLServer
        // basically save to sql server
        private void WriteBlobToSQLServer(SqlConnection cnn, string fileName, string tableName, string columnName, string whereCondition)
        {
            SqlCommand writeBlob = cnn.CreateCommand();
            writeBlob.CommandText = $"UPDATE {tableName} set {columnName} = @data where {whereCondition} ";
            writeBlob.Parameters.AddWithValue("@data", File.ReadAllBytes(fileName));    // dies here
            writeBlob.ExecuteNonQuery();
        }

        // **** Read BLOB from the Database and save it on the Filesystem
        // NOTE: columnName should specify 1 column only.  Do NOT try to fool it by setting columnName to "col1, col2". 
        //       Chaos will ensue :(
        // NOTE: the whereCondition should be sufficient to ensure that only 1 row will be returned. Failing that...
        //       Chaos will ensue :(
        private void ReadAndSaveBlobFromSQLServer(SqlConnection cnn, string tableName, string columnName, string whereCondition, string outputFileName)
        {
            SqlCommand getBinaryData = new SqlCommand(
                $"SELECT {columnName} " +
                $"FROM {tableName} " +
                $"WHERE {whereCondition} ", cnn);

            FileStream fs;                          // Writes the BLOB to a file.
            BinaryWriter bw;                        // Streams the BLOB to the FileStream object.
            int bufferSize = 512;                   // Size of the BLOB buffer.
            byte[] outbyte = new byte[bufferSize];  // The BLOB byte[] buffer to be filled by GetBytes.
            long retval;                            // The bytes returned from GetBytes.
            long startIndex = 0;                    // The starting position in the BLOB output.

            SqlDataReader myReader = getBinaryData.ExecuteReader(CommandBehavior.SequentialAccess);

            while (myReader.Read())
            {
                fs = new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.Write);
                bw = new BinaryWriter(fs);

                // Reset the starting byte for the new BLOB.
                startIndex = 0;

                // Read the bytes into outbyte[] and retain the number of bytes returned.
                retval = myReader.GetBytes(0, startIndex, outbyte, 0, bufferSize);

                // Continue reading and writing while there are bytes beyond the size of the buffer.
                while (retval == bufferSize)
                {
                    bw.Write(outbyte);
                    bw.Flush();

                    // Reposition the start index to the end of the last buffer and fill the buffer.
                    startIndex += bufferSize;
                    retval = myReader.GetBytes(0, startIndex, outbyte, 0, bufferSize);
                }

                // Write the remaining buffer.
                bw.Write(outbyte, 0, (int)retval);
                bw.Flush();

                // Close the output file.
                bw.Close();
                fs.Close();
            }

            // Close the reader and the connection.
            myReader.Close();
        }

        public List<KeyValuePair<string, decimal>> GetExpenses(int deptId)
        {
            List < KeyValuePair<string, decimal> > categories = new List<KeyValuePair<string, decimal>>();
            SqlCommand command = Connection.cnn.CreateCommand();
            command.CommandText = $"Select bc.name, bl.limit from budgetCategoryLimits bl, budgetCategories bc WHERE bl.deptId = {deptId} and bc.id = bl.catId;";
            SqlDataReader myReader = command.ExecuteReader();

            while (myReader.Read())
            {
                KeyValuePair<string, decimal> kvp = new KeyValuePair<string, decimal>(myReader.GetString(0), (decimal)myReader.GetDouble(1));
                categories.Add(kvp);
            }

            myReader.Close();

            return categories;
        }
    }
}
