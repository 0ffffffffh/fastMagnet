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
using Microsoft.Win32;

namespace fastMagnet.Core.Util
{
    enum RegRoot
    {
        LocalMachine,
        CurrentUser,
        ClassesRoot
    }

    class RegUtil
    {
        private RegistryKey key=null;

        private static RegistryKey GetKey(RegRoot root)
        {
            switch (root)
            {
                case RegRoot.ClassesRoot:
                    return Registry.ClassesRoot;
                case RegRoot.CurrentUser:
                    return Registry.CurrentUser;
                case RegRoot.LocalMachine:
                    return Registry.LocalMachine;
            }

            throw new Exception("Invalid root identifier");
        }

        public static void Delete(RegRoot root, string subKey)
        {
            GetKey(root).DeleteSubKeyTree(subKey);
        }

        public static bool Exists(RegRoot root, string subKey)
        {
            RegistryKey regKey = GetKey(root).OpenSubKey(subKey);

            if (regKey != null)
            {
                regKey.Close();
                return true;
            }

            return false;
        }

        public static string[] EnumerateSubKeys(RegRoot root, string subKey)
        {
            try
            {
                return Open(root, subKey, false).key.GetSubKeyNames();
            }
            catch { }

            return new string[0];
        }

        public static RegUtil Create(RegRoot root, string subKey, bool stayOpen)
        {
            RegistryKey regKey;
            RegUtil regUtil;

            try
            {
                regKey = GetKey(root).CreateSubKey(subKey, RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            catch
            {
                return null;
            }

            if (regKey != null)
            {
                regUtil = new RegUtil();
                regUtil.key = regKey;

                if (!stayOpen)
                    regUtil.Close();

                return regUtil;
            }

            return null;
        }

        public static RegUtil Open(RegRoot root, string subKey, bool writable=true, bool createIfNotExists=false)
        {
            RegUtil util;

            if (!Exists(root, subKey))
            {
                if (!createIfNotExists)
                    return null;

                return Create(root, subKey,true);
            }

            util = new RegUtil();
            util.key = GetKey(root).OpenSubKey(subKey, writable);

            return util;
        }

        public object this[string key]
        {
            get
            {
                return this.key.GetValue(key, null);
            }
            set
            {
                this.key.SetValue(key, value, RegistryValueKind.Unknown);
            }
        }

        public string Default
        {
            get
            {
                object val = this[null];
                
                if (val == null)
                    return null;

                return val.ToString();
            }
            set
            {
                this[null] = value;
            }
        }

        public void Close()
        {
            this.key.Close();
            this.key = null;
        }

    }
}
