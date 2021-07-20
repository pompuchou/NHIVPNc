using Hardcodet.Wpf.TaskbarNotification;
using mshtml;
using System;
using System.Timers;

namespace NHIVPNc
{
    internal partial class VPN_Downloader : IDisposable
    {
        private void TimersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            log.Info($"  Entered TimersTimer_Elapsed.");

            m.Dispatcher.Invoke((Action)(() =>
            {
                HTMLDocument d = (HTMLDocument)m.vpnweb.Document;

                // dispatcher的問題, 要叫用就要在這裡面
                if (GOTO_NEXT_PAGE)
                {
                    GOTO_NEXT_PAGE = false;

                    log.Info($"    _timer1 stopped. this pages finished.");
                    this._timer1.Stop();

                    Goto_next_page();

                    log.Info($"  Exited TimersTimer_Elapsed.");
                    return;
                }

                log.Info($"    Now dealing with page {current_page}/{total_pages}.");
                log.Info($"    Now dealing with line {current_line}.");
                IHTMLElement gvDownLoad = d.getElementById(DOM_FOR_ACTUAL_DATA);
                IHTMLElementCollection trs_ = gvDownLoad.all;
                IHTMLElementCollection trs = trs_.tags("tr");
                IHTMLElement tr = trs.item(current_line, null);
                IHTMLElement a = tr.children[4].children[0];
                a.click();
                log.Info($"    button click: {a.innerHTML}");

                System.Threading.ThreadStart th_begin = new System.Threading.ThreadStart(Work_todo);
                System.Threading.Thread thr = new System.Threading.Thread(th_begin)
                {
                    IsBackground = true,
                    Name = "PressS"
                };
                thr.Start();

                // 判斷是否這一頁讀完了? 是否最後一頁了?
                if ((queue_files.Count == 0) && (current_page == total_pages))
                {
                    // 這頁讀完, 且所有頁都讀完了.
                    log.Info($"    _timer1 stopped. all pages finished.");
                    m.Refresh_Table();
                    tb.ShowBalloonTip("結束", "完成所有頁面讀取", BalloonIcon.Info);
                    this._timer1.Stop();
                }
                else if (queue_files.Count == 0)
                {
                    // 這頁讀完, 但還有下一頁.
                    GOTO_NEXT_PAGE = true;
                }
                else
                {
                    current_line = queue_files.Dequeue();
                    log.Info($"    go to next line: {current_line}.");
                }
            }));

            log.Info($"  Exited TimersTimer_Elapsed.");
            return;
        }
    }
}
