using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WTalk.Model;

namespace Wtalk.Desktop.CustomDataTemplate
{
    public class MessageDataTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        public DataTemplate In { get; set; }
        public DataTemplate Out { get; set; }        
    
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
              var messageVm = item as dynamic;
            if (messageVm == null)
                return this.In;
            return messageVm.IncomingMessage ? this.In : this.Out;
            
        }
        
    }
}
