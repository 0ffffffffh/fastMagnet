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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using fastMagnet.Core.Jobs.Sys;
using fastMagnet.Core.Util;

namespace fastMagnet.Core.Jobs
{
    class MagnetJob : Job
    {
        private byte[] ReadBytesFromStream(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];
            int readLen;
            MemoryStream ms = new MemoryStream();

            if (!stream.CanRead)
                return null;

            while ((readLen = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, readLen);
            }

            return ms.ToArray();
        }

        //Make request and download torrent data
        private byte[] DownloadTorrentData(string url, out WebExceptionStatus exStatus)
        {
            byte[] content=null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            exStatus = WebExceptionStatus.Success;

            request.UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64; rv:26.0) Gecko/20100101 Firefox/26.0";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            request.Timeout = 5000;
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            try
            {
                content = ReadBytesFromStream(request.GetResponse().GetResponseStream());
            }
            catch (WebException we) 
            {
                exStatus = we.Status;
            }

            
            return content;
        }

        private void PassToTorrentClient(string arg)
        {
            string torrentClient;
            string[] cmdParts;
            
            //get default torrent client, and execute with argument

            torrentClient = Program.Setting["defaultHandler"];

            if (torrentClient == null)
                throw new Exception("Varsayılan bir magnet handler kurulum ayarlarında mevcut değil"); 

            cmdParts = StringHelper.SplitWithQuotedBlocks(torrentClient);

            ProcessStartInfo psi = new ProcessStartInfo();

            psi.UseShellExecute = true;
            psi.FileName = cmdParts[0];
            psi.Arguments = string.Join(" ", cmdParts, 1, cmdParts.Length - 1).Replace("%1", arg);

            Process.Start(psi);
        }

        public override bool Perform(params object[] args)
        {
            bool cacheFound = false;
            byte[] data;
            string url=null,torPath=null;
            WebExceptionStatus wes;
            Magnet magnet;
            
            if (args == null && args.Length < 1)
                return false;
            
            try
            {
                magnet = Magnet.Parse(args[0].ToString());
            }
            catch (Exception e)
            {
                base.ResultMessage = e.Message;
                return false;
            }

            torPath = string.Format("{0}{1}.torrent",Path.GetTempPath(),magnet.Hash.ToUpper());

            //already fetched?
            if (File.Exists(torPath))
            {
                //ok. still out there. Pass it on.
                PassToTorrentClient(torPath);
                return true;
            }

            foreach (CacheService cacheServ in CacheService.Services)
            {
                url = cacheServ.MakeUrl(magnet);

                //Try to fetch torrent from online cache
                data = DownloadTorrentData(url, out wes);

                if (data == null)
                {
                    switch (wes)
                    {
                            //check problem type and set counters for statistics
                        case WebExceptionStatus.Timeout:
                            cacheServ.TimeoutCount++;
                            break;
                        case WebExceptionStatus.ConnectFailure:
                        case WebExceptionStatus.ConnectionClosed:
                        case WebExceptionStatus.UnknownError:
                            cacheServ.NotConnectedCount++;
                            break;
                    }

                    cacheServ.Save();

                    continue;
                }

                //check for minimum valid data length
                if (data.Length < 11)
                {
                    cacheServ.NotFoundCount++;
                    cacheServ.Save();
                    continue;
                }

                //check for valid torrent data
                if (!Encoding.ASCII.GetString(data, 0, 11).
                    Equals("d8:announce"))
                {
                    cacheServ.NotFoundCount++;
                    cacheServ.Save();
                    continue;
                }

                cacheServ.FoundCount++;
                cacheServ.Save();

                //Ok. its a torrent content. Write it.
                File.WriteAllBytes(torPath,data);

                if (!File.Exists(torPath))
                    throw new Exception("Torrent dosya içeriği diske yazılamadı");



                cacheFound = true;
                break;
            }

            base.Result = magnet;

            //Cache missed. Bad luck. Now we must pass directly magnet link to the torrent client
            if (!cacheFound)
            {
                PassToTorrentClient(magnet.RawMagnetLink);
                base.ResultMessage = string.Format("{0} hash id'li magnet önbellekte bulunamadı. Magnet doğrudan açılacak", magnet.Hash);
                return false;
            }

            //Found. Pass fetched torrent file to the client.
            PassToTorrentClient(torPath);
            
            return true;
        }

        public override string Name
        {
            get
            {
                return "Magnet link yükleyici";
            }
        }
    }
}
