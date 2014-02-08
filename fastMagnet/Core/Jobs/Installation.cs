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
using Microsoft.Win32;

namespace fastMagnet.Core.Jobs
{
    class Installation : Job
    {
        //default torrent cache servers.
        private static readonly string[] torrentCacheServices =
        {
            "torrage.com",
            "torcache.net",
            "zoink.it"
        };

        public override bool Perform(params object[] args)
        {
            string defaultHandler, fastMagnetHandler;
            RegUtil userReg, classRootReg;

            //Grab magnet link handler registry key entries.
            userReg = RegUtil.Open(RegRoot.CurrentUser, "Software\\Classes\\Magnet\\shell\\open\\command");
            classRootReg = RegUtil.Open(RegRoot.ClassesRoot, "Magnet\\shell\\open\\command");

            if (userReg == null || classRootReg == null)
            {
                base.ResultMessage = "Magnet için geçerli bir shell handler bulunamadı";
                return false;
            }

            //already installed?
            if (Program.Setting["defaultHandler"] != null)
            {
                //check all of the shell command handlers and re-set fastMagnet path if changed.

                if (userReg.Default.IndexOf(Program.ApplicationExecutable) != -1)
                    userReg.Default = Program.ApplicationExecutable;

                if (classRootReg.Default.IndexOf(Program.ApplicationExecutable) != -1)
                    classRootReg.Default = Program.ApplicationExecutable;

                base.ResultMessage = "Kurulum güncellendi";
                return true;
            }

            //First, we must backup default torrent client handler info.
            defaultHandler = userReg.Default;

            if (string.IsNullOrEmpty(defaultHandler))
                defaultHandler = classRootReg.Default;

            Program.Setting["defaultHandler"] = defaultHandler;

            //Create service list key slot
            if (RegUtil.Create(RegRoot.LocalMachine, "Software\\fastMagnet\\ServiceList", false) == null)
                throw new Exception("Servis listesi registry kaydı oluşturulamadı.");

            //Install default cache services
            foreach (string domain in Installation.torrentCacheServices)
                CacheService.AddNewService(domain);

            //re-set our handler's information to the magnet shell cmd.
            fastMagnetHandler = string.Format("\"{0}\" -perform \"%1\"", Program.ApplicationExecutable);

            //Replace all shell command handlers
            userReg.Default = fastMagnetHandler;
            classRootReg.Default = fastMagnetHandler;

            userReg.Close();
            classRootReg.Close();

            base.ResultMessage = "fastMagnet başarıyla kuruldu";
            return true;
        }

        public override string Name
        {
            get
            {
                return "Kurulum";
            }
        }
    }
}
