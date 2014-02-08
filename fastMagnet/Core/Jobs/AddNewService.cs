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
using fastMagnet.Core.Jobs.Sys;
using fastMagnet.Core.Util;

namespace fastMagnet.Core.Jobs
{
    class AddNewService : Job
    {
        public override bool Perform(params object[] args)
        {
            string srvDomain=args[0].ToString();
            Uri uri;
            bool result;

            try
            {
                uri = new Uri(srvDomain);
                srvDomain = uri.Host;
            }
            catch { }

            if (!RegUtil.Exists(RegRoot.LocalMachine, "Software\\fastMagnet\\ServiceList"))
            {
                base.ResultMessage = "Kurulum olmadan servis eklenemez";
                return false;
            }

            result = CacheService.AddNewService(args[0].ToString());

            if (result)
                base.ResultMessage = "Servis başarıyla eklendi";
            else
                base.ResultMessage = "Servis sisteme eklenemedi";

            return result;
        }

        public override string Name
        {
            get { return "Yeni servis ekleme"; }
        }
    }
}
