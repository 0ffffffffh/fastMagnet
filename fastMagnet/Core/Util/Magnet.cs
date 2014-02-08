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
    enum HashType
    {
        BitTorrent,
        TigerTree,
        Sha1,
        BitPrint,
        Ed2k,
        Aich,
        Kazaa,
        Md5
    }

    class Magnet
    {
        private static string[] BreakString(string s, char breakChr)
        {
            return s.Split(new char[] { breakChr }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static Magnet Parse(string magnetUri)
        {
            Magnet magnet = new Magnet();
            string[] magnetParts, magItem;

            magnet.RawMagnetLink = magnetUri;

            magnetUri = Uri.UnescapeDataString(magnetUri);

            if (!magnetUri.StartsWith("magnet:?"))
                throw new Exception("Geçersiz magnet linki");

            //split magnet parts using ampersand
            magnetParts = BreakString(magnetUri.Replace("magnet:?", ""), '&');

            magnet.Trackers = new List<string>();

            foreach (string part in magnetParts)
            {
                magItem = BreakString(part, '=');

                switch (magItem[0])
                {
                        //client and hash info
                    case "xt":
                        {
                            int hashLoc=2;

                            string[] xtParts = BreakString(magItem[1], ':');

                            if (xtParts[0] == "urn")
                            {
                                //grab magnet client type
                                switch (xtParts[1])
                                {
                                    case "tree":
                                        {
                                            hashLoc++;
                                            magnet.HashType = HashType.TigerTree;
                                        }
                                        break;
                                    case "btih":
                                        magnet.HashType = HashType.BitTorrent;
                                        break;
                                    case "sha1":
                                        magnet.HashType = HashType.Sha1;
                                        break;
                                    case "bitprint":
                                        magnet.HashType = HashType.BitPrint;
                                        break;
                                    case "ed2k":
                                        magnet.HashType = HashType.Ed2k;
                                        break;
                                    case "aich":
                                        magnet.HashType = HashType.Aich;
                                        break;
                                    case "kzhash":
                                        magnet.HashType = HashType.Kazaa;
                                        break;
                                    case "md5":
                                        magnet.HashType = HashType.Md5;
                                        break;
                                }
                            }

                            magnet.Hash = xtParts[hashLoc];

                        }
                        break;
                    case "dn": //display name
                        magnet.Name = magItem[1];
                        break;
                    case "tr": //trackers
                        magnet.Trackers.Add(magItem[1]);
                        break;
                    case "xl":
                    case "as":
                    case "xs":
                    case "kt":
                    case "mt":
                        //Not implemented
                        break;
                }

            }

            if (string.IsNullOrEmpty(magnet.Hash))
                throw new Exception("Geçersiz magnet linki (Hash bilgisi yok)");

            return magnet;
        }

        public string RawMagnetLink
        {
            get;
            private set;
        }

        public List<string> Trackers
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Hash
        {
            get;
            private set;
        }

        public HashType HashType
        {
            get;
            private set;
        }

    }
}
