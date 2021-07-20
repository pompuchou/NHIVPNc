using System.Windows;

namespace NHIVPNc
{
    public partial class MainWindow : Window
    {
        private void VPN_click(object sender, RoutedEventArgs e)
        {
            log.Info("Enter VPN_click due to button pressed.");
            /// 2020/03/28 created, transcribed from vb.net
            vpnweb.Navigate("https://medvpn.nhi.gov.tw/ieae0000/IEAE0200S01.aspx");
            log.Info("Exit VPN_click.");
        }
    }
}
