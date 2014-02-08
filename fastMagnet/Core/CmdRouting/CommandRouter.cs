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
using System.Collections;
using System.Collections.Generic;

namespace fastMagnet.Core.CmdRouting
{
    delegate void CommandEventHandler(string[] optionVal);

    class CommandRouter
    {
        //Option route list
        private static Dictionary<string,OptionRoute> routes = null;

        static CommandRouter()
        {
            CommandRouter.routes = new Dictionary<string, OptionRoute>();
        }

        private static string PrepareOptionKey(string s)
        {
            return s.Trim(' ', '-').ToUpper();
        }

        public static void Register(string option, CommandEventHandler handler)
        {
            Register(option, handler, false);
        }

        public static void Register(string option, CommandEventHandler handler, bool requiresValue)
        {
            string optionKey = PrepareOptionKey(option);
            OptionRoute route;

            if (CommandRouter.routes.ContainsKey(optionKey))
                throw new Exception("Komut opsiyon key'i zaten mevcut");

            route = new OptionRoute() { Option = option, Handler = handler, RequiresValue = requiresValue };

            CommandRouter.routes.Add(optionKey, route);
        }

        public static void Route(string[] rawCmdLine)
        {
            string opt,optKey;
            string[] values=null;
            OptionRoute route;

            for (int i=0,t=0; i<rawCmdLine.Length; i++)
            {
                //get next command line block
                opt = rawCmdLine[i];

                //check for option flag
                if (opt.StartsWith("-"))
                {
                    optKey = PrepareOptionKey(opt);

                    //route lookup
                    if (!CommandRouter.routes.ContainsKey(optKey))
                        continue;

                    //get route object by option key
                    route = CommandRouter.routes[optKey];

                    //grab and set option values to the handler object if required.
                    if (route.RequiresValue)
                    {
                        int len;

                        for (t = i + 1; ; t++)
                        {
                            if (t == rawCmdLine.Length || rawCmdLine[t].StartsWith("-"))
                            {
                                len = t - (i + 1);

                                if (len > 0)
                                {
                                    values = new string[len];
                                    Array.Copy(rawCmdLine, i + 1, values, 0, len);
                                }

                                break;
                            }

                        }

                        i = t-1;

                        route.OptionValues = values;
                    }

                    //call route handler.
                    route.Handler(route.OptionValues);

                }
            }
        }


    }
}
