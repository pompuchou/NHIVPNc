using System;
using System.Windows.Navigation;

namespace NHIVPNc
{
    internal partial class VPN_Downloader : IDisposable
    {
        private void Vpn_Page_LoadCompleted(object sender, NavigationEventArgs e)
        {
            log.Info("    Delete delegate Vpn_Page_LoadCompleted.");
            m.vpnweb.LoadCompleted -= Vpn_Page_LoadCompleted;
            Vpn_PageData();
        }
    }
}
