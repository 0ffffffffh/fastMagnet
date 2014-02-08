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
using fastMagnet.Core.Jobs.Sys;
using fastMagnet.Core.Util;

namespace fastMagnet.Core.Jobs
{
    class Uninstallation : Job
    {
        public override bool Perform(params object[] args)
        {
            string defaultHandler;
            RegUtil userReg, classRootReg;

            //Grab magnet link handler registry key entries.
            userReg = RegUtil.Open(RegRoot.CurrentUser, "Software\\Classes\\Magnet\\shell\\open\\command");
            classRootReg = RegUtil.Open(RegRoot.ClassesRoot, "Magnet\\shell\\open\\command");

            if (userReg == null || classRootReg == null)
            {
                base.ResultMessage = "Magnet için geçerli bir shell handler bulunamadı";
                return false;
            }

            defaultHandler = Program.Setting["defaultHandler"];

            if (defaultHandler == null)
            {
                base.ResultMessage = "fastMagnet sisteme kurulu değil";
                return false;
            }

            //restore default torrent client handler
            userReg.Default = defaultHandler;
            classRootReg.Default = defaultHandler;

            userReg.Close();
            classRootReg.Close();

            //cleanup
            RegUtil.Delete(RegRoot.LocalMachine, "Software\\fastMagnet");

            base.ResultMessage = "fastMagnet başarıyla kaldırıldı";
            return true;
        }

        public override string Name
        {
            get
            {
                return "Kaldırma";
            }
        }
    }
}
