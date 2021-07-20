using Hardcodet.Wpf.TaskbarNotification;
using HtmlAgilityPack;
using NHIVPNc.Models;
using System;
using System.Collections.Generic;

namespace NHIVPNc
{
    /// <summary>
    /// 20200515 created
    /// 20210720 revisited
    /// </summary>
    internal partial class VPN_Downloader : IDisposable
    {
        #region "Declaration"

        private bool disposed = false;
        private static readonly log4net.ILog log = LogHelper.GetLogger();
        private System.Timers.Timer _timer1;
        private readonly TaskbarIcon tb = new TaskbarIcon();

        private int current_page, total_pages, current_line;

        // local variables
        private readonly Dictionary<int, string> VPN_files = new Dictionary<int, string>();

        private readonly List<string> Local_files = new List<string>();
        private readonly Queue<int> queue_files = new Queue<int>();

        private string DOM_FOR_PAGENUMBERS { get; set; }
        private string DOM_FOR_PAGECLICK { get; set; }
        private string DOM_FOR_ACTUAL_DATA { get; set; }
        private readonly string LOCAL_FILES_DIRECTORY = @"D:\IDrive-Sync\archive";
        private readonly MainWindow m;

        public delegate tbl_download TD_Parcer(HtmlNodeCollection tds);

        // 20200515 created, because after download, we can not go to next page directly, so we have to figure out another method
        // 20200522 defunct; and then I found out it's still a must have idea
        private bool GOTO_NEXT_PAGE = false;

        public TD_Parcer _td_parcer;

        #endregion "Declaration"

        public VPN_Downloader(MainWindow MW, DownloadType t)
        {
            m = MW;
            switch (t)
            {
                case DownloadType.ClinicDownloader:
                    DOM_FOR_ACTUAL_DATA = "ContentPlaceHolder1_gvDownLoad";
                    DOM_FOR_PAGECLICK = "ContentPlaceHolder1_pgDownLoad";
                    DOM_FOR_PAGENUMBERS = "ctl00$ContentPlaceHolder1$pgDownLoad_input";
                    _td_parcer += Clinic_TD_Parcer;
                    break;

                case DownloadType.SpecialDownloader:
                    DOM_FOR_ACTUAL_DATA = "cph_rptDownload";
                    DOM_FOR_PAGECLICK = "cph_pgDownLoad";
                    DOM_FOR_PAGENUMBERS = "ctl00$cph$pgDownLoad_input";
                    _td_parcer += Special_TD_Parcer;
                    break;

                default:
                    Dispose();
                    break;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // Free any other managed objects here.
                //
                _timer1.Stop();
                _timer1.Dispose();
                log.Info("timer1 for pressing S stopped.");
            }

            disposed = true;
        }
    }
}