using CorionSafetyLook.Models;
using Newtonsoft.Json;
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
            Loader.Visibility = Visibility.Visible;
            mainWindow.IsEnabled = false;
            this.CleanData();
            var privateKey = "";
            var publicKey = "";
            var errorMessage = "";
            var worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += (a, b) =>
            {
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
                                privateKey = DecryptPrivateKeyByPasswordEncodingVer2(corionFile.PrivateKey, Password);
                            }
                            else if (corionFile.EncryptVersion == 3)
                            {
                                privateKey = DecryptPrivateKeyByPasswordEncodingVer3(corionFile.PrivateKey, Password);
                            }
                            else
                            {
                                throw new CorionSafetyLookException(CorionSafetyLookExceptionCodes.UNKNOWN_ENCRYPTION);
                            }
                        }
                        else
                        {
                            privateKey = corionFile.PrivateKey;
                        }

                        publicKey = corionFile.PublicKey;
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
                    errorMessage = (LanguageContext.Instance.GetText("ERROR_" + ex.HResult.ToString("X8")));
                }
                catch (Exception ex)
                {
                    errorMessage = (ex.Message);
                }
            };
            worker.RunWorkerCompleted += (a, b) =>
            {
                Loader.Visibility = Visibility.Collapsed;
                mainWindow.IsEnabled = true;
                if (errorMessage == "")
                {
                    tbPrivateKey.Text = privateKey;
                    tbPublicKey.Text = publicKey;
                }
                else
                {
                    SetError(errorMessage);
                }
            };
            worker.RunWorkerAsync();

        }


        /// <summary>
        /// Return with the decrypted hex private key ver 2
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <param name="password"></param>
        /// <returns>Return with the decrypted hex private key</returns>
        private string DecryptPrivateKeyByPasswordEncodingVer2(string encryptedData, string password)
        {
           return SJCLContext.Instance.DecryptV2(encryptedData, password);
        }

        /// <summary>
        /// Return with the decrypted hex private key ver3
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <param name="password"></param>
        /// <returns>Return with the decrypted hex private key</returns>
        private string DecryptPrivateKeyByPasswordEncodingVer3(string encryptedData, string password)
        {
            return SJCLContext.Instance.DecryptV3(encryptedData, password);
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
