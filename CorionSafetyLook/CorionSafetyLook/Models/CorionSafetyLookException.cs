using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorionSafetyLook.Models
{
    public class CorionSafetyLookException : Exception
    {

        public CorionSafetyLookException(int Code)
        {
            this.HResult = Code;
        }
 
        public int Module
        {
            get
            {
                return (int)(this.HResult >> 24 & 0xff);
            }
        }

        public int LogType
        {
            get
            {
                return (int)this.HResult >> 16 & 0xff;
            }
        }

        public int? ErrorCode
        {
            get
            {
                return this.HResult & 0xffff;
            }
        }

    }
}
