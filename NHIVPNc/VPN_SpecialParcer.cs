using HtmlAgilityPack;
using NHIVPNc.Models;
using System;

namespace NHIVPNc
{
    internal partial class VPN_Downloader : IDisposable
    {
        private tbl_download Special_TD_Parcer(HtmlNodeCollection tds)
        {
            log.Info("  Enter Special_TD_Parcer");
            short order_n = 0;
            tbl_download newNHI = new tbl_download();
            foreach (HtmlNode td in tds)
            {
                try
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
                                // 20200522, 原來要Trim掉頭尾的空白,這樣就正確了
                                string[] temp_s = td.InnerText.Trim().Split(' ');
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
                catch (Exception ex)
                {
                    log.Error($"[{order_n}], error:{ex.Message}");
                }
            }
            if ((bool)newNHI.download)
            {
                VPN_files.Add(current_line, newNHI.f_name);
                log.Info($"    {current_line}, {newNHI.f_name} added to VPN_files.");
            }
            log.Info("  Exit Special_TD_Parcer");
            return newNHI;
        }
    }
}
