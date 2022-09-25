using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseClickTool
{
    internal class KeyboardHook
    {
        static int hHook = 0;
        public delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        //LowLevel键盘截获，如果是WH_KEYBOARD＝2，并不能对系统键盘截取，Acrobat Reader会在你截取之前获得键盘。 
        HookProc KeyBoardHookProcedure;
        public const int WH_KEYBOARD_LL = 13;
        //键盘Hook结构函数 
        [StructLayout(LayoutKind.Sequential)]
        public class KeyBoardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        //设置钩子 
        [DllImport("user32.dll")]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //抽掉钩子 
        public static extern bool UnhookWindowsHookEx(int idHook);
        [DllImport("user32.dll")]
        //调用下一个钩子 
        public static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        public void Hook_Start()
        {
            // 安装键盘钩子 
            if (hHook == 0)
            {
                KeyBoardHookProcedure = new HookProc(KeyBoardHookProc);

                hHook = SetWindowsHookEx(WH_KEYBOARD_LL,
                KeyBoardHookProcedure,
                GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName), 0);
                if (hHook == 0)
                {
                    Hook_Clear();
                    throw new Exception("设置Hook失败!"); 
                }
            }
        }

        //取消钩子事件 
        public void Hook_Clear()
        {
            bool retKeyboard = true;
            if (hHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hHook);
                hHook = 0;
            }
            //如果去掉钩子失败. 
            if (!retKeyboard) throw new Exception("UnhookWindowsHookEx failed.");
        }

        //这里可以添加自己想要的信息处理 
        public static int KeyBoardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KeyBoardHookStruct kbh = (KeyBoardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyBoardHookStruct));

                var vkcode = (System.Windows.Forms.Keys)kbh.vkCode;
                if (vkcode == System.Windows.Forms.Keys.F1
                    || vkcode == System.Windows.Forms.Keys.F2
                    || vkcode == System.Windows.Forms.Keys.F4
                    )
                {
                    KeyPressEvent.Invoke(vkcode, new EventArgs());
                    return 1;
                }
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        public static event EventHandler KeyPressEvent;
    }
}
