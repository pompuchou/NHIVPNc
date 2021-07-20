using Hardcodet.Wpf.TaskbarNotification;
using HtmlAgilityPack;
using mshtml;
using NHIVPNc.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHIVPNc
{
    internal partial class VPN_Downloader : IDisposable
    {
        private void Vpn_PageData()
        {
            log.Info($"Entered Vpn_PageData.");
            log.Info($"  Currently on page {current_page}/{total_pages}.");

            #region write in sql table

            /// 取得gvList
            HTMLDocument d = (HTMLDocument)m.vpnweb.Document;
            IHTMLElement gvDownLoad = d.getElementById(DOM_FOR_ACTUAL_DATA);
            log.Info($"[{DOM_FOR_ACTUAL_DATA}] is read.");

            /// 讀取
            /// 20200503 我發現Html Agility Pack不能click
            /// 只好第一層是ihtmlelement
            IHTMLElementCollection trs_ = gvDownLoad.all;
            IHTMLElementCollection trs = trs_.tags("tr");
            DateTime current_time = DateTime.Now;

            // 使用item這個方法可以將集合中的元素取出
            // 第一個參數代表的是順序,但是在msdn中標示為name
            // 第二個參數msdn中標示為index,但經過測試後,指的並不是順序,所以目前無法確定他的用途
            // 如果有知道的朋友,也請跟我說一下

            // current_line = 0 是標題行
            /// 20200503 我發現Html Agility Pack不能click
            /// 只好第一層是ihtmlelement

            VPN_files.Clear();
            for (current_line = 1; current_line < trs.length; current_line++)
            {
                IHTMLElement tr = trs.item(current_line, null);
                HtmlDocument h_ = new HtmlDocument();
                h_.LoadHtml(tr.innerHTML);
                HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

                tbl_download result = _td_parcer(tds);

                using (NHIDataContext dc = new NHIDataContext())
                {
                    var q = from p in dc.tbl_download
                            where (p.f_name == result.f_name) && (p.SDATE == result.SDATE)
                            select p;
                    if (q.Count() == 0)
                    {
                        result.QDATE = current_time;
                        dc.tbl_download.InsertOnSubmit(result);
                        dc.SubmitChanges();
                        log.Info($"    [{result.f_name}] added to SQL server");
                    }
                }
            }

            #endregion write in sql table

            #region download files

            // making queue_files
            foreach (KeyValuePair<int, string> v in VPN_files)
            {
                if (!Local_files.Contains(v.Value))
                {
                    queue_files.Enqueue(v.Key);
                    log.Info($"    {v.Key}: {v.Value} enqueued.");
                }
            }

            if ((queue_files.Count == 0) && (current_page == total_pages))
            {
                // 這頁讀完, 且所有頁都讀完了.
                m.Refresh_Table();
                tb.ShowBalloonTip("結束", "完成所有頁面讀取", BalloonIcon.Info);
            }
            else if (queue_files.Count == 0)
            {
                log.Info($"    Nothing enqueued on page {current_page}/{total_pages}");
                Goto_next_page();
            }
            else
            {
                // 有東西才需要執行

                tb.ShowBalloonTip("計時器開始", $"一共{queue_files.Count}個檔案要下載", BalloonIcon.Info);
                // initialization
                current_line = 0;
                current_line = queue_files.Dequeue();

                // execution
                this._timer1 = new System.Timers.Timer
                {
                    Interval = 6000
                };
                this._timer1.Elapsed += new System.Timers.ElapsedEventHandler(TimersTimer_Elapsed);

                log.Info($"    _timer1 started.");
                this._timer1.Start();
            }

            #endregion download files

            log.Info("Exited Vpn_PageData.");
        }
    }
}
