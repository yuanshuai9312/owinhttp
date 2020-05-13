using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace WebService
{

    /// <summary>
    /// 
    /// 一个HttpListener的测试类
    /// 作者：ehung
    /// 
    /// 
    /// </summary>
    public class HttpControl
    {
        private HttpListener _httplistener;
        private string _path = Application.StartupPath;
        private string _webpath = "webroot";
        private Thread _thread;

        public HttpControl()
        {
            _httplistener = new HttpListener();
        }

        public void Start()
        {
            _thread = new Thread(new ThreadStart(StartWork));
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Stop()
        {
            if (_thread != null)
                if (_thread.IsAlive)
                    _thread.Abort();

        }
        private void StartWork()
        {

            _httplistener = new HttpListener();
            _httplistener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            //_httplistener.Prefixes.Add("http://127.0.0.1:8080");
            _httplistener.Prefixes.Add("http://localhost:8080/");
            _httplistener.Start();

            while (_thread.IsAlive)
            {
                HttpListenerContext httpcon = _httplistener.GetContext();
                httpcon.Response.StatusCode = 200;

                string filename = GetFileName(httpcon.Request.RawUrl);
                IPEndPoint requestEndpoint = httpcon.Request.RemoteEndPoint;
                if (string.IsNullOrEmpty(filename))
                    continue;
                filename = _path + "\\" + _webpath + "\\" + filename;
                if (!File.Exists(filename))
                    continue;
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                
                Stream s = httpcon.Response.OutputStream;
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    if (fs.Read(buffer, 0, buffer.Length) == 0)
                        break;
                    s.Write(buffer,0,buffer.Length);
                   
                }
                s.Close();
                fs.Close();
               

            }
            _httplistener.Close();
        }

        private string GetFileName(string data)
        {
            if (string.IsNullOrEmpty(data) || data.Length == 1)
                return "index.html";
            string filename = data.Substring(1).Replace("/", "\\");
            string filenamereg = ".htm|.jpg|.png|.gif|.css|.js";
            Regex reg = new Regex(filenamereg);
            if (reg.IsMatch(filename))
                return filename;
            return "";

        }
    }
}
