using HtmlAgilityPack;
using mshtml;
using System;
using System.IO;

namespace NHIVPNc
{
    internal partial class VPN_Downloader : IDisposable
    {
        public void Start()
        {
            m.TabControl1.SelectedItem = m.TabPage1;

            #region Read local files

            // everytime when I pushed download key

            Local_files.Clear();

            // read file data from directory
            int i = 1;
            foreach (string f in Directory.GetFiles(LOCAL_FILES_DIRECTORY))
            {
                string f_ = f.Replace($"{LOCAL_FILES_DIRECTORY}\\", "");
                string ff = f_.Substring(0, f_.Length - 4);
                Local_files.Add(ff);
                log.Info($"{i}. {ff} added to Local_files.");
                i++;
            }

            #endregion Read local files

            #region 判斷多頁

            /// 取得gvList
            HTMLDocument d = (HTMLDocument)m.vpnweb.Document;
            IHTMLElement gvDownLoad = d.getElementById(DOM_FOR_ACTUAL_DATA);
            /// is nothing  ==>  == null
            if (gvDownLoad == null) return;
            // 20200329: wpf 要增加reference to Microsoft.mshtml, 已經沒有winform的htmlelement了

            // initialize
            current_page = total_pages = 0;
            /// 找到雲端藥歷有幾頁
            /// 設定total_pages = ????
            HtmlDocument pg = new HtmlDocument();
            if (d.getElementById(DOM_FOR_PAGECLICK) == null)
            {
                /// 沒有ContentPlaceHolder1_pg_gvList, 表示只有ㄧ頁
                total_pages = 1;
                log.Info($"Only one page detected.");
            }
            else
            {
                /// 有ContentPlaceHolder1_pg_gvList, 表示有多頁
                /// 舊方法 pg_N = pg.Children.Count - 5;
                /// 如果多頁, 轉換loadcomplete, 呼叫pager by click
                // 20200502: outerHTML的XPATH="//selection/option", innerHTML的XPATH="//option"
                pg.LoadHtml(d.getElementById(DOM_FOR_PAGENUMBERS).innerHTML);
                HtmlNodeCollection o = pg.DocumentNode.SelectNodes("//option");
                total_pages = o.Count;
                log.Info($"{total_pages} pages detected.");
            }

            #endregion 判斷多頁

            /// 前往讀取第一頁
            current_page = 1;
            Vpn_PageData();
        }
    }
}
