using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;

namespace key_sender
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Thread backThread;
        public const int GW_CHILD = 5;
        public const int GW_HWNDNEXT = 2;
        const uint WM_KEYDOWN = 0x100;
        const int VK_RETURN = 0x0D;
        private string[] combination = new string[76];
        string appName = "";

        #region dllImports
        [DllImport("user32.dll")]
        //public static extern int FindWindow(string WindowClassName, string WindowName);
        //[DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32")]
        public static extern int GetDesktopWindow();
        [DllImport("User32.Dll")]
        public static extern void GetWindowText(int h, StringBuilder s, int nMaxCount);
        [DllImport("user32")]
        public static extern int IsWindowVisible(int hwnd);
        [DllImport("user32")]
        public static extern int GetWindow(int hwnd, int wCmd);
        //[DllImport("user32.dll")]
        //public static extern int SetForegroundWindow(int hWnd);

        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
        //[DllImport("user32.dll")]
        //public static extern int SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);


        [DllImport("user32.dll")]
        static extern byte VkKeyScan(char ch); 

        #endregion


        private void Form1_Load(object sender, EventArgs e)
        {
            cb_listWin.Items.Clear();
            int index = 0;
            for (int i = 47; i <= 122; i++)
            {
                if (index == 0)
                    combination[index] = string.Empty;
                else
                    combination[index] = Convert.ToChar(i).ToString();
                index++;
            }
            
            int nDeshWndHandle = GetDesktopWindow();
            // Get the first child window
            int nChildHandle = GetWindow(nDeshWndHandle, GW_CHILD);

            while (nChildHandle != 0)
            {
                //If the child window is this (SendKeys) application then ignore it.
                if (nChildHandle == this.Handle.ToInt32())
                {
                    nChildHandle = GetWindow(nChildHandle, GW_HWNDNEXT);
                }

                // Get only visible windows
                if (IsWindowVisible(nChildHandle) != 0)
                {
                    StringBuilder sbTitle = new StringBuilder(1024);
                    // Read the Title bar text on the windows to put in combobox
                    GetWindowText(nChildHandle, sbTitle, sbTitle.Capacity);
                    String sWinTitle = sbTitle.ToString();
                    {
                        if (sWinTitle.Length > 0)
                        {
                            cb_listWin.Items.Add(sWinTitle);
                        }
                    }
                }
                // Look for the next child.
                nChildHandle = GetWindow(nChildHandle, GW_HWNDNEXT);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            appName=cb_listWin.SelectedItem.ToString();
            if(!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }
        private void setKeys()
        {
            label1.Text = code;
        }
        string code = "";
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            /////////////////////////////////////////////////
            backThread = Thread.CurrentThread;
            IntPtr iHandle = IntPtr.Zero;
            Process[] proc = Process.GetProcesses();
            foreach (Process p in proc)
            {
                if (p.MainWindowTitle == appName)
                {
                    iHandle = FindWindowEx(p.MainWindowHandle, IntPtr.Zero, null, null);
                    break;
                }
            }
            if (iHandle == IntPtr.Zero)
            {
                MessageBox.Show("Window could not be found");
                return;
            }
            
            string tempCode = string.Empty;
            for (int a = 0; a < combination.Length; a++)
            {
                for (int b = 0; b < combination.Length; b++)
                {
                    for (int c = 0; c < combination.Length; c++)
                    {
                        for (int d = 0; d < combination.Length; d++)
                        {
                            code = combination[a] + combination[b] + combination[c] + combination[d];
                            for (int i = 0; i < code.Length; i++)//this loop is used to send the code
                            {
                                PostMessage(iHandle, WM_KEYDOWN, VkKeyScan(code[i]), 0);
                            }
                            //SendMessage(iHandle, WM_KEYDOWN, VK_RETURN, 0);
                            PostMessage(iHandle, WM_KEYDOWN, VK_RETURN, 0); 
                            Thread.Sleep(300);



                        }
                    }
                }
            }
            
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            setKeys();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            backThread.Abort();
            backgroundWorker1.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form1_Load(sender, e);
        }

    }
}