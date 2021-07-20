using Hardcodet.Wpf.TaskbarNotification;
using mshtml;
using System;

namespace NHIVPNc
{
    internal partial class VPN_Downloader : IDisposable
    {
        private void Goto_next_page()
        {
            log.Info("    Entered Goto_next_page.");
            HTMLDocument d = (HTMLDocument)m.vpnweb.Document;
            // 這頁讀完, 還有下一頁

            // 不可以馬上前往下一頁
            // 如果下一頁, 前往下一頁
            tb.ShowBalloonTip("換頁", "下一頁", BalloonIcon.Info);
            current_page++;
            m.vpnweb.LoadCompleted += Vpn_Page_LoadCompleted;
            log.Info($"    Add delegate Vpn_Page_LoadCompleted.");
            log.Info($"    current_page++, press > key. go to page {current_page}/{total_pages}");
            // 按鈕機制
            foreach (IHTMLElement b in d.getElementById(DOM_FOR_PAGECLICK).all)
            {
                if (b.innerText == ">")
                {
                    b.click();
                }
            }

            //tb.ShowBalloonTip("計時器停止", "完成本頁面所有下載", BalloonIcon.Info);
            log.Info("    Exited Goto_next_page.");
            return;
        }
    }
}
