using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Xml;

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
                _WinState = GetPattern.GetWindowPattern(_Window).Current.WindowInteractionState;
            }
        }

        public UIControl(string WinName, string ElemName, string ElemClass, string ElemAutoID, int TimeOut)
        {
            //Timeout in seconds.
            GetUIWindow(WinName, TimeOut);
            if (_Window != null)
            {
                GetWinElem(_Window, ElemName, ElemClass, ElemAutoID);
            }
        }

        public UIControl(XmlDocument XMLDoc)
        {
            _WinElem = GetAutoElemFromXML(XMLDoc);
        }

        public UIControl(string XMLStr)
        {
            XmlDocument XMLDoc = new XmlDocument();
            XMLDoc.LoadXml(XMLStr);
            _WinElem = GetAutoElemFromXML(XMLDoc);
        }

        public static AutomationElement GetAutoElemFromXML(XmlDocument XML)
        {
            AutomationElement _ReturnElement = null;
            //As programmed, each XMLDoc contains multiple nodes with attributes
            //containing the element specific details. 

            //Lets start with getting all the nodes.
            XmlNodeList xmlNodeList = XML.ChildNodes;
            
            foreach(XmlNode xmlNode in xmlNodeList)
            {
                XmlAttributeCollection NodeAttrs = xmlNode.Attributes;
                AndCondition AllConditions = new AndCondition(new PropertyCondition(AutomationElement.NameProperty, NodeAttrs["Name"].Value),
                    new PropertyCondition(AutomationElement.AutomationIdProperty, NodeAttrs["AutoID"].Value),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, NodeAttrs["CtrlType"].Value),
                    new PropertyCondition(AutomationElement.ClassNameProperty, NodeAttrs["Class"].Value));

                _ReturnElement = AutomationElement.RootElement.FindFirst(TreeScope.Children,
                    AllConditions);
            }

            return _ReturnElement;
        }

        //Both the constructors above dependant on XML have similar process
        //except for the loading from string to XMLDocument needed for one.

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
                Thread.Sleep(1000);
            } while (i < Tries && _ReturnElem == null);
            _Window = _ReturnElem;
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

        private void GetWinElem(AutomationElement Win, string ElemName, string ElemClass, string ElemAutoID)
        {
            System.Windows.Automation.Condition cElem = new AndCondition(
                new PropertyCondition(AutomationElement.NameProperty, ElemName),
                new PropertyCondition(AutomationElement.ClassNameProperty, ElemClass),
                new PropertyCondition(AutomationElement.AutomationIdProperty, ElemAutoID));
            AutomationElement element = Win.FindFirst(TreeScope.Descendants, cElem);

            _WinElem = element;
        }

        [DllImport("user32")]
        static extern int SetCursorPos(int x, int y);

        [DllImport("user32.dll")]

        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);
        #endregion

        public MousePoint ClickablePoint()
        {
            Rect ElemRect = _WinElem.Current.BoundingRectangle;
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
        

        #region Public Functions (Static)
        public static MousePoint ClickablePoint(AutomationElement element)
        {
            Rect ElemRect = element.Current.BoundingRectangle;
            int X_Center = (int)(ElemRect.X + ElemRect.Width / 2);
            int Y_Center = (int)(ElemRect.Y + ElemRect.Height / 2);
            MousePoint point = new MousePoint(X_Center, Y_Center);
            return point;
        }

        public static void MouseToElem(AutomationElement element)
        {
            try
            {
                MousePoint point = ClickablePoint(element);
                SetCursorPos(point.Xp, point.Yp);
            }
            catch
            {
                throw new Exception("NoClickablePoint");
            }
        }

        public static void ElemClick(AutomationElement element, string mouseButton)
        {
            MouseToElem(element);
            MouseClick(mouseButton);
        }

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
                while (_WinElem == null && i < Tries)
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

        public static void SendKeys(AutomationElement element, string Text, bool TryClickElement)
        {
            if (TryClickElement) { ElemClick(element, "left"); }
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

        public static Hashtable GetCurrentFocusedDetails()
        {
            AutomationElement _ReturnElement = null;
            _ReturnElement = AutomationElement.FocusedElement;
            Hashtable _ReturnTable = TypeConverter.AutoElemToHash(_ReturnElement);
            return _ReturnTable;
        }

        public static Hashtable GetMouseOverElemDetails()
        {
            System.Drawing.Point CurPos = System.Windows.Forms.Cursor.Position;
            Point pt = new Point(CurPos.X, CurPos.Y);
            AutomationElement _ReturnElement = AutomationElement.FromPoint(pt);
            Hashtable _ReturnTable = TypeConverter.AutoElemToHash(_ReturnElement);
            return _ReturnTable;
        }

        public static AutomationElement GetCurrentPointedElement()
        {
            System.Drawing.Point CurPos = System.Windows.Forms.Cursor.Position;
            Point pt = new Point(CurPos.X, CurPos.Y);
            AutomationElement _ReturnElement = AutomationElement.FromPoint(pt);
            return _ReturnElement;
        }

        public static AutomationElement GetTopLevelWindow(AutomationElement element)
        {
            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement elementParent;
            AutomationElement node = element;
            if (node == AutomationElement.RootElement) return node;
            do
            {
                elementParent = walker.GetParent(node);
                if (elementParent == AutomationElement.RootElement) break;
                node = elementParent;
            }
            while (true);
            return node;
        }

        #endregion
    }
}
