using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;

namespace UIAutoScriptGen
{
    class UIControl
    {
        #region Class Variables
        public string WindowName { get; set; }
        public string WinElemName { get; set; }
        public AutomationElement _Window { get; set; } = null;
        public AutomationElement _WinElem { get; set; } = null;
        public bool Value { get; set; } = false;
        public WindowInteractionState _WinState { get; private set; }

        #endregion End Class Variables

        #region Constructor
        public UIControl(string WinName, string ElemAutoID, int Tries)
        {
            WindowName = WinName;
            WinElemName = ElemAutoID;
            GetUIWindow(WinName, Tries);
            if (_Window != null)
            {
                GetWinElem(_Window, ElemAutoID, Tries);
                if (_WinElem == null)
                {
                    GetWinElemByName(_Window, ElemAutoID, Tries);
                }
            }
            if (_WinElem != null)
            {
                Value = true;
                _WinState = GetWindowPattern(_Window).Current.WindowInteractionState;
            }
        }
        #endregion

        #region Private Functions
        private void GetUIWindow(string WinName, int Tries)
        {
            int i = 0;
            AutomationElement _ReturnElem = null;
            do
            {
                _ReturnElem = AutomationElement.RootElement.FindFirst
                (TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, WinName));
                ++i;
                Thread.Sleep(200);
            } while (i < Tries && _ReturnElem == null);
            _Window = _ReturnElem;
        }

        private void GetWinElem(AutomationElement Win, string ElemID, int Tries)
        {
            int i = 0;
            AutomationElement _ReturnElem = null;
            do
            {
                _ReturnElem = Win.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, ElemID));
                ++i;
                Thread.Sleep(200);
            } while (i < Tries && _ReturnElem == null);
            _WinElem = _ReturnElem;
        }

        private void GetWinElemByName(AutomationElement Win, string ElemName, int Tries)
        {
            int i = 0;
            AutomationElement _ReturnElem = null;
            do
            {
                _ReturnElem = Win.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, ElemName));
                ++i;
                Thread.Sleep(200);
            } while (i < Tries && _ReturnElem == null);
            _WinElem = _ReturnElem;
        }

        [DllImport("user32")]
        static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        #endregion

        #region Pattern Definitions
        private static InvokePattern GetInvokePattern(AutomationElement element)
        {
            return element.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
        }

        private static ValuePattern GetValuePattern(AutomationElement element)
        {
            return element.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
        }

        private static TogglePattern GetTogglePattern(AutomationElement element)
        {
            return element.GetCurrentPattern(TogglePattern.Pattern) as TogglePattern;
        }

        private static ScrollPattern GetScrollPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(ScrollPattern.Pattern) as ScrollPattern;
        }

        private static TextPattern GetTextPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(TextPattern.Pattern) as TextPattern;
        }

        private static SynchronizedInputPattern GetSynchronizedInputPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(SynchronizedInputPattern.Pattern) as SynchronizedInputPattern;
        }

        private static SelectionPattern GetSelectionPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(SelectionPattern.Pattern) as SelectionPattern;
        }

        private static SelectionItemPattern GetSelectionItemPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
        }

        private static ExpandCollapsePattern GetExpandCollapsePattern(AutomationElement element)
        {
            return element.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
        }

        private static WindowPattern GetWindowPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(WindowPattern.Pattern) as WindowPattern;
        }
        #endregion

        #region Public Static Functions
        public static void MouseClick(string button)
        {
            switch (button)
            {
                case "left":
                    mouse_event((uint)MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
                    mouse_event((uint)MouseEventFlags.LEFTUP, 0, 0, 0, 0);
                    break;
                case "right":
                    mouse_event((uint)MouseEventFlags.RIGHTDOWN, 0, 0, 0, 0);
                    mouse_event((uint)MouseEventFlags.RIGHTUP, 0, 0, 0, 0);
                    break;
                case "middle":
                    mouse_event((uint)MouseEventFlags.MIDDLEDOWN, 0, 0, 0, 0);
                    mouse_event((uint)MouseEventFlags.MIDDLEUP, 0, 0, 0, 0);
                    break;
            }
        }

        public static void WaitElemAppear(string WinName, string WinElemID, int Tries)
        {
            int i = 0;
            AutomationElement _Win;
            AutomationElement _WinElem;
            do
            {
                _Win = AutomationElement.RootElement.FindFirst
                (TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, WinName));
                ++i;
                Thread.Sleep(200);
            } while (i < Tries && _Win == null);
            
            if (_Win != null)
            {
                i = 0;
                do
                {
                    _WinElem = _Win.FindFirst(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.AutomationIdProperty, WinElemID));
                    ++i;
                    Thread.Sleep(200);
                } while (i < Tries && _WinElem == null);

                i = 0;
                while(_WinElem == null && i < Tries)
                {
                    _WinElem = _Win.FindFirst(TreeScope.Descendants,
                        new PropertyCondition(AutomationElement.NameProperty, WinElemID));
                    ++i;
                    Thread.Sleep(200);
                }
            }
        }

        public static void SendKeys(string Text)
        {
            System.Windows.Forms.SendKeys.SendWait(Text);
        }

        public static void WaitElemDisappear(String WinName, String WinElem, int Tries)
        {
            AutomationElement _Win = null;
            AutomationElement _ReturnWinElem = null;

            while (true)
            {
                int i = 0;
                do
                {
                    _Win = AutomationElement.RootElement.FindFirst
                    (TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, WinName));
                    ++i;
                    Thread.Sleep(200);
                } while (i < Tries && _Win == null);

                if (_Win != null)
                {
                    do
                    {
                        _ReturnWinElem = AutomationElement.RootElement.FindFirst
                        (TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, WinElem));
                        ++i;
                        Thread.Sleep(200);
                    } while (i < Tries && _Win == null);
                }

                if (_ReturnWinElem == null)
                {
                    break;
                }
            }
        }
        #endregion

        #region Public Functions (Non-Static)
        public string Type()
        {
            StringBuilder str = new StringBuilder();
            if (Value == true)
            {
                foreach (AutomationPattern pattern in _WinElem.GetSupportedPatterns())
                {
                    str.AppendLine(pattern.ProgrammaticName);
                }
            }
            return str.ToString();
        }

        public void Invoke()
        {
            if (_WinElem != null) { GetInvokePattern(_WinElem).Invoke(); }
        }

        public void Toggle()
        {
            if (_WinElem != null) { GetTogglePattern(_WinElem).Toggle(); }
        }

        public void SyncInput()
        {
            if (_WinElem != null) { GetSynchronizedInputPattern(_WinElem).StartListening(SynchronizedInputType.MouseLeftButtonDown); }
        }

        public void ValueSet(string Text)
        {
            if (_WinElem != null) { GetValuePattern(_WinElem).SetValue(Text); }
        }

        public void ExpCol()
        {
            ExpandCollapsePattern ExpColPat = GetExpandCollapsePattern(_WinElem);
            if (_WinElem != null)
            {
                if (ExpColPat.Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                {
                    ExpColPat.Expand();
                }
                else if (ExpColPat.Current.ExpandCollapseState == ExpandCollapseState.Expanded)
                {
                    ExpColPat.Collapse();
                }
            }
        }

        public void Win()
        {
            if (_Window != null)
            {
                WindowPattern WinPattern = GetWindowPattern(_Window);
                if (WinPattern.Current.WindowVisualState == WindowVisualState.Normal)
                {
                    WinPattern.SetWindowVisualState(WindowVisualState.Maximized);
                }
                else if (WinPattern.Current.WindowVisualState == WindowVisualState.Maximized)
                {
                    WinPattern.SetWindowVisualState(WindowVisualState.Minimized);
                }
                else if (WinPattern.Current.WindowVisualState == WindowVisualState.Minimized)
                {
                    WinPattern.SetWindowVisualState(WindowVisualState.Normal);
                }
            }
        }

        public MousePoint ClickablePoint()
        {
            System.Windows.Rect ElemRect = _WinElem.Current.BoundingRectangle;
            int X_Center = (int)(ElemRect.X + ElemRect.Width / 2);
            int Y_Center = (int)(ElemRect.Y + ElemRect.Height / 2);
            MousePoint point = new MousePoint(X_Center, Y_Center);
            return point;
        }

        public void MouseToElem()
        {
            try
            {
                MousePoint point = ClickablePoint();
                SetCursorPos(point.Xp, point.Yp);
            }
            catch
            {
                throw new System.Exception("NoClickablePoint");
            }
        }

        public void ElemClick()
        {
            try
            {
                MouseToElem();
                MouseClick("left");
            }
            catch(Exception) { Debug.WriteLine(string.Format("Could not find clickable point. \"{0}\" \"{1}\"", WindowName, WinElemName), "NoClickablePoint"); }
        }

        public void SendKeys(string Text, bool TryClickElement)
        {
            if (TryClickElement) { ElemClick(); }
            System.Windows.Forms.SendKeys.SendWait(Text);
        }
        #endregion

        #region Public Functions (Static)
        public static Hashtable GetCurrentFocusedDetails()
        {
            AutomationElement _ReturnElement = null;
            _ReturnElement = AutomationElement.FocusedElement;
            Hashtable _ReturnTable = new Hashtable();
            _ReturnTable.Add("Name", _ReturnElement.Current.Name);
            _ReturnTable.Add("AutoID", _ReturnElement.Current.AutomationId);
            _ReturnTable.Add("Class", _ReturnElement.Current.ClassName);
            _ReturnTable.Add("ElemType", _ReturnElement.Current.ControlType.ProgrammaticName);
            _ReturnTable.Add("ProcID", _ReturnElement.Current.ProcessId);
            _ReturnTable.Add("ItemType", _ReturnElement.Current.ItemType);
            _ReturnTable.Add("ParentName", Process.GetProcessById(_ReturnElement.Current.ProcessId).MainWindowTitle);
            return _ReturnTable;

            StringBuilder RetString = new StringBuilder();
            RetString.AppendLine(String.Format("Name: {0}", _ReturnElement.Current.Name));
            RetString.AppendLine(String.Format("AutoID: {0}", _ReturnElement.Current.AutomationId));
            RetString.AppendLine(String.Format("Class : {0}", _ReturnElement.Current.ClassName));
            RetString.AppendLine(String.Format("ElemType : {0}", _ReturnElement.Current.ControlType.ProgrammaticName));
            RetString.AppendLine(String.Format("ProcID : {0}", _ReturnElement.Current.ProcessId));
            RetString.AppendLine(String.Format("ItemType : {0}", _ReturnElement.Current.ItemType));
            RetString.AppendLine(String.Format("ParentName : {0}", Process.GetProcessById(_ReturnElement.Current.ProcessId).MainWindowTitle));
            MessageBox.Show(RetString.ToString());
        }

        public static Hashtable GetMouseOverElemDetails()
        {
            System.Drawing.Point CurPos = System.Windows.Forms.Cursor.Position;
            Point pt = new Point(CurPos.X, CurPos.Y);
            AutomationElement _ReturnElement = null;
            _ReturnElement = AutomationElement.FromPoint(pt);
            Hashtable _ReturnTable = new Hashtable();
            _ReturnTable.Add("Name", _ReturnElement.Current.Name);
            _ReturnTable.Add("AutoID", _ReturnElement.Current.AutomationId);
            _ReturnTable.Add("Class", _ReturnElement.Current.ClassName);
            _ReturnTable.Add("ElemType", _ReturnElement.Current.ControlType.ProgrammaticName);
            _ReturnTable.Add("ProcID", _ReturnElement.Current.ProcessId);
            _ReturnTable.Add("ItemType", _ReturnElement.Current.ItemType);
            _ReturnTable.Add("ParentName", Process.GetProcessById(_ReturnElement.Current.ProcessId).MainWindowTitle);
            _ReturnTable.Add("Element", _ReturnElement);

            return _ReturnTable;


            StringBuilder RetString = new StringBuilder();
            RetString.AppendLine(String.Format("Name: {0}", _ReturnElement.Current.Name));
            RetString.AppendLine(String.Format("AutoID: {0}", _ReturnElement.Current.AutomationId));
            RetString.AppendLine(String.Format("Class : {0}", _ReturnElement.Current.ClassName));
            RetString.AppendLine(String.Format("ElemType : {0}", _ReturnElement.Current.ControlType.ProgrammaticName));
            RetString.AppendLine(String.Format("ProcID : {0}", _ReturnElement.Current.ProcessId));
            RetString.AppendLine(String.Format("ItemType : {0}", _ReturnElement.Current.ItemType));
            RetString.AppendLine(String.Format("ParentName : {0}", Process.GetProcessById(_ReturnElement.Current.ProcessId).MainWindowTitle));
            MessageBox.Show(RetString.ToString());
        }

        public static AutomationElement GetCurrentPointedElement()
        {
            System.Drawing.Point CurPos = System.Windows.Forms.Cursor.Position;
            Point pt = new Point(CurPos.X, CurPos.Y);
            AutomationElement _ReturnElement = null;
            _ReturnElement = AutomationElement.FromPoint(pt);
            return _ReturnElement;
        }

        #endregion
    }
}
