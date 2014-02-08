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

namespace fastMagnet.Core.Util
{
    class Settings : IDisposable
    {
        private RegUtil regUtil;

        public Settings()
        {
            this.regUtil = RegUtil.Open(RegRoot.LocalMachine, "Software\\fastMagnet");
        }

        public string this[string name]
        {
            get
            {
                object val;

                if (this.regUtil == null)
                    return null;

                val = this.regUtil[name];

                if (val == null)
                    return null;

                return val.ToString();
            }
            set
            {
                if (this.regUtil == null)
                {
                    //Reg key does not exists. Create new one.
                    this.regUtil = RegUtil.Create(RegRoot.LocalMachine, "Software\\fastMagnet",true);

                    if (this.regUtil == null)
                        throw new Exception("Ayar registry kaydı oluşturulamadı");

                }

                this.regUtil[name] = value;
            }
        }

        public void Dispose()
        {
            if (this.regUtil != null)
            {
                this.regUtil.Close();
                this.regUtil = null;
            }
        }
    }
}
