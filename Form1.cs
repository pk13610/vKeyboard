﻿using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MouseClickTool
{/// <summary>
/// 怎么简单怎么来了
/// </summary>
/// 
    public partial class Form1 : Form
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        //新方法：https://stackoverflow.com/questions/5094398/how-to-programmatically-mouse-move-click-right-click-and-keypress-etc-in-winfo
        internal class MouseSimulator
        {
            [DllImport("user32.dll", SetLastError = true)]
            static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

            [StructLayout(LayoutKind.Sequential)]
            struct INPUT
            {
                public SendInputEventType type;
                public MouseKeybdhardwareInputUnion mkhi;
            }

            [StructLayout(LayoutKind.Explicit)]
            struct MouseKeybdhardwareInputUnion
            {
                [FieldOffset(0)]
                public MouseInputData mi;
            }

            [Flags]
            enum MouseEventFlags : uint
            {
                MOUSEEVENTF_LEFTDOWN = 0x0002,
                MOUSEEVENTF_LEFTUP = 0x0004,
                MOUSEEVENTF_RIGHTDOWN = 0x0008,
                MOUSEEVENTF_RIGHTUP = 0x0010,
            }

            [StructLayout(LayoutKind.Sequential)]
            struct MouseInputData
            {
                public int dx;
                public int dy;
                public uint mouseData;
                public MouseEventFlags dwFlags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            enum SendInputEventType : int
            {
                InputMouse
            }

            public static void ClickLeftMouseButton()
            {
                INPUT mouseDownInput = new INPUT();
                mouseDownInput.type = SendInputEventType.InputMouse;
                mouseDownInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
                SendInput(1, ref mouseDownInput, Marshal.SizeOf(new INPUT()));

                INPUT mouseUpInput = new INPUT();
                mouseUpInput.type = SendInputEventType.InputMouse;
                mouseUpInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
                SendInput(1, ref mouseUpInput, Marshal.SizeOf(new INPUT()));
            }

            public static void ClickRightMouseButton()
            {
                INPUT mouseDownInput = new INPUT();
                mouseDownInput.type = SendInputEventType.InputMouse;
                mouseDownInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTDOWN;
                SendInput(1, ref mouseDownInput, Marshal.SizeOf(new INPUT()));

                INPUT mouseUpInput = new INPUT();
                mouseUpInput.type = SendInputEventType.InputMouse;
                mouseUpInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTUP;
                SendInput(1, ref mouseUpInput, Marshal.SizeOf(new INPUT()));
            }
        }

        private int periord_ = 0;
        private bool is_stop_ = false;
        private MouseClickTool.KeyboardHook hook_ = new KeyboardHook();

        public Form1()
        {
            InitializeComponent();
            this.comboBox1.SelectedIndex = 0;
            is_begin.Click += (s, e) =>
            {
                if (is_begin.Text.Equals("停止"))
                {
                    Environment.Exit(0);
                    return;
                }

                periord_ = int.Parse(is_ms.Text);
                if (periord_ < 100)
                {
                    MessageBox.Show("输入的数字不正确，必须是正整数");
                    return;
                }
                is_ms.ReadOnly = true;
                Task.Factory.StartNew(async () =>
                {
                    await Task.Run(() =>
                    {
                        for (int i = 1; i < 5; i++)
                        {
                            is_begin.Text = string.Format("开始({0})", 5 - i);
                            Thread.Sleep(1000);
                        }
                    });
                    is_begin.Text = "停止";
                    if (this.comboBox1.SelectedIndex == 0)
                    {
                        for (; !is_stop_; )
                        {
                            await Task.Run(() =>
                            {
                                MouseSimulator.ClickLeftMouseButton();
                                Thread.Sleep(periord_);
                            });
                        }
                    }
                    else
                    {
                        for (; !is_stop_; )
                        {
                            await Task.Run(() =>
                            {
                                MouseSimulator.ClickRightMouseButton();
                                Thread.Sleep(periord_);
                            });
                        }
                    }
                    is_begin.Text = "开始";
                });
            };

            MouseClickTool.KeyboardHook.KeyPressEvent += new EventHandler(KeyPressEvent);
            hook_.Hook_Start();
        }

        private void KeyPressEvent(object sender, EventArgs e)
        {
            var keycode = (System.Windows.Forms.Keys) Convert.ToInt32(sender);
            switch (keycode)
            {
                case System.Windows.Forms.Keys.F1:
                    periord_ += 100;
                    if (periord_ > 10000) periord_ = 1000;
                    break;
                case System.Windows.Forms.Keys.F2:
                    periord_ -= 100;
                    if (periord_ < 100) periord_ = 100;
                    break;
                case System.Windows.Forms.Keys.F4:
                    is_stop_ = !is_stop_;
                    break;
            }
            is_ms.Text = periord_.ToString();
        }
    }
}