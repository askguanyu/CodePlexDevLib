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
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a CSV document.
    /// </summary>
    public class CsvDocument : IDisposable
    {
        /// <summary>
        /// Field _disposed.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvDocument" /> class.
        /// </summary>
        public CsvDocument()
        {
            this.Table = new List<List<string>>();
            this.HeaderColumns = new List<string>();
        }

        /// <summary>
        /// Gets instance represents csv data.
        /// </summary>
        public List<List<string>> Table
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
                return this.Table.Count;
            }
        }

        /// <summary>
        /// Gets the total number of columns.
        /// </summary>
        public int HeaderColumnCount
        {
            get
            {
                return this.HeaderColumns.Count;
            }
        }

        /// <summary>
        /// Gets header column names.
        /// </summary>
        public List<string> HeaderColumns
        {
            get;
            private set;
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
        /// <returns>The specified row.</returns>
        public List<string> this[int rowIndex]
        {
            get
            {
                return this.Table[rowIndex];
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
                return this.Table[rowIndex][columnIndex];
            }

            set
            {
                this.Table[rowIndex][columnIndex] = value;
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
                int columnIndex = this.HeaderColumns.FindIndex(i => i.Equals(columnName, StringComparison.Ordinal));

                if (columnIndex >= 0)
                {
                    return this.Table[rowIndex][columnIndex];
                }
                else
                {
                    throw new ArgumentException("The column specified by columnName cannot be found.");
                }
            }

            set
            {
                int columnIndex = this.HeaderColumns.FindIndex(i => i.Equals(columnName, StringComparison.Ordinal));

                if (columnIndex >= 0)
                {
                    this.Table[rowIndex][columnIndex] = value;
                }
                else
                {
                    throw new ArgumentException("The column specified by columnName cannot be found.");
                }
            }
        }

        /// <summary>
        /// Loads the csv document from the specified stream.
        /// </summary>
        /// <param name="inStream">The stream containing the csv document to load.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="qualifier">Character to use when quoting.</param>
        public void Load(Stream inStream, bool hasHeader = true, char delimiter = ',', char qualifier = '"')
        {
            this.CheckDisposed();

            StreamReader streamReader = null;

            try
            {
                streamReader = new StreamReader(inStream);
                this.InternalLoad(streamReader, hasHeader, delimiter, qualifier);
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
        /// <param name="qualifier">Character to use when quoting.</param>
        public void Load(string filename, bool hasHeader = true, char delimiter = ',', char qualifier = '"')
        {
            this.CheckDisposed();

            StreamReader streamReader = null;

            try
            {
                streamReader = File.OpenText(filename);
                this.InternalLoad(streamReader, hasHeader, delimiter, qualifier);
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
        /// <param name="qualifier">Character to use when quoting.</param>
        public void Load(TextReader reader, bool hasHeader = true, char delimiter = ',', char qualifier = '"')
        {
            this.CheckDisposed();

            try
            {
                this.InternalLoad(reader, hasHeader, delimiter, qualifier);
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
        /// <param name="qualifier">Character to use when quoting.</param>
        public void LoadCsv(string csvString, bool hasHeader = true, char delimiter = ',', char qualifier = '"')
        {
            this.CheckDisposed();

            StringReader stringReader = null;

            try
            {
                stringReader = new StringReader(csvString);
                this.InternalLoad(stringReader, hasHeader, delimiter, qualifier);
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
        /// <param name="qualifier">Character to use when quoting.</param>
        public void Save(Stream outStream, bool hasHeader = true, bool quoteAll = false, char delimiter = ',', char qualifier = '"')
        {
            this.CheckDisposed();

            this.Save(outStream, hasHeader, quoteAll, delimiter, qualifier, Environment.NewLine);
        }

        /// <summary>
        /// Saves the csv document to the specified stream.
        /// </summary>
        /// <param name="outStream">The stream to which you want to save.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="quoteAll">true to quote all cells; otherwise only quote the cell contains delimiter.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="qualifier">Character to use when quoting.</param>
        /// <param name="newLine">New line characters to use.</param>
        public void Save(Stream outStream, bool hasHeader, bool quoteAll, char delimiter, char qualifier, string newLine)
        {
            this.CheckDisposed();

            StreamWriter streamWriter = null;

            try
            {
                streamWriter = new StreamWriter(outStream);
                this.InternalSave(streamWriter, hasHeader, quoteAll, delimiter.ToString(), qualifier.ToString(), newLine);
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
        /// <param name="qualifier">Character to use when quoting.</param>
        public void Save(string filename, bool overwrite = false, bool append = false, bool hasHeader = true, bool quoteAll = false, char delimiter = ',', char qualifier = '"')
        {
            this.CheckDisposed();

            this.Save(filename, overwrite, append, hasHeader, quoteAll, delimiter, qualifier, Environment.NewLine);
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
        /// <param name="qualifier">Character to use when quoting.</param>
        /// <param name="newLine">New line characters to use.</param>
        public void Save(string filename, bool overwrite, bool append, bool hasHeader, bool quoteAll, char delimiter, char qualifier, string newLine)
        {
            this.CheckDisposed();

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
                this.InternalSave(streamWriter, hasHeader, quoteAll, delimiter.ToString(), qualifier.ToString(), newLine);
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
        /// <param name="qualifier">Character to use when quoting.</param>
        public void Save(TextWriter writer, bool hasHeader = true, bool quoteAll = false, char delimiter = ',', char qualifier = '"')
        {
            this.CheckDisposed();

            this.Save(writer, hasHeader, quoteAll, delimiter, qualifier, Environment.NewLine);
        }

        /// <summary>
        /// Saves the csv document to the specified System.IO.TextWriter.
        /// </summary>
        /// <param name="writer">The TextWriter to which you want to save.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="quoteAll">true to quote all cells; otherwise only quote the cell contains delimiter.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="qualifier">Character to use when quoting.</param>
        /// <param name="newLine">New line characters to use.</param>
        public void Save(TextWriter writer, bool hasHeader, bool quoteAll, char delimiter, char qualifier, string newLine)
        {
            this.CheckDisposed();

            try
            {
                this.InternalSave(writer, hasHeader, quoteAll, delimiter.ToString(), qualifier.ToString(), newLine);
            }
            catch (Exception e)
            {
                InternalLogger.Log(e);
                throw;
            }
        }

        /// <summary>
        /// Gets the DataTable.
        /// </summary>
        /// <param name="startIndex">The zero-based start index.</param>
        /// <returns>DataTable instance.</returns>
        public DataTable GetDataTable(int startIndex = 0)
        {
            return this.GetDataTable(startIndex, this.RowCount);
        }

        /// <summary>
        /// Gets the DataTable.
        /// </summary>
        /// <param name="startIndex">The zero-based start index.</param>
        /// <param name="count">The count of row to get.</param>
        /// <returns>DataTable instance.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Reviewed.")]
        public DataTable GetDataTable(int startIndex, int count)
        {
            if (startIndex < 0 || startIndex >= this.RowCount || count < 0 || startIndex + count > this.RowCount)
            {
                return null;
            }

            DataTable result = new DataTable();

            foreach (var item in this.HeaderColumns)
            {
                result.Columns.Add(item);
            }

            int currentColumnCount = this.HeaderColumnCount;

            for (int i = startIndex; i < count; i++)
            {
                var row = this.Table[i];

                if (row.Count > currentColumnCount)
                {
                    int extraColumnCount = row.Count - currentColumnCount;

                    currentColumnCount = row.Count;

                    for (int j = 0; j < extraColumnCount; j++)
                    {
                        result.Columns.Add();
                    }
                }

                result.Rows.Add(row.ToArray());
            }

            return result;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="CsvDocument" /> class.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="CsvDocument" /> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="CsvDocument" /> class.
        /// protected virtual for non-sealed class; private for sealed class.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;

            if (disposing)
            {
                // dispose managed resources
                ////if (managedResource != null)
                ////{
                ////    managedResource.Dispose();
                ////    managedResource = null;
                ////}

                if (this.Table != null)
                {
                    this.Table.Clear();
                    this.Table = null;
                }

                if (this.HeaderColumns != null)
                {
                    this.HeaderColumns.Clear();
                    this.HeaderColumns = null;
                }
            }

            // free native resources
            ////if (nativeResource != IntPtr.Zero)
            ////{
            ////    Marshal.FreeHGlobal(nativeResource);
            ////    nativeResource = IntPtr.Zero;
            ////}
        }

        /// <summary>
        /// Method CheckDisposed.
        /// </summary>
        private void CheckDisposed()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException("DevLib.Csv.CsvDocument");
            }
        }

        /// <summary>
        /// Loads the csv document from the specified System.IO.TextReader.
        /// </summary>
        /// <param name="reader">The TextReader used to feed the csv data into the document.</param>
        /// <param name="hasHeader">true if the csv has a header row; otherwise false.</param>
        /// <param name="delimiter">Delimiter character to use.</param>
        /// <param name="qualifier">Character to use when quoting.</param>
        private void InternalLoad(TextReader reader, bool hasHeader, char delimiter, char qualifier)
        {
            this.Table.Clear();
            this.HeaderColumns.Clear();
            this.HasHeader = hasHeader;

            bool addedHeader = false;

            while (reader.Peek() >= 0)
            {
                string line = reader.ReadLine();

                if (!string.IsNullOrEmpty(line))
                {
                    List<string> row = this.SplitNested(line, delimiter, qualifier);

                    if (addedHeader)
                    {
                        this.Table.Add(row);
                    }
                    else
                    {
                        if (hasHeader)
                        {
                            this.HeaderColumns.AddRange(row);
                        }
                        else
                        {
                            for (int i = 0; i < row.Count; i++)
                            {
                                this.HeaderColumns.Add("Column" + i.ToString());
                            }

                            this.Table.Add(row);
                        }

                        addedHeader = true;
                    }
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
        /// <param name="qualifier">Character to use when quoting.</param>
        /// <param name="newLine">New line characters to use.</param>
        private void InternalSave(TextWriter writer, bool hasHeader, bool quoteAll, string delimiter, string qualifier, string newLine)
        {
            writer.NewLine = newLine;

            if (hasHeader)
            {
                string firstColumn = this.HeaderColumns[0];

                if (firstColumn.Contains(delimiter) || quoteAll)
                {
                    firstColumn = qualifier + firstColumn + qualifier;
                }

                writer.Write(firstColumn);

                for (int index = 1; index < this.HeaderColumns.Count; index++)
                {
                    string cell = this.HeaderColumns[index];

                    if (cell.Contains(delimiter) || quoteAll)
                    {
                        cell = qualifier + cell + qualifier;
                    }

                    writer.Write(delimiter + cell);
                }

                writer.Write(newLine);
            }

            foreach (var row in this.Table)
            {
                string firstColumn = row[0];

                if (firstColumn.Contains(delimiter) || quoteAll)
                {
                    firstColumn = qualifier + firstColumn + qualifier;
                }

                writer.Write(firstColumn);

                for (int index = 1; index < row.Count; index++)
                {
                    string cell = row[index];

                    if (cell.Contains(delimiter) || quoteAll)
                    {
                        cell = qualifier + cell + qualifier;
                    }

                    writer.Write(delimiter + cell);
                }

                writer.Write(newLine);
            }

            writer.Flush();
        }

        /// <summary>
        /// Splits string by a specified delimiter and keep nested string with a specified qualifier.
        /// </summary>
        /// <param name="source">Source string.</param>
        /// <param name="delimiter">Delimiter character.</param>
        /// <param name="qualifier">Qualifier character.</param>
        /// <returns>A list whose elements contain the substrings in this instance that are delimited by the delimiter.</returns>
        private List<string> SplitNested(string source, char delimiter, char qualifier)
        {
            List<string> result = new List<string>();

            StringBuilder itemStringBuilder = new StringBuilder();
            bool inItem = false;
            bool inQuotes = false;

            for (int i = 0; i < source.Length; i++)
            {
                char character = source[i];

                if (!inItem)
                {
                    if (character == delimiter)
                    {
                        result.Add(string.Empty);
                        continue;
                    }

                    if (character == qualifier)
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        itemStringBuilder.Append(character);
                    }

                    inItem = true;
                    continue;
                }

                if (inQuotes)
                {
                    if (character == qualifier && ((source.Length > (i + 1) && source[i + 1] == delimiter) || ((i + 1) == source.Length)))
                    {
                        inQuotes = false;
                        inItem = false;
                        i++;
                    }
                    else if (character == qualifier && source.Length > (i + 1) && source[i + 1] == qualifier)
                    {
                        i++;
                    }
                }
                else if (character == delimiter)
                {
                    inItem = false;
                }

                if (!inItem)
                {
                    result.Add(itemStringBuilder.ToString());
                    itemStringBuilder.Remove(0, itemStringBuilder.Length);
                }
                else
                {
                    itemStringBuilder.Append(character);
                }
            }

            if (inItem)
            {
                result.Add(itemStringBuilder.ToString());
            }

            return result;
        }
    }
}
