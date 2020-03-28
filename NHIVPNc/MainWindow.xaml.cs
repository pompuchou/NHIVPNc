using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

namespace NHIVPNc
{
    /// <summary>
    /// 2020/03/22 created
    /// 這是我第一個cs, WPF的程式
    /// 2020/03/28 什麼是相對於winform的webbrowser
    /// menustrip=>menu; 
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void VPN_click(object sender, RoutedEventArgs e)
        {
            /// 2020/03/28 created, transcribed from vb.net
            vpnweb.Navigate("https://medvpn.nhi.gov.tw/ieae0000/IEAE0200S01.aspx");
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            NHIDataContext dc = new NHIDataContext();
            DLData.ItemsSource = dc.tbl_download;
        }
    }
}
