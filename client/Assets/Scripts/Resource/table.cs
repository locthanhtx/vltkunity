
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace game.resource
{
    public class Table
    {
        private int rowTotal;
        private string[] rowLiteralVector;
        private Dictionary<int, Dictionary<int, string>> rowKeyCacheValue; // row index => header index => value as string
        private Dictionary<string, int> headerKeyIndex; // header column key => index
        private Dictionary<int, string> headerIndexKey; // header column index => key

        private readonly resource.Buffer.Encoding encoding;

        public Table(resource.Buffer _buffer)
        {
            this.encoding = _buffer.GetEncoding();
            this.Initialize(_buffer.GetString());
        }

        private void Initialize(string _literalData)
        {
            this.rowLiteralVector = _literalData.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            this.rowTotal = this.rowLiteralVector.Length;
            this.rowKeyCacheValue = new();
            this.headerKeyIndex = new Dictionary<string, int>();
            this.headerIndexKey = new Dictionary<int, string>();

            if (this.rowTotal <= 0)
            {
                return;
            }

            string[] headerKeyVector = rowLiteralVector[0].Split('\t');

            for (int index = 0; index < headerKeyVector.Length; index++)
            {
                string headerKey = headerKeyVector[index];

                if (index == headerKeyVector.Length - 1)
                {
                    headerKey = Table.RemoveSpecialSymbol(headerKey);
                }

                if(this.headerKeyIndex.ContainsKey(headerKey) == true)
                {
                    headerKey = headerKey + "." + index;
                }

                this.headerKeyIndex[headerKey] = index;
                this.headerIndexKey[index] = headerKey;
            }
        }

        private static string RemoveSpecialSymbol(string _string)
        {
            string result = _string;
            char[] removeCharList = { '\r', '\n', '\t' };
            int removePosition;

            while ((removePosition = result.IndexOfAny(removeCharList)) != -1)
            {
                result = result.Remove(removePosition, 1);
            }

            return result;
        }

        private string GetString(int _headerIndex, int _rowIndex)
        {
            if (this.rowTotal <= 0
                || this.rowTotal <= _rowIndex)
            {
                return string.Empty;
            }

            if (_rowIndex <= 0)
            {
                if (this.headerIndexKey.ContainsKey(_headerIndex))
                {
                    return headerIndexKey[_headerIndex];
                }
                else
                {
                    return string.Empty;
                }
            }

            if (this.rowKeyCacheValue.ContainsKey(_rowIndex) == false)
            {
                string[] rowSplited = this.rowLiteralVector[_rowIndex].Split('\t');

                this.rowLiteralVector[_rowIndex] = string.Empty;
                this.rowKeyCacheValue[_rowIndex] = new();

                int indexer = 0;
                foreach (var indexHeaderPair in this.headerKeyIndex)
                {
                    if (indexHeaderPair.Value >= rowSplited.Length)
                    {
                        this.rowKeyCacheValue[_rowIndex][indexHeaderPair.Value] = string.Empty;
                        continue;
                    }

                    string value = rowSplited[indexHeaderPair.Value];

                    if (indexer == rowSplited.Length - 1)
                    {
                        value = Table.RemoveSpecialSymbol(value);
                    }

                    this.rowKeyCacheValue[_rowIndex][indexHeaderPair.Value] = value;
                    indexer++;
                }
            }

            if(this.rowKeyCacheValue[_rowIndex].ContainsKey(_headerIndex) == false)
            {
                return string.Empty;
            }

            return this.rowKeyCacheValue[_rowIndex][_headerIndex];
        }

        private string GetString(string _headerKey, int _rowIndex)
        {
            if (this.headerKeyIndex.ContainsKey(_headerKey) == false)
            {
                return string.Empty;
            }

            return this.GetString(this.headerKeyIndex[_headerKey], _rowIndex);
        }

        private int GetInt(string _headerKey, int _rowIndex)
        {
            string value = this.GetString(_headerKey, _rowIndex);

            return Table.ParseIntOrDefault(value, -1);
        }

        private int GetInt(int _columnIndex, int _rowIndex, int _default)
        {
            string value = this.GetString(_columnIndex, _rowIndex);

            return Table.ParseIntOrDefault(value, _default);
        }

        private static int ParseIntOrDefault(string value, int defaultValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            Match match = Regex.Match(value, "-?\\d+");
            if (!match.Success)
            {
                return defaultValue;
            }

            return int.TryParse(match.Value, out int result) ? result : defaultValue;
        }

        public resource.Buffer.Encoding GetEncoding()
        {
            return this.encoding;
        }

        public int HeaderCount
        {
            get { return this.headerKeyIndex.Count; }
        }

        public int RowCount
        {
            get { return this.rowTotal; }
        }

        public bool IsEmpty()
        {
            return this.rowTotal <= 0;
        }

        public bool IsNotEmpty()
        {
            return this.rowTotal > 0;
        }

        public int FindRowIndex(string _headerKey, string _data)
        {
            int result = -1;

            for (int rowIndex = 0; rowIndex < this.rowTotal; rowIndex++)
            {
                if (this.GetString(_headerKey, rowIndex).CompareTo(_data) == 0)
                {
                    result = rowIndex;
                    break;
                }
            }

            return result;
        }

        public List<string> GetHeaderKeyList()
        {
            List<string> result = new();

            foreach (KeyValuePair<string, int> pairIndex in this.headerKeyIndex)
            {
                result.Add(pairIndex.Key);
            }

            return result;
        }

        public Dictionary<string, int> GetHeaderKeyIndexPair()
        {
            return this.headerKeyIndex;
        }

        public int GetHeaderIndex(string _headerKey)
        {
            if(this.headerKeyIndex.ContainsKey(_headerKey))
            {
                return this.headerKeyIndex[_headerKey];
            }

            return -1;
        }

        public string GetHeaderKey(int _headerIndex)
        {
            if(this.headerIndexKey.ContainsKey(_headerIndex))
            {
                return this.headerIndexKey[_headerIndex];
            }

            return string.Empty;
        }

        public Dictionary<int, Dictionary<int, string>> GetRowCacheValue()
        {
            return this.rowKeyCacheValue;
        }

        /*  supporting
         *  
         *  string
         *  int
         */

        public Typename Get<Typename>(string _headerKey, int _rowIndex)
        {
            Type requestType = typeof(Typename);

            if(requestType == typeof(string)) return (Typename)(object)this.GetString(_headerKey, _rowIndex);
            if(requestType == typeof(int)) return (Typename)(object)this.GetInt(_headerKey, _rowIndex);

            return (Typename)(object)null;
        }

        public Typename Get<Typename>(int _columnIndex, int _rowIndex)
        {
            Type requestType = typeof(Typename);

            if (requestType == typeof(string)) return (Typename)(object)this.GetString(_columnIndex, _rowIndex);
            if (requestType == typeof(int)) return (Typename)(object)this.GetInt(_columnIndex, _rowIndex, -1);

            return (Typename)(object)null;
        }

        public Typename Get<Typename>(int _columnIndex, int _rowIndex, Typename _default)
        {
            Type requestType = typeof(Typename);

            if (requestType == typeof(int)) return (Typename)(object)this.GetInt(_columnIndex, _rowIndex, (int)(object)_default);

            return (Typename)(object)null;
        }
    }
}
