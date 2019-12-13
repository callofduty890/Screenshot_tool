using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using HalconDotNet;

namespace 窗口截图_01
{




    public struct RECT
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;
    }

    public partial class Form1 : Form
    {


        [DllImport("kernel32.dll")]
        public static extern void CopyMemory(int Destination, int Source, int Length);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        //创建截图类
        class ScreenCapture
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        private static extern IntPtr ReleaseDC(IntPtr hc, IntPtr hDest);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowRect(IntPtr hWnd, out RECT rect);
        [DllImport("user32.dll")]
        private static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, UInt32 nFlags);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDC(
             string lpszDriver,         // driver name驱动名
             string lpszDevice,         // device name设备名
             string lpszOutput,         // not used; should be NULL
             IntPtr lpInitData          // optional printer data
         );
        [DllImport("gdi32.dll")]
        private static extern int BitBlt(
             IntPtr hdcDest, // handle to destination DC目标设备的句柄
             int nXDest,   // x-coord of destination upper-left corner目标对象的左上角的X坐标
             int nYDest,   // y-coord of destination upper-left corner目标对象的左上角的Y坐标
             int nWidth,   // width of destination rectangle目标对象的矩形宽度
             int nHeight, // height of destination rectangle目标对象的矩形长度
             IntPtr hdcSrc,   // handle to source DC源设备的句柄
             int nXSrc,    // x-coordinate of source upper-left corner源对象的左上角的X坐标
             int nYSrc,    // y-coordinate of source upper-left corner源对象的左上角的Y坐标
             CopyPixelOperation dwRop   // raster operation code光栅的操作值
         );
        //static extern int BitBlt(IntPtr hdcDest, int xDest, int yDest, int
        //wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(
         IntPtr hdc // handle to DC
         );
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(
             IntPtr hdc,         // handle to DC
             int nWidth,      // width of bitmap, in pixels
             int nHeight      // height of bitmap, in pixels
         );
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(
             IntPtr hdc,           // handle to DC
             IntPtr hgdiobj    // handle to object
         );
        [DllImport("gdi32.dll")]
        private static extern int DeleteDC(
            IntPtr hdc           // handle to DC
         );

        /// <summary>
        /// 抓取屏幕(层叠的窗口)
        /// </summary>
        /// <param name="x">左上角的横坐标</param>
        /// <param name="y">左上角的纵坐标</param>
        /// <param name="width">抓取宽度</param>
        /// <param name="height">抓取高度</param>
        /// <returns></returns>
        public Bitmap CaptureScreen(int x, int y, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(new Point(x, y), new Point(0, 0), bmp.Size);
                g.Dispose();
            }
            //bit.Save(@"capture2.png");
            return bmp;
        }

        /// <summary>
        /// 抓取整个屏幕
        /// </summary>
        /// <returns></returns>
        public Bitmap CaptureScreen()
        {
            Size screenSize = Screen.PrimaryScreen.Bounds.Size;
            return CaptureScreen(0, 0, screenSize.Width, screenSize.Height);
        }

        /// <summary>
        /// 全屏截图
        /// </summary>
        /// <returns></returns>
        public Image CaptureScreenI()
        {
            return CaptureWindow(GetDesktopWindow());
        }

        /// <summary>
        /// 全屏指定区域截图
        /// </summary>
        /// <returns></returns>
        public Image CaptureScreenI(RECT rect)
        {
            return CaptureWindow(GetDesktopWindow(), rect);
        }

        /// <summary>
        /// 指定窗口截图
        /// </summary>
        /// <param name="handle">窗口句柄. (在windows应用程序中, 从Handle属性获得)</param>
        /// <returns></returns>
        public Bitmap CaptureWindow(IntPtr hWnd)
        {
            IntPtr hscrdc = GetWindowDC(hWnd);
            RECT rect = new RECT();
            return CaptureWindow(hWnd, rect);
        }

        /// <summary>
        /// 指定窗口区域截图
        /// </summary>
        /// <param name="handle">窗口句柄. (在windows应用程序中, 从Handle属性获得)</param>
        /// <param name="rect">窗口中的一个区域</param>
        /// <returns></returns>
        public Bitmap CaptureWindow(IntPtr hWnd, RECT rect)
        {
            // 获取设备上下文环境句柄
            IntPtr hscrdc = GetWindowDC(hWnd);

            // 创建一个与指定设备兼容的内存设备上下文环境（DC）
            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            IntPtr myMemdc = CreateCompatibleDC(hscrdc);

            // 返回指定窗体的矩形尺寸
            RECT rect1;
            GetWindowRect(hWnd, out rect1);

            // 返回指定设备环境句柄对应的位图区域句柄
            IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, rect1.x2 - rect1.x1, rect1.y2 - rect1.y1);
            IntPtr myBitmap = CreateCompatibleBitmap(hscrdc, rect.x2 - rect.x1, rect.y2 - rect.y1);

            //把位图选进内存DC 
            // IntPtr OldBitmap = (IntPtr)SelectObject(hmemdc, hbitmap);
            SelectObject(hmemdc, hbitmap);
            SelectObject(myMemdc, myBitmap);

            /////////////////////////////////////////////////////////////////////////////
            //
            // 下面开始所谓的作画过程，此过程可以用的方法很多，看你怎么调用 API 了
            //
            /////////////////////////////////////////////////////////////////////////////

            // 直接打印窗体到画布
            PrintWindow(hWnd, hmemdc, 0);

            // IntPtr hw = GetDesktopWindow();
            // IntPtr hmemdcClone = GetWindowDC(myBitmap);

            BitBlt(myMemdc, 0, 0, rect.x2 - rect.x1, rect.y2 - rect.y1, hmemdc, rect.x1, rect.y1, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            //SelectObject(myMemdc, myBitmap);

            Bitmap bmp = Bitmap.FromHbitmap(myBitmap);
            DeleteDC(hscrdc);
            DeleteDC(hmemdc);
            DeleteDC(myMemdc);
            return bmp;
        }

        /// <summary>
        /// 指定窗口截图 保存为图片文件
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }

        /// <summary>
        /// 全屏截图 保存为文件
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureScreenToFile(string filename,ImageFormat format)
        {
            Image img = CaptureScreen();
            img.Save(filename, format);

        }

        /// <summary>
        /// 设置 RECT 的左下右上
        /// </summary>
        /// <param name="rect">给出要设定的 RECT</param>
        /// <param name="left">左边</param>
        /// <param name="bottom">下边</param>
        /// <param name="right">右边</param>
        /// <param name="top">上边</param>
        public void SetRECT(ref RECT rect, int x1, int y1, int x2, int y2)
        {
            rect.x1 = x1;
            rect.y1 = y1;
            rect.x2 = x2;
            rect.y2 = y2;

        }

        /// <summary>
        /// 合并图片
        /// </summary>
        /// <param name="bmp1">图片1</param>
        /// <param name="bmp2">图片2</param>
        public Bitmap HBpic(Bitmap bmp1, Bitmap bmp2)
        {
            Bitmap newBmp = new Bitmap(bmp1.Width, bmp1.Height + bmp2.Height);
            var g = Graphics.FromImage(newBmp);
            g.DrawImage(bmp1, 0, 0);
            g.DrawImage(bmp2, 0, bmp1.Height);

            return newBmp;
        }

    }

        //调用DLL类
        public class msdk_DLL_Ifx
        {
            [DllImport("msdk.dll", EntryPoint = "M_Open", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr M_Open(int port);
            [DllImport("msdk.dll", EntryPoint = "M_Open_VidPid", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr M_Open_VidPid(int Vid, int Pid);

            [DllImport("msdk.dll", EntryPoint = "M_Close", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_Close(IntPtr m_hdl);

            //获取设备序列号
            [DllImport("msdk.dll", EntryPoint = "M_GetDevSn", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_GetDevSn(IntPtr m_hdl, ref int dwp_LenResponse, ref byte ucp_Response);

            //写入用户数据
            [DllImport("msdk.dll", EntryPoint = "M_SetUserData", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_SetUserData(IntPtr m_hdl, int dw_LenUserData, ref byte ucp_UserData);
            //验证用户数据
            [DllImport("msdk.dll", EntryPoint = "M_VerifyUserData", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_VerifyUserData(IntPtr m_hdl, int dw_LenUserData, ref byte ucp_UserData);

            //DLL内部参数恢复默认值
            [DllImport("msdk.dll", EntryPoint = "M_InitParam", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_InitParam(IntPtr m_hdl);
            //设置DLL内部参数
            [DllImport("msdk.dll", EntryPoint = "M_SetParam", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_SetParam(IntPtr m_hdl, int ParamType, int Param1, int Param2);

            /***********键盘操作函数;以下函数中的m_hdl是指M_Open返回的句柄*******/
            /***********以下所有命令返回 0: 成功；-1: 失败*******/
            //单击(按下后立刻弹起)按键  //HidKeyCode: 键盘码; Nbr: 按下次数
            [DllImport("msdk.dll", EntryPoint = "M_KeyPress", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_KeyPress(IntPtr m_hdl, int HidKeyCode, int Nbr);
            //按下某个按键不弹起        //HidKeyCode: 键盘码
            [DllImport("msdk.dll", EntryPoint = "M_KeyDown", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_KeyDown(IntPtr m_hdl, int HidKeyCode);
            //弹起某个按键              //HidKeyCode: 键盘码
            [DllImport("msdk.dll", EntryPoint = "M_KeyUp", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_KeyUp(IntPtr m_hdl, int HidKeyCode);
            //读取按键状态              //HidKeyCode: 键盘码 //返回 0: 弹起状态；1:按下状态；-1: 失败(端口未打开)
            //使用该接口，不允许手工操作键盘，否则该接口返回值有可能不正确
            [DllImport("msdk.dll", EntryPoint = "M_KeyState", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_KeyState(IntPtr m_hdl, int HidKeyCode);

            //单击(按下后立刻弹起)按键  //KeyCode: 键盘码; Nbr: 按下次数
            [DllImport("msdk.dll", EntryPoint = "M_KeyPress2", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_KeyPress2(IntPtr m_hdl, int KeyCode, int Nbr);
            //按下某个按键不弹起        //KeyCode: 键盘码
            [DllImport("msdk.dll", EntryPoint = "M_KeyDown2", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_KeyDown2(IntPtr m_hdl, int KeyCode);
            //弹起某个按键              //KeyCode: 键盘码
            [DllImport("msdk.dll", EntryPoint = "M_KeyUp2", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_KeyUp2(IntPtr m_hdl, int KeyCode);
            //读取按键状态              //KeyCode: 键盘码 //返回 0: 弹起状态；1:按下状态；-1: 失败(端口未打开)
            //使用该接口，不允许手工操作键盘，否则该接口返回值有可能不正确
            [DllImport("msdk.dll", EntryPoint = "M_KeyState2", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_KeyState2(IntPtr m_hdl, int KeyCode);

            //弹起所有按键
            [DllImport("msdk.dll", EntryPoint = "M_ReleaseAllKey", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_ReleaseAllKey(IntPtr m_hdl);

            //读取小键盘NumLock灯的状态 //返回 0:灭；1:亮；-1: 失败
            [DllImport("msdk.dll", EntryPoint = "M_NumLockLedState", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_NumLockLedState(IntPtr m_hdl);
            //读取CapsLock灯的状态 //返回 0:灭；1:亮；-1: 失败
            [DllImport("msdk.dll", EntryPoint = "M_CapsLockLedState", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_CapsLockLedState(IntPtr m_hdl);
            //读取ScrollLock灯的状态 //返回 0:灭；1:亮；-1: 失败
            [DllImport("msdk.dll", EntryPoint = "M_ScrollLockLedState", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_ScrollLockLedState(IntPtr m_hdl);

            //输入一串ASCII字符串，如"ABCdef012,.<>"，在InputLen个字节内将忽略非ASCII字符，  //InputStr: 输入缓冲区首地址; 注
            [DllImport("msdk.dll", EntryPoint = "M_KeyInputString", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_KeyInputString(IntPtr m_hdl, ref byte InputStr, int InputLen);

            //输入一串字符串，支持中文(GBK编码)英文混合，如"啊啊啊ABCdef012,.<>"，在InputLen个字节内将忽略非ASCII和中文字符，  //InputStr: 输入缓冲区首地址; 注意：不支持解析\n\r等转义字符！ InputLen:输出的长度
            [DllImport("msdk.dll", EntryPoint = "M_KeyInputStringGBK", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_KeyInputStringGBK(IntPtr m_hdl, ref byte InputStr, int InputLen);

            //输入一串字符串，支持中文(Unicode编码)英文混合，如"啊啊啊ABCdef012,.<>"，在InputLen个字节内将忽略非ASCII和中文字符，  //InputStr: 输入缓冲区首地址; 注意：不支持解析\n\r等转义字符！ InputLen:输出的长度
            [DllImport("msdk.dll", EntryPoint = "M_KeyInputStringUnicode", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_KeyInputStringUnicode(IntPtr m_hdl, ref byte InputStr, int InputLen);

            /***********鼠标操作函数;以下函数中的m_hdl是指M_Open返回的句柄*******/
            /***********以下所有命令返回 0: 成功；-1: 失败*******/
            //左键单击   //Nbr: 左键在当前位置单击次数 
            [DllImport("msdk.dll", EntryPoint = "M_LeftClick", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_LeftClick(IntPtr m_hdl, int Nbr);
            //左键双击   //Nbr: 左键在当前位置双击次数
            [DllImport("msdk.dll", EntryPoint = "M_LeftDoubleClick", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_LeftDoubleClick(IntPtr m_hdl, int Nbr);
            //按下左键不弹起
            [DllImport("msdk.dll", EntryPoint = "M_LeftDown", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_LeftDown(IntPtr m_hdl);
            //弹起左键
            [DllImport("msdk.dll", EntryPoint = "M_LeftUp", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_LeftUp(IntPtr m_hdl);
            //右键单击   //Nbr: 左键在当前位置单击次数
            [DllImport("msdk.dll", EntryPoint = "M_RightClick", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_RightClick(IntPtr m_hdl, int Nbr);
            //按下右键不弹起
            [DllImport("msdk.dll", EntryPoint = "M_RightDown", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_RightDown(IntPtr m_hdl);
            //弹起右键
            [DllImport("msdk.dll", EntryPoint = "M_RightUp", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_RightUp(IntPtr m_hdl);
            //中键单击   //Nbr: 左键在当前位置单击次数
            [DllImport("msdk.dll", EntryPoint = "M_MiddleClick", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_MiddleClick(IntPtr m_hdl, int Nbr);
            //按下中键不弹起
            [DllImport("msdk.dll", EntryPoint = "M_MiddleDown", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_MiddleDown(IntPtr m_hdl);
            //弹起中键
            [DllImport("msdk.dll", EntryPoint = "M_MiddleUp", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_MiddleUp(IntPtr m_hdl);
            //弹起鼠标的所有按键
            [DllImport("msdk.dll", EntryPoint = "M_ReleaseAllMouse", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_ReleaseAllMouse(IntPtr m_hdl);
            //读取鼠标左中右键状态  //MouseKeyCode: 1=左键 2=右键 3=中键  //返回 0: 弹起状态；1:按下状态；-1: 失败
            //只能读取盒子中鼠标的状态，读取不到实体鼠标的状态
            [DllImport("msdk.dll", EntryPoint = "M_MouseKeyState", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_MouseKeyState(IntPtr m_hdl, int MouseKeyCode);

            //滚动鼠标滚轮;  Nbr: 滚动量,  为正,向上滚动；为负, 向下滚动;
            [DllImport("msdk.dll", EntryPoint = "M_MouseWheel", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_MouseWheel(IntPtr m_hdl, int Nbr);
            //将鼠标移动到原点(0,0)  在出现移动出现异常时，可以用该函数将鼠标复位
            [DllImport("msdk.dll", EntryPoint = "M_ResetMousePos", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_ResetMousePos(IntPtr m_hdl);
            //从当前位置移动鼠标    x: x方向（横轴）的距离（正:向右; 负值:向左）; y: y方向（纵轴）的距离（正:向下; 负值:向上）
            [DllImport("msdk.dll", EntryPoint = "M_MoveR", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_MoveR(IntPtr M_hdl, int x, int y);
            //移动鼠标到指定坐标    x: x方向（横轴）的坐标; y: y方向（纵轴）的坐标。坐标原点(0, 0)在屏幕左上角
            //注意：如果出现过将鼠标移动的距离超过屏幕大小，再次MoveTo可能会出现无法正确移动到指定坐标的问题，如果出现该问题，需调用ResetMousePos复位
            [DllImport("msdk.dll", EntryPoint = "M_MoveTo", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_MoveTo(IntPtr m_hdl, int x, int y);
            //读取当前鼠标所在坐标  返回坐标在x、y中。 
            //注意：该函数必须在执行一次MoveTo或ResetMousePos函数后才能正确执行！
            //注意：如果曾经出现过将鼠标移动的距离超过屏幕大小，这里读取到的坐标值有可能是不正确的！如果出现该问题，需调用ResetMousePos复位
            [DllImport("msdk.dll", EntryPoint = "M_GetCurrMousePos", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_GetCurrMousePos(IntPtr m_hdl, ref int x, ref int y);

            //以下接口仅适用主控机和被控机是同一台电脑的使用方式(单头模块；双头模块的两个USB头都连接到同一台电脑)
            //以下接口将调用系统的API来获取当前鼠标位置，DLL将不记录鼠标移动的位置
            //移动鼠标到指定坐标    x: x方向（横轴）的坐标; y: y方向（纵轴）的坐标。
            [DllImport("msdk.dll", EntryPoint = "M_MoveR2", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_MoveR2(IntPtr m_hdl, int x, int y);
            //移动鼠标到指定坐标    x: x方向（横轴）的坐标; y: y方向（纵轴）的坐标。坐标原点(0, 0)在屏幕左上角
            [DllImport("msdk.dll", EntryPoint = "M_MoveTo2", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_MoveTo2(IntPtr m_hdl, int x, int y);
            //读取当前鼠标所在坐标  返回坐标在x、y中。
            [DllImport("msdk.dll", EntryPoint = "M_GetCurrMousePos2", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_GetCurrMousePos2(ref int x, ref int y);

            //以下接口将使用绝对移动功能。该功能目前还不能支持安卓
            //在使用绝对移动功能前，必须先输入被控机的屏幕分辨率
            //x: x方向（横轴）的坐标; y: y方向（纵轴）的坐标。坐标原点(0, 0)在屏幕左上角
            //返回值如果是-10，表示该盒子不支持绝对移动功能。返回0表示执行正确。可以用该接口判断盒子是否支持绝对移动功能
            [DllImport("msdk.dll", EntryPoint = "M_ResolutionUsed", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_ResolutionUsed(IntPtr m_hdl, int x, int y);
            //将鼠标移动到指定坐标。绝对移动功能，鼠标移动到指定位置时，在某些坐标上最大会有±2的误差
            //使用该接口后，可以调用M_GetCurrMousePos读取鼠标位置
            [DllImport("msdk.dll", EntryPoint = "M_MoveTo3", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_MoveTo3(IntPtr m_hdl, int x, int y);
            /*******************通用操作函数****************************/
            //延时指定时间  time:单位ms
            [DllImport("msdk.dll", EntryPoint = "M_Delay", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_Delay(int time);
            //在指定的最小最大值之间延时随机时间  Min_time:最小延时时间; Max_time: 最大延时时间 （单位：ms）
            [DllImport("msdk.dll", EntryPoint = "M_DelayRandom", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_DelayRandom(int Min_time, int Max_time);
            //在最小最大值之间取随机数
            [DllImport("msdk.dll", EntryPoint = "M_RandDomNbr", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int M_RandDomNbr(int Min_V, int Max_V);
        }

        // C# bitmap变量转为 halcon变量
        public static HObject Bitmap2HObject(Bitmap bmp)
        {
            try
            {
 
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
 
                System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                IntPtr pointer = bmpData.Scan0;
 
                byte[] dataBlue = new byte[bmp.Width * bmp.Height];
                unsafe
                {
                    fixed (byte* ptrdata = dataBlue)
                    {
                        for (int i = 0; i < bmp.Height; i++)
                        {
                            CopyMemory((int)(ptrdata + bmp.Width * i), (int)(pointer + bmpData.Stride * i), bmp.Width);
                        }
                        HObject ho;
                        HOperatorSet.GenImage1(out ho, "byte", bmp.Width, bmp.Height, (int)ptrdata);
                        HImage himg = new HImage("byte", bmp.Width, bmp.Height, (IntPtr)ptrdata);
 
                        //HOperatorSet.DispImage(ho, hWindowControl1.HalconWindow);
 
                        bmp.UnlockBits(bmpData);
                        return ho;
                    }
                }
            }
            catch (Exception exc)
            {
                return null;
            }
 
        }


        //创建对象-截图操作
        ScreenCapture Screen_Capture;
        //创建对象-创建双头
        msdk_DLL_Ifx MSDK_DLL;
        //窗口句柄
        public IntPtr M_Handle;

        public void Bitmap2HObjectBpp24(Bitmap bmp, out HObject image)
        {
            try
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
 
                BitmapData srcBmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                HOperatorSet.GenImageInterleaved(out image, srcBmpData.Scan0, "bgr", bmp.Width, bmp.Height, 0, "byte", 0, 0, 0, 0, -1, 0);
                bmp.UnlockBits(srcBmpData);
 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                image = null;
            }
        }

        public void disp_message(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem,
    HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_GenParamName = new HTuple(), hv_GenParamValue = new HTuple();
            HTuple hv_Color_COPY_INP_TMP = new HTuple(hv_Color);
            HTuple hv_Column_COPY_INP_TMP = new HTuple(hv_Column);
            HTuple hv_CoordSystem_COPY_INP_TMP = new HTuple(hv_CoordSystem);
            HTuple hv_Row_COPY_INP_TMP = new HTuple(hv_Row);

            // Initialize local and output iconic variables 
            try
            {
                //This procedure displays text in a graphics window.
                //
                //Input parameters:
                //WindowHandle: The WindowHandle of the graphics window, where
                //   the message should be displayed
                //String: A tuple of strings containing the text message to be displayed
                //CoordSystem: If set to 'window', the text position is given
                //   with respect to the window coordinate system.
                //   If set to 'image', image coordinates are used.
                //   (This may be useful in zoomed images.)
                //Row: The row coordinate of the desired text position
                //   A tuple of values is allowed to display text at different
                //   positions.
                //Column: The column coordinate of the desired text position
                //   A tuple of values is allowed to display text at different
                //   positions.
                //Color: defines the color of the text as string.
                //   If set to [], '' or 'auto' the currently set color is used.
                //   If a tuple of strings is passed, the colors are used cyclically...
                //   - if |Row| == |Column| == 1: for each new textline
                //   = else for each text position.
                //Box: If Box[0] is set to 'true', the text is written within an orange box.
                //     If set to' false', no box is displayed.
                //     If set to a color string (e.g. 'white', '#FF00CC', etc.),
                //       the text is written in a box of that color.
                //     An optional second value for Box (Box[1]) controls if a shadow is displayed:
                //       'true' -> display a shadow in a default color
                //       'false' -> display no shadow
                //       otherwise -> use given string as color string for the shadow color
                //
                //It is possible to display multiple text strings in a single call.
                //In this case, some restrictions apply:
                //- Multiple text positions can be defined by specifying a tuple
                //  with multiple Row and/or Column coordinates, i.e.:
                //  - |Row| == n, |Column| == n
                //  - |Row| == n, |Column| == 1
                //  - |Row| == 1, |Column| == n
                //- If |Row| == |Column| == 1,
                //  each element of String is display in a new textline.
                //- If multiple positions or specified, the number of Strings
                //  must match the number of positions, i.e.:
                //  - Either |String| == n (each string is displayed at the
                //                          corresponding position),
                //  - or     |String| == 1 (The string is displayed n times).
                //
                //
                //Convert the parameters for disp_text.
                if ((int)((new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                    new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(new HTuple())))) != 0)
                {

                    hv_Color_COPY_INP_TMP.Dispose();
                    hv_Column_COPY_INP_TMP.Dispose();
                    hv_CoordSystem_COPY_INP_TMP.Dispose();
                    hv_Row_COPY_INP_TMP.Dispose();
                    hv_GenParamName.Dispose();
                    hv_GenParamValue.Dispose();

                    return;
                }
                if ((int)(new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(-1))) != 0)
                {
                    hv_Row_COPY_INP_TMP.Dispose();
                    hv_Row_COPY_INP_TMP = 12;
                }
                if ((int)(new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(-1))) != 0)
                {
                    hv_Column_COPY_INP_TMP.Dispose();
                    hv_Column_COPY_INP_TMP = 12;
                }
                //
                //Convert the parameter Box to generic parameters.
                hv_GenParamName.Dispose();
                hv_GenParamName = new HTuple();
                hv_GenParamValue.Dispose();
                hv_GenParamValue = new HTuple();
                if ((int)(new HTuple((new HTuple(hv_Box.TupleLength())).TupleGreater(0))) != 0)
                {
                    if ((int)(new HTuple(((hv_Box.TupleSelect(0))).TupleEqual("false"))) != 0)
                    {
                        //Display no box
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_GenParamName = hv_GenParamName.TupleConcat(
                                    "box");
                                hv_GenParamName.Dispose();
                                hv_GenParamName = ExpTmpLocalVar_GenParamName;
                            }
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_GenParamValue = hv_GenParamValue.TupleConcat(
                                    "false");
                                hv_GenParamValue.Dispose();
                                hv_GenParamValue = ExpTmpLocalVar_GenParamValue;
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_Box.TupleSelect(0))).TupleNotEqual(
                        "true"))) != 0)
                    {
                        //Set a color other than the default.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_GenParamName = hv_GenParamName.TupleConcat(
                                    "box_color");
                                hv_GenParamName.Dispose();
                                hv_GenParamName = ExpTmpLocalVar_GenParamName;
                            }
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_GenParamValue = hv_GenParamValue.TupleConcat(
                                    hv_Box.TupleSelect(0));
                                hv_GenParamValue.Dispose();
                                hv_GenParamValue = ExpTmpLocalVar_GenParamValue;
                            }
                        }
                    }
                }
                if ((int)(new HTuple((new HTuple(hv_Box.TupleLength())).TupleGreater(1))) != 0)
                {
                    if ((int)(new HTuple(((hv_Box.TupleSelect(1))).TupleEqual("false"))) != 0)
                    {
                        //Display no shadow.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_GenParamName = hv_GenParamName.TupleConcat(
                                    "shadow");
                                hv_GenParamName.Dispose();
                                hv_GenParamName = ExpTmpLocalVar_GenParamName;
                            }
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_GenParamValue = hv_GenParamValue.TupleConcat(
                                    "false");
                                hv_GenParamValue.Dispose();
                                hv_GenParamValue = ExpTmpLocalVar_GenParamValue;
                            }
                        }
                    }
                    else if ((int)(new HTuple(((hv_Box.TupleSelect(1))).TupleNotEqual(
                        "true"))) != 0)
                    {
                        //Set a shadow color other than the default.
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_GenParamName = hv_GenParamName.TupleConcat(
                                    "shadow_color");
                                hv_GenParamName.Dispose();
                                hv_GenParamName = ExpTmpLocalVar_GenParamName;
                            }
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            {
                                HTuple
                                  ExpTmpLocalVar_GenParamValue = hv_GenParamValue.TupleConcat(
                                    hv_Box.TupleSelect(1));
                                hv_GenParamValue.Dispose();
                                hv_GenParamValue = ExpTmpLocalVar_GenParamValue;
                            }
                        }
                    }
                }
                //Restore default CoordSystem behavior.
                if ((int)(new HTuple(hv_CoordSystem_COPY_INP_TMP.TupleNotEqual("window"))) != 0)
                {
                    hv_CoordSystem_COPY_INP_TMP.Dispose();
                    hv_CoordSystem_COPY_INP_TMP = "image";
                }
                //
                if ((int)(new HTuple(hv_Color_COPY_INP_TMP.TupleEqual(""))) != 0)
                {
                    //disp_text does not accept an empty string for Color.
                    hv_Color_COPY_INP_TMP.Dispose();
                    hv_Color_COPY_INP_TMP = new HTuple();
                }
                //
                HOperatorSet.DispText(hv_WindowHandle, hv_String, hv_CoordSystem_COPY_INP_TMP,
                    hv_Row_COPY_INP_TMP, hv_Column_COPY_INP_TMP, hv_Color_COPY_INP_TMP, hv_GenParamName,
                    hv_GenParamValue);

                hv_Color_COPY_INP_TMP.Dispose();
                hv_Column_COPY_INP_TMP.Dispose();
                hv_CoordSystem_COPY_INP_TMP.Dispose();
                hv_Row_COPY_INP_TMP.Dispose();
                hv_GenParamName.Dispose();
                hv_GenParamValue.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {

                hv_Color_COPY_INP_TMP.Dispose();
                hv_Column_COPY_INP_TMP.Dispose();
                hv_CoordSystem_COPY_INP_TMP.Dispose();
                hv_Row_COPY_INP_TMP.Dispose();
                hv_GenParamName.Dispose();
                hv_GenParamValue.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Screen_Capture = new ScreenCapture();
        }

        private void button1_Click(object sender, EventArgs e)
        {



            //保存图像
            //Screen_Capture.CaptureScreenToFile("F:\\TEST.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
           // Screen_Capture.CaptureScreenToFile(00190056, "F:\\TEST.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            //IntPtr hwnd = FindWindow("Notepad", null);
            //IntPtr hwnd = new IntPtr("001A0A72")
            //IntPtr hwnd = Marshal.StringToHGlobalAnsi("001A0A72");
            //IntPtr hwnd = FindWindow(null, "无标题 - 记事本");
            //Screen_Capture.CaptureWindowToFile(hwnd, "F:\\TEST.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

        }

        //转化显示
        private void button2_Click(object sender, EventArgs e)
        {
            HTuple hv_Width = new HTuple(), hv_Height = new HTuple();
            HObject Halcon_Image;
            HTuple hv_Seconds1 = new HTuple(), hv_Seconds2 = new HTuple(),hv_Time = new HTuple();


            HOperatorSet.GenEmptyObj(out Halcon_Image);
            //计算时间-开
            HOperatorSet.CountSeconds(out hv_Seconds1);
            //截图图片
            Bitmap Bit_Map = Screen_Capture.CaptureScreen();
            //保存截图

            //转换成Halcon图像
            //Halcon_Image = Bitmap2HObject(Bit_Map);
            Bitmap2HObjectBpp24(Bit_Map, out Halcon_Image);
            //获取图像大小
            HOperatorSet.GetImageSize(Halcon_Image, out hv_Width, out hv_Height);
            //设置窗口填充
            HOperatorSet.SetPart(this.hWindowControl1.HalconWindow, 0, 0, hv_Height, hv_Width);
            //获取结束时间
            HOperatorSet.CountSeconds(out hv_Seconds2);
            //显示图像
            HOperatorSet.DispObj(Halcon_Image, this.hWindowControl1.HalconWindow);
            //显示耗时
            hv_Time = (hv_Seconds2 - hv_Seconds1) * 1000;
            disp_message(this.hWindowControl1.HalconWindow, "耗时(MS):" + hv_Time, "window", 12, 12, "black", "true");
            //保存图像
            HOperatorSet.WriteImage(Halcon_Image, "bmp", 0, "D:/Halcon_Image/TEST_1");
            //释放图像
            Halcon_Image.Dispose();
            hv_Width.Dispose();
            hv_Height.Dispose();
            hv_Seconds1.Dispose();
            hv_Seconds2.Dispose();
            hv_Time.Dispose();
        }

        long  M_Handle_Number;

        //打开端口
        private void button3_Click(object sender, EventArgs e)
        {
            M_Handle = msdk_DLL_Ifx.M_Open(1);
            M_Handle_Number = M_Handle.ToInt64();
            if (M_Handle.ToInt64() == -1)
            {
                MessageBox.Show("打开设备失败，请检查USB设备是否已经插入");
            }

            //设置窗口分辨率
            //msdk_DLL_Ifx.M_ResolutionUsed(M_Handle, 1920, 1080);
            msdk_DLL_Ifx.M_ResolutionUsed(M_Handle, 1440, 900);

            //
            this.button3.Enabled = false;
            this.button4.Enabled = true;
        }



        //关闭端口
        private void button4_Click(object sender, EventArgs e)
        {
            //关闭端口
            int  Close = msdk_DLL_Ifx.M_Close(M_Handle);
            if (Close == 0)
            {
                Console.WriteLine("关闭端口成功！");
            }
            else
            {
                Console.WriteLine("关闭端口失败！");
            }
            this.button3.Enabled = true;
            this.button4.Enabled = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            msdk_DLL_Ifx.M_MoveTo3(M_Handle, 100, 100);
        }
        //获取鼠标坐标
        private void button6_Click(object sender, EventArgs e)
        {
            int x, y;
            x = 0;
            y=0;
            msdk_DLL_Ifx.M_GetCurrMousePos2(ref x,ref y);
            MessageBox.Show(x.ToString() + "," + y.ToString());


        }


    }
}
