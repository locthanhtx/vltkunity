
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace game.resource
{
    class Ini
    {
        private readonly Dictionary<string, Dictionary<string, string>> mapping; // section => key => value

        public Ini(resource.Buffer _buffer) : this(_buffer.GetString())
        {
        }

        public Ini(string _literalData)
        {
            this.mapping = new Dictionary<string, Dictionary<string, string>>();
            this.Initialize(_literalData ?? string.Empty);
        }

        private void Initialize(string _literalData)
        {
            string[] rowVector = _literalData.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            string lastSectionName = string.Empty;

            for (int indexRow = 0; indexRow < rowVector.Length; indexRow++)
            {
                string rowLiteral = rowVector[indexRow];
                rowLiteral = rowLiteral.Trim();

                if (rowLiteral.Length <= 0)
                {
                    continue;
                }


                if(indexRow == 0)
                {
                    int sectionPosBegin = rowLiteral.IndexOf('[');
                    int sectionPosEnd = rowLiteral.IndexOf(']');

                    if(sectionPosBegin != -1 && sectionPosEnd != -1)
                    {
                        lastSectionName = rowLiteral.Substring(sectionPosBegin + 1, sectionPosEnd - sectionPosBegin - 1);
                        this.AddSection(lastSectionName);
                        continue;
                    }
                }
                else if (rowLiteral.StartsWith('[') && rowLiteral.EndsWith(']'))
                {
                    rowLiteral = rowLiteral.TrimStart('[');
                    rowLiteral = rowLiteral.TrimEnd(']');

                    lastSectionName = rowLiteral;
                    this.AddSection(rowLiteral);
                    continue;
                }

                if (rowLiteral.StartsWith("//"))
                {
                    continue;
                }

                if (rowLiteral.IndexOf('=') == -1)
                {
                    continue;
                }

                string[] pairVector = rowLiteral.Split('=');
                string[] pairKeyValue = new string[2];

                if (pairVector.Length < 2)
                {
                    pairKeyValue[0] = pairVector[0];
                    pairKeyValue[1] = string.Empty;
                }
                else if (pairVector.Length == 2)
                {
                    pairKeyValue[0] = pairVector[0];
                    pairKeyValue[1] = pairVector[1];
                }
                else if (pairVector.Length > 2)
                {
                    pairKeyValue[0] = pairVector[0];
                    pairKeyValue[1] = pairVector[1];

                    for (int index = 2; index < pairVector.Length; index++)
                    {
                        pairKeyValue[1] += "=" + pairVector[index];
                    }
                }

                this.AddPair(lastSectionName, pairKeyValue);
            }
        }

        private void AddSection(string _name)
        {
            string sectionName = _name.ToLower();

            if (this.mapping.ContainsKey(sectionName) == false)
            {
                this.mapping[sectionName] = new();
            }
        }

        private void AddPair(string _sectionName, string[] _pairVector)
        {
            string sectionName = _sectionName.ToLower();
            string key = _pairVector[0].Trim().ToLower();
            string value = _pairVector[1].Trim();

            if (this.mapping.ContainsKey(sectionName) == false)
            {
                this.mapping[sectionName] = new();
            }

            this.mapping[sectionName][key] = value;
        }

        private string GetString(string _sectionName, string _key)
        {
            string section = _sectionName.ToLower();
            string key = _key.ToLower();

            if (this.mapping.ContainsKey(section) == false)
            {
                return string.Empty;
            }

            if (this.mapping[section].ContainsKey(key) == false)
            {
                return string.Empty;
            }

            return this.mapping[section][key];
        }

        private int GetInt(string _sectionName, string _key)
        {
            string value = this.GetString(_sectionName, _key);

            if (value == string.Empty)
            {
                return -1;
            }

            value = Regex.Replace(value, "[^0-9-]", string.Empty);

            return int.Parse(value);
        }

        //private byte[] GetBytes(string _sectionName, string _key)
        //{
        //    string value = this.GetString(_sectionName, _key);

        //    if (value == string.Empty)
        //    {
        //        return new byte[0];
        //    }

        //    return Encoding.ASCII.GetBytes(value);
        //}

        /// <summary>
        /// section => key => value
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, string>> GetMappingData()
        {
            return this.mapping;
        }

        public bool IsEmpty()
        {
            return this.mapping.Count <= 0;
        }

        public bool IsNotEmpty()
        {
            return this.mapping.Count > 0;
        }

        /*  supporting
         *  
         *  string
         *  int
         */

        public Typename Get<Typename>(string _section, string _key)
        {
            System.Type requestType = typeof(Typename);

            if (requestType == typeof(string)) return (Typename)(object)this.GetString(_section, _key);
            if (requestType == typeof(int)) return (Typename)(object)this.GetInt(_section, _key);
            //if(requestType == typeof(byte[])) return (Typename)(object)this.GetBytes(_section, _key);

            return (Typename)(object)null;
        }
    }
}
