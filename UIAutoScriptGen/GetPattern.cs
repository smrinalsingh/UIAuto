using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace UIAutoScriptGen
{
    class GetPattern
    {
        #region Pattern Definitions
        public static InvokePattern GetInvokePattern(AutomationElement element)
        {
            return element.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
        }

        public static ValuePattern GetValuePattern(AutomationElement element)
        {
            return element.GetCurrentPattern(ValuePattern.Pattern) as ValuePattern;
        }

        public static TogglePattern GetTogglePattern(AutomationElement element)
        {
            return element.GetCurrentPattern(TogglePattern.Pattern) as TogglePattern;
        }

        public static ScrollPattern GetScrollPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(ScrollPattern.Pattern) as ScrollPattern;
        }

        public static ScrollItemPattern GetScrollItemPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(ScrollItemPattern.Pattern) as ScrollItemPattern;
        }

        public static TextPattern GetTextPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(TextPattern.Pattern) as TextPattern;
        }

        public static SynchronizedInputPattern GetSynchronizedInputPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(SynchronizedInputPattern.Pattern) as SynchronizedInputPattern;
        }

        public static SelectionPattern GetSelectionPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(SelectionPattern.Pattern) as SelectionPattern;
        }

        public static SelectionItemPattern GetSelectionItemPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(SelectionItemPattern.Pattern) as SelectionItemPattern;
        }

        public static ExpandCollapsePattern GetExpandCollapsePattern(AutomationElement element)
        {
            return element.GetCurrentPattern(ExpandCollapsePattern.Pattern) as ExpandCollapsePattern;
        }

        public static WindowPattern GetWindowPattern(AutomationElement element)
        {
            return element.GetCurrentPattern(WindowPattern.Pattern) as WindowPattern;
        }
        #endregion
    }
}
