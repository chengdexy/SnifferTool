using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnifferTool
{
    class Program
    {
        static NetCut.NetCut nc = new NetCut.NetCut();
        static WebBrowser wb = new WebBrowser();
        static bool _continue = true;
        static string vid;
        public static int counter = 0;

        [STAThread]
        static void Main(string[] args)
        {
            vid = args[0];

            if (string.IsNullOrEmpty(vid))
            {
                Console.WriteLine("请求的URL暂不被支持,目前仅支持由腾讯公众平台发布的包含视频的链接.");
                return;
            }

            nc.RequestComplete += Nc_RequestComplete;
            nc.Install();
            wb.Navigate("http://localhost/");
            wb.Navigate($"https://v.qq.com/iframe/preview.html?vid={vid}&amp;auto=1");
            while (_continue)
            {
                Application.DoEvents();
            }
            Console.ReadKey();
        }

        private static void Nc_RequestComplete(object sender, NetCut.NetTab e)
        {

            string requestStr = e.Service.ToString() + ":/" + e.Url;
            if (counter > 10)
            {
                Console.WriteLine("解析超时,可能是网络拥堵,请过两分钟再试.");
                nc.Uninstall();
                _continue = false;
                return;
            }
            if (requestStr.Contains(vid) && requestStr.Contains(".mp4") && requestStr.Contains("vkey="))
            {
                Console.WriteLine("Found one:" + DateTime.Now.ToString("ss"));
                string testUrl = e.Url.Split('/')[1];
                if (testUrl.Contains("qq.com"))
                {
                    Console.WriteLine(requestStr);
                    nc.Uninstall();
                    _continue = false;
                }
                else
                {
                    Console.WriteLine("Pass it,refresh:" + DateTime.Now.ToString("ss"));
                    counter++;

                    wb.Refresh();
                }

            }
            else
            {
                Console.WriteLine("Pass one:" + DateTime.Now.ToString("ss"));
            }
        }
    }
}
