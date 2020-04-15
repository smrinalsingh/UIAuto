using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
using System.Xml;

namespace UIAutoScriptGen
{
    public class TypeConverter
    {
        #region Static functions.
        //Converts all the keys and values to items in each line.
        public static string HashToString(Hashtable hash)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string item in hash.Keys)
                builder.AppendLine(item + ": " + hash[item]);

            return builder.ToString();
        }

        public static string ElemListItemToString(ElemListItem listItem)
        {
            Hashtable ElemHash = new Hashtable();
            PropertyInfo[] properties = listItem.GetType()
                                                .GetProperties();
            foreach (PropertyInfo prop in properties)
                ElemHash.Add(prop.Name, prop.GetValue(listItem));

            return HashToString(ElemHash);
        }

        public static ElemListItem HashToElemListItem(Hashtable hash)
        {
            ElemListItem ElemLItem = new ElemListItem()
            {
                Action = (string)hash["Action"],
                ElemAutoID = (string)hash["AutoID"],
                ElemClass = (string)hash["Class"],
                ElemName = (string)hash["Name"],
                WinName = (string)hash["WinName"],
                Data = (string)hash["Data"],
            };
            return ElemLItem;
        }

        public static Hashtable ElemListItemToHash(ElemListItem listItem)
        {
            Hashtable ElemHash = new Hashtable();
            PropertyInfo[] properties = listItem.GetType()
                                                .GetProperties();
            foreach (PropertyInfo prop in properties)
                ElemHash.Add(prop.Name, prop.GetValue(listItem));

            return ElemHash;
        }        

        public static Hashtable AutoElemToHash(AutomationElement element)
        {
            Hashtable _ReturnTable = new Hashtable();
            _ReturnTable.Add("Name", element.Current.Name);
            _ReturnTable.Add("AutoID", element.Current.AutomationId);
            _ReturnTable.Add("Class", element.Current.ClassName);
            _ReturnTable.Add("ElemType", element.Current.ControlType.ProgrammaticName);
            _ReturnTable.Add("ProcID", element.Current.ProcessId);
            _ReturnTable.Add("ItemType", element.Current.ItemType);
            _ReturnTable.Add("ParentName", UIControl.GetTopLevelWindow(element).Current.Name);
            _ReturnTable.Add("Element", element);
            _ReturnTable.Add("ElemXML", BeautifyXMLDoc(AutoElemToXMLElem(element))); 
            return _ReturnTable;
        }
        
        public static XmlDocument AutoElemToXMLElem(AutomationElement element)
        {
            List<Hashtable> ElemTree = HierarchyActions.Tree(element);
            XmlDocument XML = new XmlDocument();
            XmlNode lastNode = XML;

            foreach (Hashtable level in ElemTree)
            {
                XmlNode Element = XML.CreateElement(level["CtrlType"].ToString());
                lastNode.AppendChild(Element);

                XmlAttribute NameAttr = XML.CreateAttribute("Name");
                XmlAttribute AutoIDAttr = XML.CreateAttribute("AutoID");
                XmlAttribute ClassAttr = XML.CreateAttribute("Class");

                NameAttr.Value = level["Name"].ToString();
                AutoIDAttr.Value = level["AutoID"].ToString();
                ClassAttr.Value = level["Class"].ToString();

                Element.Attributes.Append(NameAttr);
                Element.Attributes.Append(AutoIDAttr);
                Element.Attributes.Append(ClassAttr);

                lastNode = Element;
            }
            return XML;
        }

        public static string BeautifyXMLDoc(XmlDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                doc.Save(writer);
            }
            return sb.ToString();
        }

        #endregion
    }
}
