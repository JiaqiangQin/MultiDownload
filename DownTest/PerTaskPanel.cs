using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Configuration;

namespace DownTest
{
    /// <summary>
    /// 下载任务自定义控件
    /// </summary>
    public partial class PerTaskPanel : UserControl
    {
        #region 事件
        public delegate void DeleteTask(string httpurl, string saverootpath, string msg);
        public event DeleteTask deleteTask;

        public delegate void LogTask(string msg);
        public event LogTask logTask;

        public delegate void ReFreshHandler(DownStatus status, long size, long currentsize, Exception e);
        #endregion

        #region 变量
        private string httpurl = "";
        private string saverootpath = "";
        private MultiDownload down = new MultiDownload(1, "", "");
        #endregion

        public PerTaskPanel()
        {
            InitializeComponent();

            this.Load += UserControl1_Load;
            this.btnstop.Click += Btnstop_Click;
            this.btnstart.Click += Btnstart_Click;
            this.btnrestart.Click += Btnrestart_Click;
            this.btnopen.Click += Btnopen_Click1;
            this.btnclose.Click += Btnclose_Click;
        }

        /// <summary>
        /// 开始任务
        /// </summary>
        public void NewTask(string httpurl, string saverootpath)
        {
            this.httpurl = httpurl;
            this.saverootpath = saverootpath;

            var threadNum = int.Parse(ConfigurationManager.AppSettings["ThreahNum"].ToString());
            if (threadNum <= 0)
            {
                threadNum = 1;
            }
            down = new MultiDownload(threadNum, httpurl, saverootpath);
            down.Reading -= Down_mReading;
            down.Reading += Down_mReading;

            SetButtons(false);
            this.txthttpurl.Text = httpurl;

            //开始任务
            this.BeginInvoke(new Action(Start));
        }

        private void Start()
        {
            Btnstart_Click(null, null);
        }

        #region 任务进度

        private void Down_mReading(DownStatus status, long size, long currentsize, Exception e)
        {
            this.Invoke(new ReFreshHandler(ReFresh), status, size, currentsize, e);
        }

        private void ReFresh(DownStatus status, long size, long currentsize, Exception e)
        {
            switch (status)
            {
                case DownStatus.Downing:
                    if (true)
                    {
                        var percent = 0;
                        try
                        {
                            if (size > 0)
                            {
                                percent = (int)(((float)currentsize / (float)size) * 100);
                            }
                            if (percent > 100)
                            {
                                percent = 100;
                            }
                        }
                        catch (Exception)
                        {
                        }
                        var mconvert = new SizeConvert();
                        this.lblpercent.Text = string.Format("{0}/{1}", mconvert.GetSize(currentsize), mconvert.GetSize(size));
                        this.barstatus.Value = percent;
                    }
                    break;
                case DownStatus.Stop:
                    if (true)
                    {
                        SetButtons(false);
                        if (logTask != null)
                        {
                            logTask(string.Format("任务：{0}异常：{1}，堆栈：{2}！", httpurl, e.Message, e.StackTrace));
                        }
                    }
                    break;
                case DownStatus.Complete:
                    if (true)
                    {
                        var mconvert = new SizeConvert();
                        this.lblpercent.Text = string.Format("{0}/{1}", mconvert.GetSize(size), mconvert.GetSize(size));
                        this.barstatus.Value = 100;
                        if (logTask != null)
                        {
                            logTask(string.Format("任务：{0}已完成！", httpurl));
                            //标记停止
                            down.Stop();
                        }
                        SetButtons(false);
                    }
                    break;
                case DownStatus.Deleted:
                    if (true)
                    {
                        var mconvert = new SizeConvert();
                        this.lblpercent.Text = string.Format("{0}/{1}", mconvert.GetSize(0), mconvert.GetSize(0));
                        if (logTask != null)
                        {
                            logTask(string.Format("任务：{0}临时文件已删除！", httpurl));
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region 控件事件
        /// <summary>
        /// 加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl1_Load(object sender, EventArgs e)
        {
            SetButtons(false);
            var mconvert = new SizeConvert();
            this.lblpercent.Text = string.Format("{0}/{1}", mconvert.GetSize(0), mconvert.GetSize(0));
        }
        /// <summary>
        /// 开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnstart_Click(object sender, EventArgs e)
        {
            SetButtons(true);
            down.Start();
            if (logTask != null)
            {
                logTask(string.Format("任务：{0}开始！", httpurl));
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnstop_Click(object sender, EventArgs e)
        {
            down.Stop();
            SetButtons(false);
            if (logTask != null)
            {
                logTask(string.Format("任务：{0}已暂停！", httpurl));
            }
        }

        /// <summary>
        /// 重新下载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnrestart_Click(object sender, EventArgs e)
        {
            SetButtons(false);
            down.DeleteTmpFile();
            Btnstart_Click(null, null);
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnopen_Click1(object sender, EventArgs e)
        {
            var savepath = down.SavePath;
            //文件不存在时，打开文件夹
            if (File.Exists(savepath) == false)
            {
                savepath = saverootpath;
            }
            Func.OpenFileDirectory(savepath);
        }

        /// <summary>
        /// 关闭任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnclose_Click(object sender, EventArgs e)
        {
            if (logTask != null)
            {
                logTask(string.Format("任务：{0}任务手动关闭！", httpurl));
            }
            down.Stop();
            this.Parent.Controls.Remove(this);
            if (deleteTask != null)
            {
                deleteTask(httpurl, saverootpath, "");
            }
        }

        #endregion

        #region 私有方法
        /// <summary>
        /// 控制按钮状态
        /// </summary>
        /// <param name="downing"></param>
        private void SetButtons(bool downing)
        {
            if (downing)
            {
                this.btnstop.Enabled = true;
                this.btnstart.Enabled = false;
                this.btnrestart.Enabled = false;
                this.btnopen.Enabled = true;
            }
            else
            {
                this.btnstop.Enabled = false;
                this.btnstart.Enabled = true;
                this.btnrestart.Enabled = true;
                this.btnopen.Enabled = true;
            }
        }
        #endregion
    }
}
