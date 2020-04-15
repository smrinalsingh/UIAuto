using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;

namespace UIAutoScriptGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string openFileName;
        private string OpenFileName
        {
            get
            {
                return openFileName;
            }
            set
            {
                openFileName = value;
                SetWinName();
            }
        }
        PatternWin patternWin = new PatternWin();
        DataWindow dataWindow = new DataWindow();

        public MainWindow()
        {
            InitializeComponent();
            //PatternWin.PreviewMouseDown += stkItemClicked;
        }

        private void SetWinName()
        {
            if (OpenFileName != null && OpenFileName != "")
            {
                Title = "Automation Script Generator | " + OpenFileName;
            }

            else
            {
                Title = "Automation Script Generator";
            }
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
            RegisterHotKey(Handle, 2, (uint)KeyboardKeys.Ctrl, (uint)KeyboardKeys.F9);
            RegisterHotKey(Handle, 3, (uint)KeyboardKeys.Ctrl, (uint)KeyboardKeys.F8);
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

        private void MainWinKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.S &&
                System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                SaveScript();
                MessageBox.Show("Saved.");
            }
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
            //throw new NotImplementedException();
            var PointedElem = UIControl.GetCurrentPointedElement();
            var ElemXML = TypeConverter.AutoElemToXMLElem(PointedElem);
            string XMLString = TypeConverter.BeautifyXMLDoc(ElemXML);

            MessageBox.Show(XMLString);
        }

        private void OnF9Pressed()
        {
            Hashtable ElemDetails = UIControl.GetMouseOverElemDetails();
            StringBuilder Details = new StringBuilder();

            foreach (string key in ElemDetails.Keys)
            {
                Details.AppendLine(key + ": " + ElemDetails[key]);
            }
            MessageBox.Show(Details.ToString());
        }

        private void OnF10Pressed()
        {
            Hashtable _ElemHash = UIControl.GetMouseOverElemDetails();

            //Clear existing items in both Controls' and SubControls' stackpanels. 
            patternWin.stkControlTypes.Children.Clear();
            patternWin.stkSubControlTypes.Children.Clear();
            AutomationPattern[] SupportedPatterns = ((AutomationElement)_ElemHash["Element"]).GetSupportedPatterns();

            //Add buttons to SupportPattern stackpanel.
            foreach (AutomationPattern Pattern in SupportedPatterns)
            {
                Button but = new Button()
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.DarkBlue,
                };
                but.Content = Pattern.ProgrammaticName;

                //This event sends Element Details (Hashtable) and the Button itself to Handler.
                but.Click += delegate (object sender, RoutedEventArgs e)
                {
                    stkItemClicked(sender, e, _ElemHash, ((AutomationElement)_ElemHash["Element"]));
                };

                //Adds the button to ControlType (SupportedPattern) stackpanel.
                patternWin.stkControlTypes.Children.Add(but);
            }

            //For other common element based actions.
            Button CommonButton = new Button()
            {
                Background = Brushes.White,
                BorderBrush = Brushes.DarkBlue,
            };
            CommonButton.Content = "Common";
            CommonButton.Click += delegate (object sender, RoutedEventArgs e)
            {
                stkItemClicked(sender, e, _ElemHash, ((AutomationElement)_ElemHash["Element"]));
            };
            patternWin.stkControlTypes.Children.Add(CommonButton);

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
        private void stkItemClicked(object sender, RoutedEventArgs e, Hashtable _ElemHash, AutomationElement _Elem)
        {
            patternWin.stkSubControlTypes.Children.Clear();
            string StkClickedButton = e.Source.ToString().Split(':')[1].Trim();

            Hashtable ControlsNData = new Hashtable();
            string[] NeededData;
            if (StkClickedButton == "InvokePatternIdentifiers.Pattern")
            {
                NeededData = new string[] { };
                ControlsNData.Add("InvokeClick", NeededData);
            }
            else if (StkClickedButton == "ExpandCollapsePatternIdentifiers.Pattern")
            {
                NeededData = new string[] { };
                ControlsNData.Add("Expand", NeededData);
                ControlsNData.Add("Collapse", NeededData);
            }
            else if (StkClickedButton == "WindowPatternIdentifiers.Pattern")
            {
                NeededData = new string[] { };
                ControlsNData.Add("WinMaxState", NeededData);
                ControlsNData.Add("WinMinState", NeededData);
                ControlsNData.Add("WinNormalState", NeededData);
                ControlsNData.Add("Close", NeededData);
            }
            else if (StkClickedButton == "ScrollPatternIdentifiers.Pattern")
            {
                NeededData = new string[] { "horizontalAmount", "verticalAmount" };
                ControlsNData.Add("Scroll", NeededData);

                NeededData = new string[] { "horizontalAmount" };
                ControlsNData.Add("ScrollHorizontal", NeededData);

                NeededData = new string[] { "verticalAmount" };
                ControlsNData.Add("ScrollVertical", NeededData);

                NeededData = new string[] { "horizontalPercent", "verticalPercent" };
                ControlsNData.Add("SetScrollPercent", NeededData);
            }
            else if (StkClickedButton == "ScrollItemPatternIdentifiers.Pattern")
            {
                NeededData = new string[] { };
                ControlsNData.Add("ScrollToView", NeededData);
            }
            else if (StkClickedButton == "ValuePatternIdentifiers.Pattern")
            {
                NeededData = new string[] { "Text" };
                ControlsNData.Add("SetElemText", NeededData);
            }
            else if (StkClickedButton == "TextPatternIdentifiers.Pattern")
            {
                NeededData = new string[] { };
                ControlsNData.Add("GetSelectedText", NeededData);
            }
            else if (StkClickedButton == "TogglePatternIdentifiers.Pattern")
            {
                NeededData = new string[] { };
                ControlsNData.Add("Toggle", NeededData);
            }
            else if (StkClickedButton == "Common")
            {
                NeededData = new string[] { "MouseButton" };
                ControlsNData.Add("ElementClick", NeededData);
                ControlsNData.Add("MouseToElement", NeededData);

                NeededData = new string[] { "Timeout" };
                ControlsNData.Add("WaitDisappear", NeededData);
                ControlsNData.Add("WaitAppear", NeededData);

                NeededData = new string[] { "Text", "ToClick" };
                ControlsNData.Add("UseKeyboard", NeededData);
            }
            else
            {
                NeededData = new string[] { "Undefined" };
                ControlsNData.Add(StkClickedButton, NeededData);
            }

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
            foreach (string SubControl in ControlsNData.Keys)
            {
                Button btnSubControl = new Button()
                {
                    Content = SubControl,
                };
                btnSubControl.Click += delegate (object senderr, RoutedEventArgs ee)
                {
                    BtnSubControl_Click(senderr, ee, _ElemHash, btnSubControl, (string[])ControlsNData[SubControl]);
                };
                patternWin.stkSubControlTypes.Children.Add(btnSubControl);
            }
        }

        private void BtnSubControl_Click(object sender, RoutedEventArgs e, Hashtable _ElemHash, Button Btn, string[] SubMenuActions)
        {
            //MessageBox.Show(Btn.Content.ToString());
            //If the data needed for selected action is not null, show the window and ask for
            //needed details.
            if (!(SubMenuActions.Length == 0))
            {
                dataWindow.stkDataLabel.Children.Clear(); dataWindow.stkDataText.Children.Clear();
                LayControls(dataWindow.stkDataLabel, dataWindow.stkDataText, SubMenuActions, Btn, _ElemHash);

                dataWindow.Title = Btn.Content.ToString();
                dataWindow.Show();
            }
            //Else, do not show the DataWindow and proceed with adding the data.
            else
            {
                ElemListItem ToAdd = new ElemListItem(Btn.Content.ToString(), _ElemHash, null);
                Command.RunItem(ToAdd, 10);
                lstElemList.Items.Add(ToAdd);
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
        private void LayControls(StackPanel labelStack, StackPanel textboxStack,
            string[] SubMenuActions, Button Btn, Hashtable _ElemHash)
        {
            foreach (string action in SubMenuActions)
            {
                Label lbl = new Label()
                {
                    Content = action,
                };

                TextBox txtbx = new TextBox()
                {
                    Height = lbl.Height + 10,
                    Width = lbl.Width,
                };

                labelStack.Children.Add(lbl);
                textboxStack.Children.Add(txtbx);
            }
            buttToDataWin = Btn;
            _elHashToDataWin = _ElemHash;

            //RemoveClickEvent(dataWindow.btnAddData);
            dataWindow.btnAddData.Click += BtnAddData_Click;
        }

        private Button buttToDataWin = null;
        private Hashtable _elHashToDataWin = null;

        private void BtnAddData_Click(object sender, RoutedEventArgs e)
        {
            List<string> Data = new List<string>();
            UIElementCollection stkTextBoxes = dataWindow.stkDataText.Children;
            foreach (UIElement stkTextBox in stkTextBoxes)
            {
                Data.Add(((TextBox)stkTextBox).Text);
            }
            string Datum = string.Join(",", Data);

            ElemListItem ToAdd = new ElemListItem(buttToDataWin.Content.ToString(), _elHashToDataWin, Datum);
            //MessageBox.Show(buttToDataWin.Content.ToString());

            Command.RunItem(ToAdd, 10);
            lstElemList.Items.Add(ToAdd);
            dataWindow.Hide();

            //Handling and setting needed data back to normal
            e.Handled = true;
            buttToDataWin = null;
            _elHashToDataWin = null;
        }

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
            SaveScript();
        }

        private void btnLoadScript_Click(object sender, RoutedEventArgs e)
        {
            if (OpenFileName == "" || OpenFileName == null)
            {
                LoadFile();
            }

            else
            {
                MessageBoxResult Choice = MessageBox.Show("Save file?", "Confirmation", MessageBoxButton.YesNoCancel);
                if (Choice == MessageBoxResult.Yes)
                {
                    SaveScript();
                    lstElemList.Items.Clear();
                    OpenFileName = null;
                }

                else if (Choice == MessageBoxResult.No)
                {
                    lstElemList.Items.Clear();
                    OpenFileName = null;
                    LoadFile();
                }

                else if (Choice == MessageBoxResult.Cancel || Choice == MessageBoxResult.None) { }
            }
        }

        private void LoadFile()
        {
            OpenFileDialog FilePath = new OpenFileDialog()
            {
                Filter = "UIAuto File|*.uia",
                Title = "Open Generated Script",
                DefaultExt = "uia",
            };
            FilePath.ShowDialog();
            OpenFileName = FilePath.FileName;

            if (OpenFileName != null && OpenFileName != "")
            {
                try
                {
                    Stream stream = new FileStream(OpenFileName, FileMode.Open, FileAccess.Read);
                    IFormatter formatter = new BinaryFormatter();
                    List<ElemListItem> items = (List<ElemListItem>)formatter.Deserialize(stream);
                    foreach (ElemListItem item in items) { lstElemList.Items.Add(item); }
                    stream.Close();
                }
                catch (Exception ee)
                {
                    MessageBox.Show("Unable to open file.\nError: " + ee.Message);
                }
            }

            else
            {
                OpenFileName = null;
            }
        }

        private void SaveScript()
        {
            List<ElemListItem> ScriptLines = new List<ElemListItem>();
            ScriptLines.AddRange(lstElemList.Items.OfType<ElemListItem>());
            //ElemListItem one = (ElemListItem)ScriptLines[0];
            IFormatter formatter = new BinaryFormatter();

            if (OpenFileName == "" || OpenFileName == null)
            {
                SaveFileDialog SaveName = new SaveFileDialog()
                {
                    Filter = "UIAuto File|*.uia",
                    Title = "Save Generated Script",
                    DefaultExt = "uia",
                };
                SaveName.ShowDialog();

                OpenFileName = SaveName.FileName;

                Stream stream = new FileStream(OpenFileName, FileMode.Create, FileAccess.Write);
                formatter.Serialize(stream, ScriptLines);
                stream.Close();
            }

            else if (OpenFileName != "")
            {
                try
                {
                    Stream stream = new FileStream(OpenFileName, FileMode.Create, FileAccess.Write);
                    formatter.Serialize(stream, ScriptLines);
                    stream.Close();
                }

                catch (Exception ee)
                {
                    Debug.Write(ee.Message);
                    MessageBox.Show("File not saved!\nError: " + ee.Message);
                }
            }
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            List<ElemListItem> ScriptLines = new List<ElemListItem>();
            ScriptLines.AddRange(lstElemList.Items.OfType<ElemListItem>());

            foreach (ElemListItem line in ScriptLines)
            {
                lstElemList.SelectedItem = line;
                Command.RunItem(line, 10);
                Task.Factory.StartNew(() =>
                {
                });
            }
        }

        private void btnRunSelected_Click(object sender, RoutedEventArgs e)
        {
            List<ElemListItem> SelectedLines = new List<ElemListItem>();
            SelectedLines.AddRange(lstElemList.SelectedItems.OfType<ElemListItem>());
            foreach (ElemListItem line in SelectedLines)
                Command.RunItem(line, 10);
        }

        private void lstElemList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;
            Hashtable ElemHash = new Hashtable();

            while (obj != null && obj != lstElemList)
            {
                if (obj.GetType() == typeof(ListViewItem))
                {
                    ElemListItem Selected = (ElemListItem)lstElemList.SelectedItem;
                    PropertyInfo[] properties = Selected.GetType()
                                                        .GetProperties();
                    foreach (PropertyInfo prop in properties)
                        ElemHash.Add(prop.Name, prop.GetValue(Selected));
                }
                obj = VisualTreeHelper.GetParent(obj);
            }

            MessageBox.Show(TypeConverter.HashToString(ElemHash));
        }
    }  
}
