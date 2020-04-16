using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;

namespace UIAutoScriptGen
{
    class Command
    {
        string IAction;
        string IName;
        string IClass;
        string IAutoID;
        string IWinName;
        string IData;

        public Command(ElemListItem listItem)
        {
            IAction = listItem.Action;
            IName = listItem.ElemName;
            IClass = listItem.ElemClass;
            IAutoID = listItem.ElemAutoID;
            IWinName = listItem.WinName;
            IData = listItem.Data;
        }

        public static void RunItem(ElemListItem listItem, int TimeOut)
        {
            string IAction = listItem.Action;
            string IName = listItem.ElemName;
            string IClass = listItem.ElemClass;
            string IAutoID = listItem.ElemAutoID;
            string IWinName = listItem.WinName;
            string IData = listItem.Data;
            string Ixml = listItem.ElemXMLTree;

            //UIControl UI = new UIControl(IWinName, IName, IClass, IAutoID, TimeOut);
            UIControl UI = new UIControl(Ixml);
            var Element = UI._WinElem;
            if (Element != null)
            {
                switch (IAction)
                {
                    case "InvokeClick":
                        GetPattern.GetInvokePattern(Element).Invoke();
                        break;

                    case "Expand":
                        GetPattern.GetExpandCollapsePattern(Element).Expand();
                        break;

                    case "Collapse":
                        GetPattern.GetExpandCollapsePattern(Element).Collapse();
                        break;

                    case "WinMaxState":
                        GetPattern.GetWindowPattern(Element).SetWindowVisualState(WindowVisualState.Maximized);
                        break;

                    case "WinMinState":
                        GetPattern.GetWindowPattern(Element).SetWindowVisualState(WindowVisualState.Minimized);
                        break;

                    case "WinNormalState":
                        GetPattern.GetWindowPattern(Element).SetWindowVisualState(WindowVisualState.Normal);
                        break;

                    case "Scroll":
                        string[] XnY = IData.Split(',');
                        ScrollAmount X = (ScrollAmount)Enum.Parse(typeof(ScrollAmount), XnY[0]);
                        ScrollAmount Y = (ScrollAmount)Enum.Parse(typeof(ScrollAmount), XnY[1]);
                        GetPattern.GetScrollPattern(Element).Scroll(X, Y);
                        break;

                    case "ScrollHorizontal":
                        Y = (ScrollAmount)Enum.Parse(typeof(ScrollAmount), IData);
                        GetPattern.GetScrollPattern(Element).ScrollHorizontal(Y);
                        break;

                    case "ScrollVertical":
                        X = (ScrollAmount)Enum.Parse(typeof(ScrollAmount), IData);
                        GetPattern.GetScrollPattern(Element).ScrollVertical(X);
                        break;

                    case "SetScrollPercent":
                        double horPer = double.Parse(IData.Split(',')[0]);
                        double verPer = double.Parse(IData.Split(',')[1]);
                        GetPattern.GetScrollPattern(Element).SetScrollPercent(horPer, verPer);
                        break;

                    case "ScrollToView":
                        GetPattern.GetScrollItemPattern(Element).ScrollIntoView();
                        break;

                    case "SetElemText":
                        GetPattern.GetValuePattern(Element).SetValue(IData);
                        break;

                    case "GetSelectedText":
                        throw new NotImplementedException();

                    case "Toggle":
                        GetPattern.GetTogglePattern(Element).Toggle();
                        break;

                    case "ElementClick":
                        UIControl.ElemClick(Element, IData);
                        break;

                    case "MouseToElement":
                        UIControl.MouseToElem(Element);
                        break;

                    //Wait disappear and appear to be implemented.

                    case "UseKeyboard":
                        string Text = IData.Split(',')[0];
                        string ToClick = IData.Split(',')[1].ToLower();
                        bool Click;

                        if (ToClick == "yes" || ToClick == "true")
                            UIControl.SendKeys(Element, Text, true);
                        else if (ToClick == "no" || ToClick == "false")
                            UIControl.SendKeys(Element, Text, false);
                        else
                            Console.WriteLine($"Invalid value. Data given {IData}");
                        break;
                }
            }

            else
                Console.WriteLine($"Element not found.\nName: {IName}\n" +
                    $"Class: {IClass}\nAutoID: {IAutoID}\nWindow: {IWinName}");
        }
    }
}
