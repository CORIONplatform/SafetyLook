using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorionSafetyLook.Models
{
    public static class CorionSafetyLookExceptionCodes
    {
        public const int FILE_NOT_SET = 0x01010001;
        public const int PASSWORD_NOT_SET = 0x01010002;
        public const int INVALID_CORION_FILE = 0x01010003;
        public const int WRONG_PASSWORD = 0x01010004;
        public const int UNKNOWN_ENCRYPTION = 0x01010005;
    }
}
