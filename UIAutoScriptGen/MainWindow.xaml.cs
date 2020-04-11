using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace UIAutoScriptGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PatternWin patternWin = new PatternWin();
        DataWindow dataWindow = new DataWindow();

        public MainWindow()
        {
            InitializeComponent();
            //PatternWin.PreviewMouseDown += stkItemClicked;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            //PatternWin Related Initialization
            patternWin.Closing += PatternWin_Closing;
            dataWindow.Closing += DataWin_Closing;

            //Set instructions.
            txtbInstructions.Text = "*Use Ctrl + F10 hotkey to open supported patterns' window.\n" +
                "*Select the pattern from supported patterns' list and then select the control.\n" +
                "*If the functionality needs additional data, a window would popup asking for the same.";

            //Registering Hotkeys On Source Init
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            IntPtr Handle = new WindowInteropHelper(this).Handle;
            RegisterHotKey(Handle, 1, (uint)KeyboardKeys.Ctrl, (uint)KeyboardKeys.F10);
        }

        private void PatternWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            patternWin.Hide();
        }

        private void DataWin_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            dataWindow.Hide();
        }

        #region Hotkey stuff

        #region core stuff
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;

        private void UnregisterHotKey(int HOTKEY_ID)
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        //Switch as per the event ID mentioned earlier.
                        case 1:
                            OnF10Pressed();
                            handled = true;
                            break;

                        case 2:
                            OnF9Pressed();
                            break;

                        case 3:
                            OnF8Pressed();
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        #endregion
        private void OnF8Pressed()
        {
            throw new NotImplementedException();
        }

        private void OnF9Pressed()
        {
            throw new NotImplementedException();
        }

        private void OnF10Pressed()
        {
            Hashtable _ElemHash = UIControl.GetMouseOverElemDetails();
            AutomationElement _Elem = UIControl.GetCurrentPointedElement();

            //Clear existing items in both Controls' and SubControls' stackpanels. 
            patternWin.stkControlTypes.Children.Clear();
            patternWin.stkSubControlTypes.Children.Clear();
            var SupportedPatterns = ((AutomationElement)_ElemHash["Element"]).GetSupportedPatterns();

            //Add buttons to SupportPattern stackpanel.
            foreach (var Pattern in SupportedPatterns)
            {
                Button but = new Button()
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.DarkBlue,
                };
                but.Content = Pattern.ProgrammaticName;

                //This event sends Element Details (Hashtable) and the Button itself to Handler.
                but.Click += delegate (object sender, RoutedEventArgs e) { stkItemClicked(sender, e, _ElemHash, but, _Elem); };

                //Adds the button to ControlType (SupportedPattern) stackpanel.
                patternWin.stkControlTypes.Children.Add(but);
            }
            if (!patternWin.IsVisible) { patternWin.ShowDialog(); }
        }
        #endregion

        /// <summary>
        /// Event handler for adding buttons for subcontrols once a SupportedPattern button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="_Elem"></param>
        /// <param name="but"></param>
        private void stkItemClicked(object sender, RoutedEventArgs e, Hashtable _ElemHash, Button but, AutomationElement _Elem)
        {
            patternWin.stkSubControlTypes.Children.Clear();
            string StkClickedButton = e.Source.ToString().Split(':')[1].Trim();

            Hashtable ControlsNData = new Hashtable();
            string[] NeededData = null;
            List<string> SubControls = new List<string>();
            if (StkClickedButton == "InvokePatternIdentifiers.Pattern") 
            {
                NeededData = new string[] { };
                ControlsNData.Add("InvokeClick", NeededData);
                SubControls.Add("InvokeClick"); 
            }
            else if (StkClickedButton == "ExpandCollapsePatternIdentifiers.Pattern") 
            {
                NeededData = new string[] { };
                ControlsNData.Add("Expand", NeededData);
                SubControls.Add("Expand");
                ControlsNData.Add("Collapse", NeededData);
                SubControls.Add("Collapse"); 
            }
            else if (StkClickedButton == "WindowPatternIdentifiers.Pattern") 
            {
                NeededData = new string[] { };
                SubControls.Add("WinMaxState");
                ControlsNData.Add("WinMaxState", NeededData);
                SubControls.Add("WinMinState");
                ControlsNData.Add("WinMinState", NeededData);
                SubControls.Add("WinNormalState");
                ControlsNData.Add("WinNormalState", NeededData);
                SubControls.Add("Close");
                ControlsNData.Add("Close", NeededData);
            }
            else if (StkClickedButton == "ScrollPatternIdentifiers.Pattern")
            {
                NeededData = new string[] { "horizontalAmount", "verticalAmount" };
                SubControls.Add("Scroll");
                ControlsNData.Add("Scroll", NeededData);

                NeededData = new string[] { "horizontalAmount" };
                SubControls.Add("ScrollHorizontal");
                ControlsNData.Add("ScrollHorizontal", NeededData);

                NeededData = new string[] { "verticalAmount" };
                SubControls.Add("ScrollVertical");
                ControlsNData.Add("ScrollVertical", NeededData);

                NeededData = new string[] { "horizontalPercent", "verticalPercent" };
                SubControls.Add("SetScrollPercent");
                ControlsNData.Add("SetScrollPercent", NeededData);
            }

            else if (StkClickedButton == "ScrollItemPatternIdentifiers.Pattern")
            {
                NeededData = new string[] { };
                SubControls.Add("ScrollToView");
                ControlsNData.Add("ScrollToView", NeededData);
            }

            else if (StkClickedButton == "ValuePatternIdentifiers.Pattern")
            {
                NeededData = new string[] { "Text" };
                SubControls.Add("SetElemText");
                ControlsNData.Add("SetElemText", NeededData);
            }

            else if (StkClickedButton == "TextPatternIdentifiers.Pattern")
            {
                NeededData = new string[] { };
                SubControls.Add("GetSelectedText");
                ControlsNData.Add("GetSelectedText", NeededData);
            }

            else if (StkClickedButton == "TogglePatternIdentifiers.Pattern")
            {
                NeededData = new string[] { };
                SubControls.Add("Toggle");
                ControlsNData.Add("Toggle", NeededData);
            }

            else 
            {
                NeededData = new string[] { "Undefined" };
                SubControls.Add("Undefined");
                ControlsNData.Add(StkClickedButton, NeededData);
            }

            //AddSubMenus(SubControls, _Elem);
            AddSubMenus(ControlsNData, _ElemHash, _Elem);

        }


        /// <summary>
        /// This function creates each button, adds the button's functionality and adds the button to the
        /// SubControl stackpanel.
        /// </summary>
        /// <param name="ControlsNData"></param>
        /// <param name="_Elem"></param>
        private void AddSubMenus(Hashtable ControlsNData, Hashtable _ElemHash, AutomationElement _Elem)
        {
            patternWin.stkSubControlTypes.Children.Clear();
            foreach (var SubControl in ControlsNData.Keys)
            {
                Button btnSubControl = new Button()
                {
                    Content = SubControl,
                    Background = Brushes.White,
                    BorderBrush = Brushes.DarkBlue,
                };
                btnSubControl.Click += delegate (object senderr, RoutedEventArgs ee) { BtnSubControl_Click(senderr, ee, _ElemHash, btnSubControl, (string[])ControlsNData[SubControl.ToString()], _Elem); };
                patternWin.stkSubControlTypes.Children.Add(btnSubControl);
            }
        }

        private void BtnSubControl_Click(object sender, RoutedEventArgs e, Hashtable _ElemHash, Button Btn, string[] SubMenuActions, AutomationElement _Elem)
        {
            //lstElemList.Items.Add(new ElemListItem(Btn.Content.ToString(), _Elem));
                        
            //If the data needed for selected action is not null, show the window and ask for
            //needed details.
            if (!(SubMenuActions.Length == 0))
            {
                dataWindow.stkDataLabel.Children.Clear(); dataWindow.stkDataText.Children.Clear();                
                LayControls(dataWindow.stkDataLabel, dataWindow.stkDataText, 22, 125, SubMenuActions, Btn, _ElemHash, _Elem);

                dataWindow.Title = Btn.Content.ToString();
                dataWindow.Show();
            }
            //Else, do not show the DataWindow and proceed with adding the data.
            else
            {
                lstElemList.Items.Add(new ElemListItem(Btn.Content.ToString(), _ElemHash, null, _Elem));
            }
            patternWin.Hide();
        }

        /// <summary>
        /// This function lays out controls created at Runtime onto the given window. The requirement 
        /// would be the window and the control is pre-defined as of now. A label and textbox for each.
        /// </summary>
        /// <param name="labelStack"></param>
        /// <param name="textboxStack"></param>
        /// <param name="Height"></param>
        /// <param name="Width"></param>
        /// <param name="SubMenuActions"></param>
        /// <param name="Btn"></param>
        private void LayControls(StackPanel labelStack, StackPanel textboxStack, int Height, int Width, 
            string[] SubMenuActions, Button Btn, Hashtable _ElemHash, AutomationElement _Elem)
        {
            
            foreach (string action in SubMenuActions)
            {
                Label lbl = new Label()
                {
                    Content = action,
                };

                TextBox txtbx = new TextBox()
                {
                    Name = action,
                    Height = lbl.Height+10,
                    Width = lbl.Width,
                };

                labelStack.Children.Add(lbl);
                textboxStack.Children.Add(txtbx);
            }

            dataWindow.btnAddData.Click += delegate (object sender, RoutedEventArgs e) { BtnAddData_Click(sender, e, Btn, _ElemHash, _Elem); };
        }

        private void BtnAddData_Click(object sender, RoutedEventArgs e, Button Btn, Hashtable _ElemHash, AutomationElement _Elem)
        {
            List<string> Data = new List<string>();
            UIElementCollection stkTextBoxes = dataWindow.stkDataText.Children;
            foreach (UIElement stkTextBox in stkTextBoxes)
            {
                Data.Add(((TextBox)stkTextBox).Text);
            }
            string Datum = string.Join(",", Data);

            lstElemList.Items.Add(new ElemListItem(Btn.Content.ToString(), _ElemHash, Datum, _Elem));
            e.Handled = true;
        }

        #region Unused.
        /*
        //The two functions below are not used as of now since we also need DataWindow.
        /// <summary>
        /// This function creates each button, adds the button to the SubControl stackpanel.
        /// </summary>
        /// <param name="SubControls"></param>
        /// <param name="_Elem"></param>
        private void AddSubMenus(List<string> SubControls, Hashtable _Elem)
        {
            patternWin.stkSubControlTypes.Children.Clear();
            foreach (var SubControl in SubControls)
            {
                Button btnSubControl = new Button()
                {
                    Content = SubControl,
                    Background = Brushes.White,
                    BorderBrush = Brushes.DarkBlue,
                };
                btnSubControl.Click += delegate (object senderr, RoutedEventArgs ee) { BtnSubControl_Click(senderr, ee, _Elem, btnSubControl); };
                patternWin.stkSubControlTypes.Children.Add(btnSubControl);
            }
        }

        private void BtnSubControl_Click(object sender, RoutedEventArgs e, Hashtable _Elem, Button Btn)
        {
            lstElemList.Items.Add(new ElemListItem(Btn.Content.ToString(), _Elem, ""));
            patternWin.Hide();
        }
        //The two functions above are not used as of now since we also need DataWindow.
        */
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            //Unregister hotkey.
            base.OnClosed(e);
            UnregisterHotKey(1);
            
            //Remove override of closing method to hide.
            patternWin.Closing -= PatternWin_Closing;
            dataWindow.Closing -= DataWin_Closing;

            //close
            patternWin.Close();
            dataWindow.Close();
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            lstElemList.Items.Clear();
        }

        private void btnClearSelected_Click(object sender, RoutedEventArgs e)
        {
            while (lstElemList.SelectedIndex > 0)
            {
                lstElemList.Items.RemoveAt(lstElemList.SelectedIndex);
            }
        }

        private void btnSaveScript_Click(object sender, RoutedEventArgs e)
        {
            IList ScriptLines = lstElemList.Items;
            ElemListItem one = (ElemListItem)ScriptLines[0];
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("C:\\Users\\Admn\\Documents\\TestSerial.txt", FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, one);
            stream.Close();

            stream = new FileStream("C:\\Users\\Admn\\Documents\\TestSerial.txt", FileMode.Open, FileAccess.Read);
            ElemListItem testDeSer = (ElemListItem)(formatter.Deserialize(stream));

            MessageBox.Show(testDeSer.Elem.Current.AutomationId);
        }
    }

    [Serializable]
    public class ElemListItem
    {
        public string Action { get; set; }
        public string ElemName { get; set; }
        public string ElemClass { get; set; }
        public string ElemAutoID { get; set; }
        public string WinName { get; set; }
        public string Data { get; set; }
        public AutomationElement Elem { get; set; }
        public ElemListItem(string Act, string ElName, string ElClass, string ElAutoID, string WName, string Dat)
        {
            Action = Act;
            ElemName = ElName;
            ElemClass = ElClass;
            ElemAutoID = ElAutoID;
            WinName = WName;
            Data = Dat;
        }

        public ElemListItem(string SelectedAction, Hashtable ElemDetails, string Dat, AutomationElement Element)
        {
            Action = SelectedAction;
            ElemName = ElemDetails["Name"].ToString();
            ElemClass = ElemDetails["Class"].ToString();
            ElemAutoID = ElemDetails["AutoID"].ToString();
            WinName = ElemDetails["ParentName"].ToString();
            Data = Dat;
            Elem = Element;
        }
    }
}
