//-----------------------------------------------------------------------
// <copyright file="CsvDocument.cs" company="YuGuan Corporation">
//     Copyright (c) YuGuan Corporation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace DevLib.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a CSV document.
    /// </summary>
    public class CsvDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CsvDocument" /> class.
        /// </summary>
        public CsvDocument()
        {
            this.Table = new DataTable();
        }

        /// <summary>
        /// Gets DataTable instance represents csv data.
        /// </summary>
        public DataTable Table
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the total number of rows.
        /// </summary>
        public int RowCount
        {
            get
            {
                return this.Table.Rows.Count;
            }
        }

        /// <summary>
        /// Gets the total number of columns.
        /// </summary>
        public int ColumnCount
        {
            get
            {
                return this.Table.Columns.Count;
            }
        }

        /// <summary>
        /// Gets all column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                List<string> result = new List<string>(this.ColumnCount);

                foreach (DataColumn item in this.Table.Columns)
                {
                    result.Add(item.ColumnName ?? string.Empty);
                }

                return result;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the loaded csv data has headers.
        /// </summary>
        public bool HasHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the row at the specified index.
        /// </summary>
        /// <param name="rowIndex">The zero-based index of the row to return.</param>
        /// <returns>The specified System.Data.DataRow.</returns>
        public DataRow this[int rowIndex]
        {
            get
            {
                return this.Table.Rows[rowIndex];
            }
        }

        /// <summary>
        /// Gets the column with the specified name.
        /// </summary>
        /// <param name="columnName">The System.Data.DataColumn.ColumnName of the column to return.</param>
        /// <returns>The System.Data.DataColumn in the collection with the specified System.Data.DataColumn.ColumnName; otherwise a null value if the System.Data.DataColumn does not exist.</returns>
        public DataColumn this[string columnName]
        {
            get
            {
                return this.Table.Columns[columnName];
            }
        }

        /// <summary>
        /// Gets or sets the value at the specified row index and the specified column index.
        /// </summary>
        /// <param name="rowIndex">The zero-based index of the row to return.</param>
        /// <param name="columnIndex">The zero-based index of the column to return.</param>
        /// <returns>The value at the specified row index and the specified column index.</returns>
        public string this[int rowIndex, int columnIndex]
        {
            get
            {
                return this.Table.Rows[rowIndex][columnIndex].ToString();
            }

            set
            {
                this.Table.Rows[rowIndex][columnIndex] = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the value at the specified row index and the specified column name.
        /// </summary>
        /// <param name="rowIndex">The zero-based index of the row to return.</param>
        /// <param name="columnName">The System.Data.DataColumn.ColumnName of the column to return.</param>
        /// <returns>The value at the specified row index and the specified column name.</returns>
        public string this[int rowIndex, string columnName]
        {
            get
            {
                return this.Table.Rows[rowIndex][columnName].ToString();
            }

            set
            {
                this.Table.Rows[rowIndex][columnName] = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Loads the csv document from the specified stream.
        /// </summary>
        /// <param name="inStream">The stream containing the csv document to load.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        public void Load(Stream inStream, bool hasHeader = true, char delimiter = ',', char quoteChar = '"')
        {
            StreamReader streamReader = null;

            try
            {
                streamReader = new StreamReader(inStream);
                this.InternalLoad(streamReader, hasHeader, delimiter, quoteChar);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Dispose();
                    streamReader = null;
                }
            }
        }

        /// <summary>
        /// Loads the csv document from the specified file.
        /// </summary>
        /// <param name="filename">The file containing the csv document to load.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        public void Load(string filename, bool hasHeader = true, char delimiter = ',', char quoteChar = '"')
        {
            StreamReader streamReader = null;

            try
            {
                streamReader = File.OpenText(filename);
                this.InternalLoad(streamReader, hasHeader, delimiter, quoteChar);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Dispose();
                    streamReader = null;
                }
            }
        }

        /// <summary>
        /// Loads the csv document from the specified System.IO.TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to feed the csv data into the document.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        public void Load(TextReader reader, bool hasHeader = true, char delimiter = ',', char quoteChar = '"')
        {
            try
            {
                this.InternalLoad(reader, hasHeader, delimiter, quoteChar);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Loads the csv document from the specified string.
        /// </summary>
        /// <param name="csvString">String containing the csv document to load.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        public void LoadCsv(string csvString, bool hasHeader = true, char delimiter = ',', char quoteChar = '"')
        {
            StringReader stringReader = null;

            try
            {
                stringReader = new StringReader(csvString);
                this.InternalLoad(stringReader, hasHeader, delimiter, quoteChar);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                if (stringReader != null)
                {
                    stringReader.Dispose();
                    stringReader = null;
                }
            }
        }

        /// <summary>
        /// Saves the csv document to the specified stream.
        /// </summary>
        /// <param name="outStream">The stream to which you want to save.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="quoteAll">true to quote all cells; otherwise only quote the cell contains delimiter.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        public void Save(Stream outStream, bool hasHeader = true, bool quoteAll = true, char delimiter = ',', char quoteChar = '"')
        {
            this.Save(outStream, hasHeader, quoteAll, delimiter, quoteChar, Environment.NewLine);
        }

        /// <summary>
        /// Saves the csv document to the specified stream.
        /// </summary>
        /// <param name="outStream">The stream to which you want to save.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="quoteAll">true to quote all cells; otherwise only quote the cell contains delimiter.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        /// <param name="newLine">New line characters to use.</param>
        public void Save(Stream outStream, bool hasHeader, bool quoteAll, char delimiter, char quoteChar, string newLine)
        {
            StreamWriter streamWriter = null;

            try
            {
                streamWriter = new StreamWriter(outStream);
                this.InternalSave(streamWriter, hasHeader, quoteAll, delimiter.ToString(), quoteChar.ToString(), newLine);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Dispose();
                    streamWriter = null;
                }
            }
        }

        /// <summary>
        /// Saves the csv document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document.</param>
        /// <param name="overwrite">Whether overwrite exists file</param>
        /// <param name="append">Determines whether data is to be appended to the file. If the file exists and append is false, the file is overwritten. If the file exists and append is true, the data is appended to the file. Otherwise, a new file is created.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="quoteAll">true to quote all cells; otherwise only quote the cell contains delimiter.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        public void Save(string filename, bool overwrite = false, bool append = false, bool hasHeader = true, bool quoteAll = true, char delimiter = ',', char quoteChar = '"')
        {
            this.Save(filename, overwrite, append, hasHeader, quoteAll, delimiter, quoteChar, Environment.NewLine);
        }

        /// <summary>
        /// Saves the csv document to the specified file.
        /// </summary>
        /// <param name="filename">The location of the file where you want to save the document.</param>
        /// <param name="overwrite">Whether overwrite exists file</param>
        /// <param name="append">Determines whether data is to be appended to the file. If the file exists and append is false, the file is overwritten. If the file exists and append is true, the data is appended to the file. Otherwise, a new file is created.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="quoteAll">true to quote all cells; otherwise only quote the cell contains delimiter.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        /// <param name="newLine">New line characters to use.</param>
        public void Save(string filename, bool overwrite, bool append, bool hasHeader, bool quoteAll, char delimiter, char quoteChar, string newLine)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }

            string fullPath = Path.GetFullPath(filename);

            string fullDirectoryPath = Path.GetDirectoryName(fullPath);

            if (!overwrite && File.Exists(fullPath))
            {
                throw new ArgumentException("The specified file already exists.", fullPath);
            }

            if (!Directory.Exists(fullDirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(fullDirectoryPath);
                }
                catch (Exception e)
                {
                    InternalLogger.Log(e);
                    throw;
                }
            }

            StreamWriter streamWriter = null;

            try
            {
                streamWriter = new StreamWriter(fullPath, append);
                this.InternalSave(streamWriter, hasHeader, quoteAll, delimiter.ToString(), quoteChar.ToString(), newLine);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Dispose();
                    streamWriter = null;
                }
            }
        }

        /// <summary>
        /// Saves the csv document to the specified System.IO.TextWriter.
        /// </summary>
        /// <param name="writer">The TextWriter to which you want to save.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="quoteAll">true to quote all cells; otherwise only quote the cell contains delimiter.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        public void Save(TextWriter writer, bool hasHeader = true, bool quoteAll = true, char delimiter = ',', char quoteChar = '"')
        {
            this.Save(writer, hasHeader, quoteAll, delimiter, quoteChar, Environment.NewLine);
        }

        /// <summary>
        /// Saves the csv document to the specified System.IO.TextWriter.
        /// </summary>
        /// <param name="writer">The TextWriter to which you want to save.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="quoteAll">true to quote all cells; otherwise only quote the cell contains delimiter.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        /// <param name="newLine">New line characters to use.</param>
        public void Save(TextWriter writer, bool hasHeader, bool quoteAll, char delimiter, char quoteChar, string newLine)
        {
            try
            {
                this.InternalSave(writer, hasHeader, quoteAll, delimiter.ToString(), quoteChar.ToString(), newLine);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Loads the csv document from the specified System.IO.TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to feed the csv data into the document.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        private void InternalLoad(TextReader reader, bool hasHeader, char delimiter, char quoteChar)
        {
            this.Table.Clear();
            this.HasHeader = hasHeader;
            StringBuilder cell = new StringBuilder();
            List<string> row = new List<string>();
            bool addedHeader = false;
            bool inColumn = false;
            bool inQuotes = false;

            while (reader.Peek() >= 0)
            {
                string line = reader.ReadLine();

                if (!string.IsNullOrEmpty(line))
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        char character = line[i];

                        if (!inColumn)
                        {
                            if (character.Equals(delimiter))
                            {
                                row.Add(string.Empty);
                                continue;
                            }

                            if (character.Equals(quoteChar))
                            {
                                inQuotes = true;
                            }
                            else
                            {
                                cell.Append(character);
                            }

                            inColumn = true;
                            continue;
                        }

                        if (inQuotes)
                        {
                            if (character.Equals(quoteChar) && ((line.Length > (i + 1) && line[i + 1].Equals(delimiter)) || ((i + 1) == line.Length)))
                            {
                                inQuotes = false;
                                inColumn = false;
                                i++;
                            }
                            else if (character.Equals(quoteChar) && line.Length > (i + 1) && line[i + 1].Equals(quoteChar))
                            {
                                i++;
                            }
                        }
                        else if (character.Equals(delimiter))
                        {
                            inColumn = false;
                        }

                        if (!inColumn)
                        {
                            row.Add(cell.ToString());
                            cell.Remove(0, cell.Length);
                        }
                        else
                        {
                            cell.Append(character);
                        }
                    }

                    if (inColumn)
                    {
                        row.Add(cell.ToString());
                    }

                    if (addedHeader)
                    {
                        DataRow dataRow = this.Table.NewRow();

                        for (int i = 0; i < row.Count; i++)
                        {
                            dataRow[i] = row[i];
                        }

                        this.Table.Rows.Add(dataRow);
                    }
                    else
                    {
                        if (hasHeader)
                        {
                            for (int i = 0; i < row.Count; i++)
                            {
                                this.Table.Columns.Add(row[i]);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < row.Count; i++)
                            {
                                this.Table.Columns.Add(i.ToString());
                            }

                            DataRow dataRow = this.Table.NewRow();

                            for (int i = 0; i < row.Count; i++)
                            {
                                dataRow[i] = row[i];
                            }

                            this.Table.Rows.Add(dataRow);
                        }

                        addedHeader = true;
                    }

                    cell.Remove(0, cell.Length);
                    row.Clear();
                }
            }
        }

        /// <summary>
        /// Saves the csv document to the specified System.IO.TextWriter.
        /// </summary>
        /// <param name="writer">The TextWriter to which you want to save.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="quoteAll">true to quote all cells; otherwise only quote the cell contains delimiter.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="quoteChar">Character to use when quoting.</param>
        /// <param name="newLine">New line characters to use.</param>
        private void InternalSave(TextWriter writer, bool hasHeader, bool quoteAll, string delimiter, string quoteChar, string newLine)
        {
            writer.NewLine = newLine;

            if (hasHeader)
            {
                List<string> headers = this.ColumnNames;

                string firstColumn = headers[0];

                if (firstColumn.Contains(delimiter) || quoteAll)
                {
                    firstColumn = quoteChar + firstColumn + quoteChar;
                }

                writer.Write(firstColumn);

                for (int index = 1; index < headers.Count; index++)
                {
                    string cell = headers[index];

                    if (cell.Contains(delimiter) || quoteAll)
                    {
                        cell = quoteChar + cell + quoteChar;
                    }

                    writer.Write(delimiter + cell);
                }

                writer.Write(newLine);
            }

            foreach (DataRow row in this.Table.Rows)
            {
                string firstColumn = row.ItemArray[0].ToString();

                if (firstColumn.Contains(delimiter) || quoteAll)
                {
                    firstColumn = quoteChar + firstColumn + quoteChar;
                }

                writer.Write(firstColumn);

                for (int index = 1; index < row.ItemArray.Length; index++)
                {
                    string cell = row.ItemArray[index].ToString();

                    if (cell.Contains(delimiter) || quoteAll)
                    {
                        cell = quoteChar + cell + quoteChar;
                    }

                    writer.Write(delimiter + cell);
                }

                writer.Write(newLine);
            }

            writer.Flush();
        }
    }
}
