using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace UIAutoScriptGen
{
    class HierarchyActions
    {
        AutomationElement _element;

        #region Static functions.
        public static List<Hashtable> Tree(AutomationElement _element)
        {
            List<Hashtable> ParentDetails = new List<Hashtable>();
            TreeWalker walker = TreeWalker.ContentViewWalker;
            AutomationElement _parent = walker.GetParent(_element);

            Hashtable CurrElemHash = new Hashtable();
            CurrElemHash.Add("Name", _element.Current.Name);
            CurrElemHash.Add("CtrlType", _element.Current.ControlType.ProgrammaticName);
            CurrElemHash.Add("AutoID", _element.Current.AutomationId);

            ParentDetails.Add(CurrElemHash);

            while (!(_parent == null) && _parent != AutomationElement.RootElement)
            {
                CurrElemHash = new Hashtable();
                CurrElemHash["Name"] = _parent.Current.Name;
                CurrElemHash["CtrlType"] = _parent.Current.ControlType.ProgrammaticName;
                CurrElemHash["AutoID"] = _parent.Current.AutomationId;

                ParentDetails.Add(CurrElemHash);
                
                _parent = walker.GetParent(_parent);
            }
            ParentDetails.Reverse();

            return ParentDetails;
        }
        #endregion
    }
}
