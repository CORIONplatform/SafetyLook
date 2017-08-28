using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CorionSafetyLook
{
    public class SJCLContext
    {
        static SJCLContext instance = null;
        public static SJCLContext Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SJCLContext();
                    instance.Init();
                }
                return instance;
            }
        }

        Noesis.Javascript.JavascriptContext Context;

        private SJCLContext()
        {

        }

        public void Init()
        {
            Context = new Noesis.Javascript.JavascriptContext();

            var sjcl = GetJavascriptSource("sjcl.min.js");
            var utils = GetJavascriptSource("utils.min.js");
            var sha3 = GetJavascriptSource("sha3.min.js");

            var decrypt_v2 = @"function deCryptV2(data, password) {
	                        var saltBits = sjcl.codec.base64.fromBits(sjcl.codec.hex.toBits(keccak_256(password).slice(0,32)));
	                        var iv = sjcl.codec.base64.fromBits(sjcl.codec.hex.toBits(keccak_256(password).slice(32,64)));
	                        var derivedKey = sjcl.misc.pbkdf2( keccak_256(keccak_256(password)), saltBits, 50000, 256 );
	                        return sjcl.decrypt(derivedKey, JSON.stringify({ v:1, iv:iv, salt:saltBits, ks:256, ts:128, mode:""ccm"", cipher:""aes"", ct:sjcl.codec.base64.fromBits(sjcl.codec.hex.toBits(data)) }));
                        }";
            var decrypt_v3 = @"
	            function _genKeys(plainPassword) {
		            if ( typeof(plainPassword) === '' ) { throw new Error(""Password is not initialized!""); }

                    var bn = sjcl.bn.fromBits(sjcl.misc.pbkdf2(keccak_256(keccak_256(plainPassword)), sjcl.codec.base64.fromBits(sjcl.codec.hex.toBits(keccak_256(plainPassword).slice(0, 32))), 50000, 512));
                        bn = bn.mod(sjcl.ecc.curves.c384.r);
                    return sjcl.ecc.elGamal.generateKeys(sjcl.ecc.curves.c384, 6, bn);
                }
                function deCryptV3(plainPassword, msg) {
		            return sjcl.decrypt(new sjcl.ecc.elGamal.secretKey(sjcl.ecc.curves.c384, sjcl.bn.prime.p384.fromBits(_genKeys(plainPassword).sec.get())), msg);
	            }";

            var custom_keccak_256 = @"
                function keccak_256(message) {
                    return new Keccak(256, KECCAK_PADDING, 256).update(message)[""hex""]();
                }";


            this.Context.Run(sjcl);
            this.Context.Run(sha3);
            this.Context.Run(custom_keccak_256);
            this.Context.Run(decrypt_v2);
            this.Context.Run(decrypt_v3);

        }

        public string DecryptV2(string data, string password)
        {
            var resultScript = "var result = deCryptV2('" + data + "',keccak_256('" + password + "'));";

            this.Context.Run(resultScript);
            return this.Context.GetParameter("result").ToString();
        }
        public string DecryptV3(string data, string password)
        {
            var resultScript = "var result = deCryptV3('" + password + "','"+data+"');";
            this.Context.Run(resultScript);
            return this.Context.GetParameter("result").ToString();
        }

        private static string GetJavascriptSource(string fileName)
        {
            var assembly = Assembly.GetEntryAssembly();
            var fileData = assembly.GetManifestResourceNames().SingleOrDefault(p => p.Contains("Resources."+fileName));
            string source = "";
            if (string.IsNullOrEmpty(fileData))
                throw new Exception("Javascript file ('"+fileName+"') not found in source!");
            // try to get from embed resource
            using (Stream stream = assembly.GetManifestResourceStream(fileData))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        source = reader.ReadToEnd();
                    }
                }
            }
            return source;
        }
    }
}
