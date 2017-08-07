using CorionSafetyLook.Models;
using MebiusLib;
using Newtonsoft.Json;
using SjclHelpers.Codec;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using TC.Translate;

namespace CorionSafetyLook
{

    public partial class MainWindow : Window
    {
        string Password { get; set; }
        string FileLocation { get; set; }

        DispatcherTimer timer = null;
        TimeSpan time;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += (a, b) => { CleanData(); };
        }

        private void SetError(string message)
        {
            tbMessage.Text = message;
        }


        private void BrowseFileLocation(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = FileLocation;
            dlg.DefaultExt = ".cor";
            dlg.Filter = "Corion File (*.cor)|*.cor|All files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                FileLocation = dlg.FileName;
                tbFileLocation.Text = FileLocation;
            }
        }

        private void DecryptButtonClicked(object sender, RoutedEventArgs e)
        {
            this.CleanData();
            try
            {
                if (string.IsNullOrEmpty(FileLocation))
                {
                    throw new CorionSafetyLookException(CorionSafetyLookExceptionCodes.FILE_NOT_SET);
                }

                try
                {
                    var corionFile = ReadCorionFile(FileLocation);
                    if (corionFile == null || corionFile.PrivateKey == null || corionFile.PublicKey == null || corionFile.EncryptVersion == 0)
                    {
                        throw new CorionSafetyLookException(CorionSafetyLookExceptionCodes.INVALID_CORION_FILE);
                    }

                    if (corionFile.Encrypted)
                    {
                        if (string.IsNullOrEmpty(Password))
                        {
                            throw new CorionSafetyLookException(CorionSafetyLookExceptionCodes.PASSWORD_NOT_SET);
                        }
                        if (corionFile.EncryptVersion == 2)
                        {
                            tbPrivateKey.Text = DecryptPrivateKeyByPassword(corionFile.PrivateKey, Password);
                        }
                        else
                        {
                            throw new CorionSafetyLookException(CorionSafetyLookExceptionCodes.UNKNOWN_ENCRYPTION);
                        }
                    }
                    else
                    {
                        tbPrivateKey.Text = corionFile.PrivateKey;
                    }

                    tbPublicKey.Text = corionFile.PublicKey;
                    StartCounter();
                }
                catch (InvalidDataException ex)
                {
                    throw new CorionSafetyLookException(CorionSafetyLookExceptionCodes.INVALID_CORION_FILE);
                }
                catch (System.Security.Cryptography.CryptographicException ex)
                {
                    throw new CorionSafetyLookException(CorionSafetyLookExceptionCodes.WRONG_PASSWORD);
                }
                catch (Exception ex)
                {
                    if (ex.StackTrace.Contains("SJCL"))
                    {
                        throw new CorionSafetyLookException(0x01010004);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
            catch (CorionSafetyLookException ex)
            {
                SetError(LanguageContext.Instance.GetText("ERROR_" + ex.HResult.ToString("X8")));
            }
            catch (Exception ex)
            {
                SetError(ex.Message);
            }

        }


        /// <summary>
        /// Return with the decrypted hex private key
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <param name="password"></param>
        /// <returns>Return with the decrypted hex private key</returns>
        private string DecryptPrivateKeyByPassword(string encryptedData, string password)
        {
            password = keccak_256(keccak_256(password));
            // 0~32
            var saltBits = BinaryNotationConverter.ToBase64(password.Substring(0, 32));
            // 32~64
            var iv = BinaryNotationConverter.ToBase64(password.Substring(32, 32));

            var sjclDecryptor = new SJCLDecryptor("{ v : 1, iv : \"" + iv + "\", salt : \"" + saltBits + "\", ks : 256, ts : 128, mode : \"ccm\",iter:50000, cipher : \"aes\", ct : \"" + BinaryNotationConverter.ToBase64(encryptedData) + "\" }", keccak_256(password));
            return sjclDecryptor.Plaintext;
        }

        /// <summary>
        /// Return with the input string keccak encoded hex version
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string keccak_256(string input)
        {
            var data = Encoding.UTF8.GetBytes(input);
            Org.BouncyCastle.Crypto.Digests.KeccakDigest dig = new Org.BouncyCastle.Crypto.Digests.KeccakDigest(256);
            dig.Reset();
            dig.BlockUpdate(data, 0, data.Length);
            byte[] hashedBytes = new byte[256 / 8];
            dig.DoFinal(hashedBytes, 0);
            return BinaryNotationConverter.ToHex(hashedBytes);
        }

        /// <summary>
        /// load the CorionWallerKeyFile from location
        /// </summary>
        /// <param name="fileLocation"></param>
        /// <returns></returns>
        private CorionWalletKeyFile ReadCorionFile(string fileLocation)
        {
            string json = File.ReadAllText(fileLocation);
            return JsonConvert.DeserializeObject<CorionWalletKeyFile>(json);
        }

        private void StartCounter()
        {
            time = TimeSpan.FromSeconds(30);

            timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                tbTimer.Text = String.Format(LanguageContext.Instance.GetText("LEFT_TIME"), time.Seconds);
                if (time == TimeSpan.Zero)
                {
                    tbTimer.Text = "";
                    timer.Stop();
                    CleanData();
                }
                time = time.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            timer.Start();
        }

        private void CleanData()
        {
            tbPublicKey.Text = String.Empty;
            tbPrivateKey.Text = String.Empty;
            GC.Collect();

            tbMessage.Text = "";

            if (timer != null)
            {
                timer.Stop();
                tbTimer.Text = "";
            }
        }

        private void StorePassword(object sender, RoutedEventArgs e)
        {
            this.Password = passwordBox.Password;
        }

    }
}
