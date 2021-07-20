using mshtml;
using System.Windows;

namespace NHIVPNc
{
    public partial class MainWindow : Window
    {
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
    }
}
