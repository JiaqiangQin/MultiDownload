using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DownTest
{
    public partial class NewTaskForm : Form
    {
        #region 事件
        public delegate void NewTask(string httpurl, string saverootpath);
        public event NewTask newTask;
        #endregion

        public NewTaskForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

            this.Load += Form2_Load;
            this.btnNew.Click += BtnNew_Click;
            this.btnSelectRootPath.Click += BtnSelectRootPath_Click;
            //this.btnTest.Click += BtnTest_Click;
            this.btnopen.Click += Btnopen_Click;
            this.KeyPreview = true;//键盘事件传递
            this.KeyDown += NewTaskForm_KeyDown;
            this.Focus();
            this.txthttpurl.Focus();
        }

        #region 事件
        /// <summary>
        /// 加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form2_Load(object sender, EventArgs e)
        {
            //文件夹地址
            var path = GetRootPath().Trim();
            this.txtsaverootpath.Text = path;
            //测试地址
            this.txthttpurl.Text = "http://cr1a.197946.com/premiumsoftnavicatpremiumx64zcj.zip";
        }
        /// <summary>
        /// 回车事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewTaskForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnNew_Click(null, null);
                return;
            }
        }

        /// <summary>
        /// 设置测试下载地址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTest_Click(object sender, EventArgs e)
        {
            this.txthttpurl.Text = "http://cr1a.197946.com/premiumsoftnavicatpremiumx64zcj.zip";
        }

        /// <summary>
        /// 选择保存路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSelectRootPath_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "请选择所在文件夹";
            //默认文件夹
            var currentsaverootpath = this.txtsaverootpath.Text;
            if (Directory.Exists(currentsaverootpath))
            {
                dialog.SelectedPath = currentsaverootpath;
            }
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (string.IsNullOrEmpty(dialog.SelectedPath))
                {
                    return;
                }
                this.txtsaverootpath.Text = dialog.SelectedPath.Trim();
                //保存
                var path = this.txtsaverootpath.Text.Trim();
                SetRootPath(path);
            }
        }

        /// <summary>
        /// 新建任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNew_Click(object sender, EventArgs e)
        {
            var httpurl = this.txthttpurl.Text.Trim();
            var saverootpath = this.txtsaverootpath.Text.Trim();

            var down = new MultiDownload(0, httpurl, saverootpath);
            var msg = "";
            var isvalid = down.IsValid(out msg);
            if (isvalid == false)
            {
                MessageBox.Show(msg);
                this.txthttpurl.Focus();
                return;
            }
            if (newTask != null)
            {
                newTask(httpurl, saverootpath);
            }
            this.Close();
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btnopen_Click(object sender, EventArgs e)
        {
            var httpurl = this.txthttpurl.Text.Trim();
            var saverootpath = this.txtsaverootpath.Text.Trim();

            var down = new MultiDownload(0, httpurl, saverootpath);

            var savepath = down.SavePath;
            //文件不存在时，打开文件夹
            if (File.Exists(savepath) == false)
            {
                savepath = saverootpath;
            }
            Func.OpenFileDirectory(savepath);
        }
        #endregion

        #region 私有方法

        /// <summary>
        /// 存储保存文件路径
        /// </summary>
        /// <param name="path"></param>
        private void SetRootPath(string path)
        {
            var logfile = Environment.CurrentDirectory + "\\config\\" + "rootpath.config";

            Func.WriteFile(logfile, path, false);
        }
        /// <summary>
        /// 读取保存文件路径
        /// </summary>
        /// <returns></returns>
        private string GetRootPath()
        {
            var logfile = Environment.CurrentDirectory + "\\config\\" + "rootpath.config";

            var path = "";
            if (File.Exists(logfile))
            {
                var data = Func.ReadFile(logfile).FirstOrDefault();
                path = data;
            }
            if (string.IsNullOrEmpty(path))
            {
                path = @"C:\Users\Administrator\Desktop";
            }
            return path;
        }
        #endregion

    }
}
