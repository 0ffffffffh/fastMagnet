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
using System.Windows.Forms;
using fastMagnet.Core.CmdRouting;
using fastMagnet.Core.Jobs.Sys;

namespace fastMagnet
{
    static class Program
    {
        static Program()
        {
            Program.Setting = new Core.Util.Settings();
        }

        public static fastMagnet.Core.Util.Settings Setting
        {
            get;
            private set;
        }

        //Generic job dispatcher routine
        static object DispatchJob(JobType type, params object[] args)
        {
            bool result;

            Job job = JobLoader.GetJob(type);

            result = job.Perform(args);

            if (!string.IsNullOrEmpty(job.ResultMessage))
                Program.LoadForm(new MessageForm(job.ResultMessage, job.Name, result ? MessageForm.MessageType.Info : MessageForm.MessageType.Error));
            
            return job.Result;
        }

        static void RegisterCommandHandlers()
        {
            CommandRouter.Register("-inst", delegate(string[] val)
            {
                DispatchJob(JobType.Install);
            });

            CommandRouter.Register("-uninst", delegate(string[] val)
            {
                DispatchJob(JobType.Uninstall);
            });

            CommandRouter.Register("-perform", delegate(string[] val)
            {
                DispatchJob(JobType.MagnetPerform, val[0]);
            }, true );

            CommandRouter.Register("-addsvc", delegate(string[] val)
            {
                DispatchJob(JobType.AddService, val[0]);
            }, true);
        }

        public static string ApplicationExecutable
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().Location;
            }
        }

        public static void LoadForm(Form form)
        {
            Application.Run(form);
        }

        /// <summary>
        /// Here we go!
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                LoadForm(new MessageForm("Geçerli bir işlem opsiyonu yok.", "Hata", MessageForm.MessageType.Error));
                Environment.Exit(1);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

            RegisterCommandHandlers();

            try
            {
                CommandRouter.Route(args);
            }
            catch (Exception e)
            {
                LoadForm(new MessageForm(e.Message, "Hata", MessageForm.MessageType.Error));
            }
        }
    }
}
