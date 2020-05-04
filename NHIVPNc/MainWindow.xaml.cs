using HtmlAgilityPack;
using mshtml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Navigation;
using WindowsInput;
using WindowsInput.Native;

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
        #region "Declaration"

        ///20200322 created
        private static readonly log4net.ILog log = LogHelper.GetLogger();

        private System.Timers.Timer _timer1;

        private int current_page, total_pages, current_line, total_lines;
        private bool should_click = false;
        private readonly Queue<int> queue_item = new Queue<int>();

        #endregion "Declaration"

        public MainWindow()
        {
            InitializeComponent();
            log.Info("Log in successfully.");
        }

        #region "Buttons"

        private void VPN_click(object sender, RoutedEventArgs e)
        {
            /// 2020/03/28 created, transcribed from vb.net
            vpnweb.Navigate("https://medvpn.nhi.gov.tw/ieae0000/IEAE0200S01.aspx");
        }

        private void DL_click(object sender, RoutedEventArgs e)
        {
            /// 20200329 transcribed from vb.net 20191020 created
            /// 存儲頁面

            this.TabControl1.SelectedItem = this.TabPage1;
            /// 取得gvList
            HTMLDocument d = (HTMLDocument)vpnweb.Document;
            IHTMLElement gvDownLoad = d.getElementById("ContentPlaceHolder1_gvDownLoad");
            /// is nothing  ==>  == null
            if (gvDownLoad == null) return;

            // 20200329: wpf 要增加reference to Microsoft.mshtml, 已經沒有winform的htmlelement了

            #region 判斷多頁

            // initialize
            current_page = total_pages = 0;
            /// 找到雲端藥歷有幾頁
            /// 設定total_pages = ????
            HtmlDocument pg = new HtmlDocument();
            if (d.getElementById("ContentPlaceHolder1_pgDownLoad") == null)
            {
                /// 沒有ContentPlaceHolder1_pg_gvList, 表示只有ㄧ頁
                total_pages = 1;
            }
            else
            {
                /// 有ContentPlaceHolder1_pg_gvList, 表示有多頁
                /// 舊方法 pg_N = pg.Children.Count - 5;
                /// 如果多頁, 轉換loadcomplete, 呼叫pager by click
                // 20200502: outerHTML的XPATH="//selection/option", innerHTML的XPATH="//option"
                pg.LoadHtml(d.getElementById("ctl00$ContentPlaceHolder1$pgDownLoad_input").innerHTML);
                HtmlNodeCollection o = pg.DocumentNode.SelectNodes("//option");
                total_pages = o.Count;
            }

            #endregion 判斷多頁

            /// 前往讀取第一頁
            current_page = 1;
            Vpn_PageData();
        }

        private void Vpn_PageData()
        {
            /// 取得gvList
            HTMLDocument d = (HTMLDocument)vpnweb.Document;
            IHTMLElement gvDownLoad = d.getElementById("ContentPlaceHolder1_gvDownLoad");

            /// 讀取
            /// 20200503 我發現Html Agility Pack不能click
            /// 只好第一層是ihtmlelement
            IHTMLElementCollection trs_ = gvDownLoad.all;
            IHTMLElementCollection trs = trs_.tags("tr");

            // 使用item這個方法可以將集合中的元素取出
            // 第一個參數代表的是順序,但是在msdn中標示為name
            // 第二個參數msdn中標示為index,但經過測試後,指的並不是順序,所以目前無法確定他的用途
            // 如果有知道的朋友,也請跟我說一下
            // current_line = 0 是標題行

            // initialization
            current_line = 1;
            // total_lines 比較少一條, 因為有標題列
            total_lines = trs.length - 1;

            Vpn_LineData();
        }

        private void Vpn_LineData()
        {
            vpnweb.LoadCompleted -= Vpn_Page_LoadCompleted;
            vpnweb.LoadCompleted -= Vpn_Line_LoadCompleted;

            if (should_click)
            {
                System.Threading.Thread.Sleep(1000);

                System.Threading.ThreadStart th_begin = new System.Threading.ThreadStart(Work_todo);
                System.Threading.Thread thr = new System.Threading.Thread(th_begin)
                {
                    IsBackground = true,
                    Name = "PressS"
                };
                thr.Start();
                should_click = false;
            }

            /// 取得gvList
            HTMLDocument d = (HTMLDocument)vpnweb.Document;
            IHTMLElement gvDownLoad = d.getElementById("ContentPlaceHolder1_gvDownLoad");
            DateTime current_time = DateTime.Now;

            /// 讀取
            /// 20200503 我發現Html Agility Pack不能click
            /// 只好第一層是ihtmlelement

            #region "Write"

            NHIDataContext dc = new NHIDataContext();
            IHTMLElementCollection trs_ = gvDownLoad.all;
            IHTMLElementCollection trs = trs_.tags("tr");

            // 使用item這個方法可以將集合中的元素取出
            // 第一個參數代表的是順序,但是在msdn中標示為name
            // 第二個參數msdn中標示為index,但經過測試後,指的並不是順序,所以目前無法確定他的用途
            // 如果有知道的朋友,也請跟我說一下
            // current_line = 0 是標題行

            IHTMLElement tr = trs.item(current_line, null);
            HtmlDocument h_ = new HtmlDocument();
            h_.LoadHtml(tr.innerHTML);
            HtmlNodeCollection tds = h_.DocumentNode.SelectNodes("//td");

            short order_n = 0;
            string s_f_name = string.Empty, s_f_remark = string.Empty, s_remark = string.Empty;
            DateTime d_SDATE = DateTime.Now;
            bool b_archive = false;

            foreach (HtmlNode td in tds)
            {
                switch (order_n)
                {
                    case 0:  //檔案名稱
                        s_f_name = td.InnerText;
                        break;

                    case 1: //檔案說明
                        s_f_remark = td.InnerText;
                        break;

                    case 2: //下載備註
                        s_remark = td.InnerText;
                        break;

                    case 3: //提供下載日期
                        if (td.InnerText != string.Empty)
                        {
                            HtmlNode div = td.ChildNodes[1]; // 怪,第一個member竟然是/r/n
                            string[] temp_s = div.InnerHtml.Replace("<br>", "|").Split('|');
                            string[] temp_d = temp_s[0].Split('/');
                            d_SDATE = DateTime.Parse($"{int.Parse(temp_d[0]) + 1911}/{temp_d[1]}/{temp_d[2]} {temp_s[1]}");
                        }
                        break;

                    case 4: //檔案下載
                        if (td.SelectNodes("//a").Count == 2)
                        {
                            b_archive = false;
                        }
                        else
                        {
                            b_archive = true;
                        }
                        break;

                    default:
                        break;
                }
                order_n++;
            }

            var q = from p in dc.tbl_download
                    where (p.f_name == s_f_name) && (p.SDATE == d_SDATE)
                    select p;
            //if (q.Count() == 0)
            //{
            tbl_download newNHI = new tbl_download()
            {
                QDATE = current_time,
                f_name = s_f_name,
                f_remark = s_f_remark,
                remark = s_remark,
                SDATE = d_SDATE
            };
            //存檔
            if (b_archive)
            {
                newNHI.download = false;
            }
            else
            {
                should_click = true;
                newNHI.download = true;
            }
            //dc.tbl_download.InsertOnSubmit(newNHI);
            //dc.SubmitChanges();
            //}

            #endregion "Write"

            // 判斷是否最後一行了?
            if ((current_line == total_lines) && (current_page == total_pages))
            {
                // 已讀完所有頁面及所有行數
                // 結束
                return;
            }
            else if (current_line == total_lines)
            {
                // 如果還有下一頁, 前往下一頁
                current_page++;
                vpnweb.LoadCompleted += Vpn_Page_LoadCompleted;
                // 按鈕機制
                foreach (IHTMLElement a in d.getElementById("ContentPlaceHolder1_pgDownLoad").all)
                {
                    if (a.innerText == ">")
                    {
                        a.click();
                    }
                }
            }
            else
            {
                // 還有下一行, 此時有兩種情形, 第一種該存檔, 第二種不用存檔
                // to do
                current_line++;
                if (should_click)
                {
                    //vpnweb.LoadCompleted += Vpn_Line_LoadCompleted;
                    IHTMLElement a = tr.children[4].children[0];
                    a.click();
                    System.Threading.Thread.Sleep(1000);
                    Debug.WriteLine($"button click: {a.outerHTML}");
                }
                else
                {
                    // 下一行
                    Vpn_LineData();
                }
            }
        }

        private void Vpn_Page_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Vpn_PageData();
        }

        private void Vpn_Line_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Vpn_LineData();
        }

        private void Work_todo()
        {
            InputSimulator sim = new InputSimulator();
            System.Threading.Thread.Sleep(4000);
            sim.Keyboard.KeyPress(VirtualKeyCode.VK_S);
            System.Threading.Thread.Sleep(1000);
            sim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            NHIDataContext dc = new NHIDataContext();
            DLData.ItemsSource = dc.tbl_download;
        }

        private void SP_Click(object sender, RoutedEventArgs e)
        {
            this._timer1 = new System.Timers.Timer
            {
                Interval = 6000
            };
            this._timer1.Elapsed += new System.Timers.ElapsedEventHandler(TimersTimer_Elapsed);

            // initialization
            current_line = 0;
            queue_item.Enqueue(1);
            queue_item.Enqueue(3);
            queue_item.Enqueue(5);
            queue_item.Enqueue(7);
            queue_item.Enqueue(9);
            current_line = queue_item.Dequeue();

            this._timer1.Start();


        }

        private void TimersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                HTMLDocument d = (HTMLDocument)vpnweb.Document;
                IHTMLElement gvDownLoad = d.getElementById("ContentPlaceHolder1_gvDownLoad");
                IHTMLElementCollection trs_ = gvDownLoad.all;
                IHTMLElementCollection trs = trs_.tags("tr");
                IHTMLElement tr = trs.item(current_line, null);
                IHTMLElement a = tr.children[4].children[0];
                a.click();
                Debug.WriteLine($"button click: {a.outerHTML}");

                System.Threading.ThreadStart th_begin = new System.Threading.ThreadStart(Work_todo);
                System.Threading.Thread thr = new System.Threading.Thread(th_begin)
                {
                    IsBackground = true,
                    Name = "PressS"
                };
                thr.Start();

                if (queue_item.Count == 0)
                {
                    this._timer1.Stop();
                }
                else
                {
                    current_line = queue_item.Dequeue();
                }
            }));
        }


        #endregion "Buttons"
    }
}