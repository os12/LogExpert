using System;
using System.Collections.Generic;
using System.Text;

namespace LogExpert
{
    internal class ColumnCache
    {
        private string[] cached_columns = null;
        private ILogLineColumnizer last_columnizer;
        private int last_line = -1;

        internal ColumnCache()
        {
        }

        internal string[] GetColumnsForLine(
            LogfileReader logFileReader, int line,
            ILogLineColumnizer columnizer,
            LogExpert.LogWindow.ColumnizerCallback callback)
        {
            if (this.last_columnizer == columnizer && this.last_line == line &&
                this.cached_columns != null)
                return this.cached_columns;

            string line_data = logFileReader.GetLogLineWithWait(line);
            if (line_data != null)
            {
                callback.LineNum = line;
                this.cached_columns = columnizer.SplitLine(callback, line_data);
                this.last_columnizer = columnizer;
                this.last_line = line;
            }

            return this.cached_columns;
        }
    }
}
