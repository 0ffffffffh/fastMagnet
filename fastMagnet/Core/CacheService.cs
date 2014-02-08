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
using fastMagnet.Core.Util;

namespace fastMagnet.Core
{
    class CacheService
    {
        private static List<CacheService> _services;

        private int foundCount, notFoundCount, timeoutCount, notConnectedCount;

        public static List<CacheService> Services
        {
            get
            {
                RegUtil regKey;
                string[] serviceDomains = RegUtil.EnumerateSubKeys(RegRoot.LocalMachine, "Software\\fastMagnet\\ServiceList");

                if (CacheService._services == null)
                {
                    CacheService._services = new List<CacheService>();

                    //Load all cache services
                    foreach (string domain in serviceDomains)
                    {
                        regKey = RegUtil.Open(RegRoot.LocalMachine, string.Format("Software\\fastMagnet\\ServiceList\\{0}", domain));

                        if (regKey == null)
                            continue;

                        CacheService._services.Add(
                            new CacheService(domain, (int)regKey["foundCount"], (int)regKey["notFoundCount"],
                                            (int)regKey["timeoutCount"], (int)regKey["notConnectedCount"]));

                        regKey.Close();

                    }

                    //Sort by ideal rate.
                    CacheService._services.Sort(new Comparison<CacheService>(delegate(CacheService cs1, CacheService cs2)
                    {
                        return cs2.IdealServiceRate.CompareTo(cs1.IdealServiceRate);
                    }));

                }

                return CacheService._services;
            }
        }

        public static bool AddNewService(string domain)
        {
            RegUtil servKey = RegUtil.Create(RegRoot.LocalMachine, 
                string.Format("Software\\fastMagnet\\ServiceList\\{0}",domain), true);

            if (servKey == null)
                return false;

            servKey["foundCount"] = 0;
            servKey["notFoundCount"] = 0;
            servKey["timeoutCount"] = 0;
            servKey["notConnectedCount"] = 0;

            servKey.Close();

            return true;
        }

        public CacheService(string domain, int foundCount,int notFoundCount,int timeoutCount,int notConnectedCount)
        {
            this.foundCount = foundCount;
            this.notFoundCount = notFoundCount;
            this.timeoutCount = timeoutCount;
            this.notConnectedCount = notConnectedCount;

            this.ServiceDomain = domain;

            Calculate();
        }

        //Simply calculate ideal service rate
        private void Calculate()
        {
            double serviceFactor, netFoundSuccessRate, serviceWorkingRate;
            int total = this.foundCount + this.notFoundCount + this.timeoutCount + this.notConnectedCount;

            if (total > 0)
            {
                this.FoundRate = PercentOfValue(foundCount, total);
                this.NotFoundRate = PercentOfValue(notFoundCount, total);
                this.TimeoutRate = PercentOfValue(timeoutCount, total);
                this.NotConnectedRate = PercentOfValue(notConnectedCount, total);

                netFoundSuccessRate = PercentOfValue(foundCount, foundCount + notFoundCount);

                serviceFactor = this.NotFoundRate == 0.0 ? this.FoundRate/10 : this.FoundRate / this.NotFoundRate;

                if (serviceFactor == 0.0)
                    serviceFactor = 1.0;

                serviceWorkingRate = 100.0 - (this.TimeoutRate + this.NotConnectedRate);

                this.IdealServiceRate = serviceWorkingRate + netFoundSuccessRate + serviceFactor;

                this.IdealServiceRate -= PercentOfValue(timeoutCount + notConnectedCount, total);
            }
        }

        private double PercentOfValue(double val, double total)
        {
            double r = (val / total) * 100;

            //we dont need nan value.
            if (double.IsNaN(r))
                return 0.0;

            return r;
        }

        public string ServiceDomain
        {
            get;
            private set;
        }

        public int FoundCount
        {
            get
            {
                return this.foundCount;
            }
            set
            {
                this.foundCount = value;
                Calculate();
            }
        }

        public int NotFoundCount
        {
            get
            {
                return this.notFoundCount;
            }
            set
            {
                this.notFoundCount = value;
                Calculate();
            }
        }

        public int TimeoutCount
        {
            get
            {
                return this.timeoutCount;
            }
            set
            {
                this.timeoutCount = value;
                Calculate();
            }
        }

        public int NotConnectedCount
        {
            get
            {
                return this.notConnectedCount;
            }
            set
            {
                this.notConnectedCount = value;
                Calculate();
            }
        }

        public double FoundRate
        {
            get;
            private set;
        }

        public double NotFoundRate
        {
            get;
            private set;
        }

        public double TimeoutRate
        {
            get;
            private set;
        }

        public double NotConnectedRate
        {
            get;
            private set;
        }

        public double IdealServiceRate
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return "Ideal Service Rate: " + this.IdealServiceRate.ToString();
        }

        public void Save()
        {
            RegUtil servKey = RegUtil.Open(RegRoot.LocalMachine, string.Format("Software\\fastMagnet\\ServiceList\\{0}", this.ServiceDomain), true);

            servKey["foundCount"] = this.foundCount;
            servKey["notFoundCount"] = this.notFoundCount;
            servKey["timeoutCount"] = this.timeoutCount;
            servKey["notConnectedCount"] = this.notConnectedCount;

            servKey.Close();
        }

        public string MakeUrl(Magnet magnet)
        {
            return string.Format("http://{0}/torrent/{1}.torrent", this.ServiceDomain, magnet.Hash.ToUpper());
        }
    }
}
