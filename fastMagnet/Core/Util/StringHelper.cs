/*
Copyright 2014 Oguz Kartal

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace fastMagnet.Core.Util
{
    static class StringHelper
    {
        public static string CalculateMD5(string s)
        {
            StringBuilder sb;

            System.Security.Cryptography.MD5 md5 =
                new System.Security.Cryptography.MD5CryptoServiceProvider();

            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(s));

            sb = new StringBuilder();

            for (int i = 0; i < md5.Hash.Length; i++)
            {
                sb.Append(md5.Hash[i].ToString("x2"));
            }

            return sb.ToString();
        }

        public static string[] SplitWithQuotedBlocks(string s)
        {
            bool quoteOpen=false;
            string temp="";
            List<string> newParts = new List<string>();

            string[] parts = s.Split(new char[] { ' ' },StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                if (!quoteOpen && part.StartsWith("\""))
                {
                    if (part.EndsWith("\""))
                    {
                        newParts.Add(part);
                        continue;
                    }

                    temp = part + " ";
                    quoteOpen = true;
                    continue;
                }
                else if (quoteOpen && part.EndsWith("\""))
                {
                    temp += part;
                    quoteOpen = false;
                    newParts.Add(temp);
                    temp = "";
                    continue;
                }

                if (quoteOpen)
                    temp += part + " ";
                else if (!string.IsNullOrEmpty(part.Trim()))
                    newParts.Add(part);
            }

            parts = newParts.ToArray();
            newParts.Clear();

            return parts;
        }
    }
}
