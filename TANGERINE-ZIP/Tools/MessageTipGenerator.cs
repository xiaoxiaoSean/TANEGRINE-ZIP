using System;
using System.Collections.Generic;
using System.Text;

namespace TANGERINE_ZIP.Tools
{
   public static class MessageTipGenerator
    {
        public static string GenerateTip(string stageCode, string exceptionMessage)
        {
            return LanguageManager.Get("StageCodeTip1")+stageCode+"\n\n"+LanguageManager.Get("StageCodeTip2") +exceptionMessage;
        }
    }
}
