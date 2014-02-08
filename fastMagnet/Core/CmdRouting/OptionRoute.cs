﻿/*
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

namespace fastMagnet.Core.CmdRouting
{
    internal class OptionRoute
    {
        public string Option
        {
            get;
            internal set;
        }

        public string[] OptionValues
        {
            get;
            internal set;
        }

        public bool RequiresValue
        {
            get;
            internal set;
        }

        public CommandEventHandler Handler
        {
            get;
            internal set;
        }
    }
}