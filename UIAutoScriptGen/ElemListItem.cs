using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIAutoScriptGen
{
    [Serializable]
    public class ElemListItem
    {
        public string Action { get; set; }
        public string ElemXMLTree { get; set; }
        public string ElemName { get; set; }
        public string ElemClass { get; set; }
        public string ElemAutoID { get; set; }
        public string WinName { get; set; }
        public string Data { get; set; }        

        public ElemListItem(string Act, string ElName, string ElClass, string ElAutoID, string WName, string Dat, string ElemXML)
        {
            Action = Act;
            ElemName = ElName;
            ElemClass = ElClass;
            ElemAutoID = ElAutoID;
            WinName = WName;
            Data = Dat;
            ElemXMLTree = ElemXML;
        }

        public ElemListItem(string SelectedAction, Hashtable ElemDetails, string Dat)
        {
            Action = SelectedAction;
            ElemName = ElemDetails["Name"].ToString();
            ElemClass = ElemDetails["Class"].ToString();
            ElemAutoID = ElemDetails["AutoID"].ToString();
            WinName = ElemDetails["ParentName"].ToString();
            Data = Dat;
            ElemXMLTree = ElemDetails["ElemXML"].ToString();
        }

        public ElemListItem() { }
    }
}
