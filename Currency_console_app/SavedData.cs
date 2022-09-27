using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency_console_app
{
    public class SavedData
    {  
        public string localCurrency { get; set; }    
        public string RateCurrency { get; set; }    
        public string RateType { get; set; }    
        public string middleRate { get; set; }    
        public string buyRate { get; set; }    
        public string sellRate { get; set; }    
        public string cbRate { get; set; }    
        public string buyMultiplier { get; set; }    
        public string sellMultiplier { get; set; }  
    }
}
