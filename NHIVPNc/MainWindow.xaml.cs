using mshtml;
using NHIVPNc.Models;
using System.Deployment.Application;
using System.Linq;
using System.Windows;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace NHIVPNc
{
    /// <summary>
    /// 2020/03/22 created
    /// 這是我第一個cs, WPF的程式
    /// 2020/03/28 什麼是相對於winform的webbrowser
    /// menustrip=>menu;
    /// 2020/05/03 revisited
    /// 似乎不適合MVVM, 太多
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Declaration

        ///20200322 created
        private static readonly log4net.ILog log = LogHelper.GetLogger();
        private VPN_Downloader v;

        #endregion Declaration

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string version;
            try
            {
                //// get deployment version
                version = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch (InvalidDeploymentException)
            {
                //// you cannot read publish version when app isn't installed 
                //// (e.g. during debug)
                version = "debugging, not installed";
            }
            this.Title += $" v.{version}";
            log.Info($"Log in. Version: {version}");

            Refresh_Table();
        }

        #region Buttons

        private void VPN_click(object sender, RoutedEventArgs e)
        {
            log.Info("Enter VPN_click due to button pressed.");
            /// 2020/03/28 created, transcribed from vb.net
            vpnweb.Navigate("https://medvpn.nhi.gov.tw/ieae0000/IEAE0200S01.aspx");
            log.Info("Exit VPN_click.");
        }

        private void DL_click(object sender, RoutedEventArgs e)
        {
            /// 20200329 transcribed from vb.net 20191020 created
            /// 存儲頁面
            log.Info("Enter DL_click due to download key pressed.");
            // configuration for 院所下載

            // 判斷所在的頁面
            HTMLDocument d = (HTMLDocument)vpnweb.Document;
            if (d?.getElementById("ContentPlaceHolder1_gvDownLoad") != null)
            {
                log.Info("  Clinic Download page found.");
                v = new VPN_Downloader(this, DownloadType.ClinicDownloader);
            }
            else if (d?.getElementById("cph_rptDownload") != null)
            {
                log.Info("  Special Download page found.");
                v = new VPN_Downloader(this, DownloadType.SpecialDownloader);
            }
            else
            {
                log.Info("  No download page found.");
                log.Info("Exit DL_click.");
                return;
            }
                /// 前往讀取第一頁
                v.Start();
            log.Info("Exit DL_click.");
        }

        #endregion Buttons

        public void Refresh_Table()
        {
            log.Info("Enter Refresh_Table.");
            using (NHIDataContext dc = new NHIDataContext())
            {
                DLData.ItemsSource = from p in dc.tbl_download
                                     orderby p.SDATE descending
                                     select p;
            }
            log.Info("Exit Refresh_Table.");
        }
    }
}