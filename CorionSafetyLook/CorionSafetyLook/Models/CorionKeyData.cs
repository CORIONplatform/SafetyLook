using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorionSafetyLook.Models
{
    
    /// <summary>
    /// Corion wallet key file
    /// </summary>
    public class CorionWalletKeyFile
    {
        public string Description { get; set; }
        /// <summary>
        /// If true there is an encryption on the private key
        /// </summary>
        public bool Encrypted { get; set; }
        /// <summary>
        /// The encryption method
        /// </summary>
        public int EncryptVersion { get; set; }
        /// <summary>
        /// The wallet public key 
        /// </summary>
        public string PublicKey { get; set; }
        /// <summary>
        /// The wallet private key plain or encrypted
        /// </summary>
        public string PrivateKey { get; set; }
        /// <summary>
        /// json file version ?!
        /// </summary>
        public int Version { get; set; }
    }
}
