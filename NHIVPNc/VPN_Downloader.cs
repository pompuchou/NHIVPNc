using Hardcodet.Wpf.TaskbarNotification;
using HtmlAgilityPack;
using mshtml;
using NHIVPNc.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Navigation;
using WindowsInput;
using WindowsInput.Native;

namespace NHIVPNc
{
    internal class VPN_Downloader : IDisposable
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
                string ff = f.Replace($"{LOCAL_FILES_DIRECTORY}\\", "").Replace(".zip", "");
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

        private tbl_download Clinic_TD_Parcer(HtmlNodeCollection tds)
        {
            short order_n = 0;
            tbl_download newNHI = new tbl_download();
            foreach (HtmlNode td in tds)
            {
                switch (order_n)
                {
                    case 0:  //檔案名稱
                        newNHI.f_name = td.InnerText;
                        break;

                    case 1: //檔案說明
                        newNHI.f_remark = td.InnerText;
                        break;

                    case 2: //下載備註
                        newNHI.remark = td.InnerText;
                        break;

                    case 3: //提供下載日期
                        if (td.InnerText != string.Empty)
                        {
                            HtmlNode div = td.ChildNodes[1]; // 怪,第一個member竟然是/r/n
                            string[] temp_s = div.InnerHtml.Replace("<br>", "|").Split('|');
                            string[] temp_d = temp_s[0].Split('/');
                            newNHI.SDATE = DateTime.Parse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]} {temp_s[1]}");
                        }
                        break;

                    case 4: //檔案下載
                        switch (td.SelectNodes("//a").Count)
                        {
                            case 2:
                                newNHI.download = true;
                                break;

                            case 1:
                                newNHI.download = false;
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
                order_n++;
            }
            if ((bool)newNHI.download)
            {
                VPN_files.Add(current_line, newNHI.f_name);
                log.Info($"    {current_line}, {newNHI.f_name} added to VPN_files.");
            }
            return newNHI;
        }

        private tbl_download Special_TD_Parcer(HtmlNodeCollection tds)
        {
            short order_n = 0;
            tbl_download newNHI = new tbl_download();
            foreach (HtmlNode td in tds)
            {
                switch (order_n)
                {
                    case 1:  //檔案名稱
                        newNHI.f_name = td.InnerText;
                        break;

                    case 2: //檔案說明
                        newNHI.f_remark = td.InnerText;
                        break;

                    case 3: //提供下載日期
                        if (td.InnerText != string.Empty)
                        {
                            HtmlNode div = td.ChildNodes[1]; // 怪,第一個member竟然是/r/n
                            string[] temp_s = div.InnerHtml.Replace("<br>", "|").Split('|');
                            string[] temp_d = temp_s[0].Split('/');
                            newNHI.SDATE = DateTime.Parse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]} {temp_s[1]}");
                        }
                        break;

                    case 4: //檔案下載
                        switch (td.SelectNodes("//a").Count)
                        {
                            case 2:
                                newNHI.download = true;
                                break;

                            case 1:
                                newNHI.download = false;
                                break;

                            default:
                                break;
                        }
                        break;

                    default:
                        break;
                }
                order_n++;
            }
            if ((bool)newNHI.download)
            {
                VPN_files.Add(current_line, newNHI.f_name);
                log.Info($"    {current_line}, {newNHI.f_name} added to VPN_files.");
            }
            return newNHI;
        }

        private void Vpn_PageData()
        {
            log.Info($"Entered Vpn_PageData.");
            log.Info($"  Currently on page {current_page}/{total_pages}.");

            #region write in sql table

            /// 取得gvList
            HTMLDocument d = (HTMLDocument)m.vpnweb.Document;
            IHTMLElement gvDownLoad = d.getElementById(DOM_FOR_ACTUAL_DATA);

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

            if (queue_files.Count == 0)
            {
                log.Info($"    Nothing enqueued on page {current_page}/{total_pages}");
            }
            else
            {
                // 有東西才需要執行

                // execution
                this._timer1 = new System.Timers.Timer
                {
                    Interval = 6000
                };
                this._timer1.Elapsed += new System.Timers.ElapsedEventHandler(TimersTimer_Elapsed);

                tb.ShowBalloonTip("計時器開始", $"一共{queue_files.Count}個檔案要下載", BalloonIcon.Info);
                // initialization
                current_line = 0;
                current_line = queue_files.Dequeue();

                log.Info($"    _timer1 started.");
                this._timer1.Start();
            }

            #endregion download files

            log.Info("Exited Vpn_PageData.");
        }

        private void TimersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            log.Info($"  Entered TimersTimer_Elapsed.");
            m.Dispatcher.Invoke((Action)(() =>
            {
                log.Info($"    Now dealing with page {current_page}/{total_pages}.");
                log.Info($"    Now dealing with line {current_line}.");
                HTMLDocument d = (HTMLDocument)m.vpnweb.Document;
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
                    this._timer1.Stop();
                    m.Refresh_Table();
                    return;
                }
                else if (queue_files.Count == 0)
                {
                    log.Info($"    _timer1 stopped. this page finished.");
                    // 這頁讀完, 還有下一頁
                    this._timer1.Stop();

                    // 如果下一頁, 前往下一頁
                    current_page++;
                    m.vpnweb.LoadCompleted += Vpn_Page_LoadCompleted;
                    log.Info($"    Add delegate Vpn_Page_LoadCompleted.");
                    log.Info($"    current_page++, press > key. page {current_page}/{total_pages}");
                    // 按鈕機制
                    foreach (IHTMLElement b in d.getElementById(DOM_FOR_PAGECLICK).all)
                    {
                        if (b.innerText == ">")
                        {
                            b.click();
                        }
                    }
                    return;
                }
                else
                {
                    current_line = queue_files.Dequeue();
                    log.Info($"    go to next line: {current_line}.");
                }
            }));
            log.Info($"  Exited TimersTimer_Elapsed.");
        }

        private void Vpn_Page_LoadCompleted(object sender, NavigationEventArgs e)
        {
            log.Info("    Delete delegate Vpn_Page_LoadCompleted.");
            m.vpnweb.LoadCompleted -= Vpn_Page_LoadCompleted;
            Vpn_PageData();
        }

        private void Work_todo()
        {
            log.Info("Enter Work_todo.");
            InputSimulator sim = new InputSimulator();
            log.Info("Press s");
            System.Threading.Thread.Sleep(4000);
            sim.Keyboard.KeyPress(VirtualKeyCode.VK_S);
            log.Info("  Press Enter.");
            System.Threading.Thread.Sleep(1000);
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            log.Info("Exit Work_todo.");
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

    public enum DownloadType
    {
        ClinicDownloader = 0,
        SpecialDownloader = 1
    }
}