using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Security.Cryptography;

namespace kacicnik_VIZ_3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private byte[] nesif_dat;
        private byte[] sif_dat;
        //private byte[] desif_dat;
        private string tip;
        private byte[] AESkljuc;
        private int dolzina_kljuca_AES = 128;
        private int dolzina_kljuca_RSA = 1024;
        private string RSA_javni_kljuc;
        private string RSA_zasebni_kljuc;
        private bool imamo_kljuc = false;
        private bool imamo_kljuc_RSA_javni = false;
        private bool imamo_kljuc_RSA_zasebni = false;

        RijndaelManaged generiraj;
        public MainWindow()
        {
            InitializeComponent();
        }
        private byte[] generiraj_kljuc() {

            generiraj = new RijndaelManaged();
            imamo_kljuc = true;
            generiraj.KeySize = dolzina_kljuca_AES;
            generiraj.GenerateKey();

            return generiraj.Key;
        }

        private void generiraj_kljuc_RSA()
        {

            RSA rsa = new RSACryptoServiceProvider(dolzina_kljuca_RSA);
            imamo_kljuc_RSA_javni = true;
            imamo_kljuc_RSA_zasebni = true;
            RSA_zasebni_kljuc = rsa.ToXmlString(true);
            RSA_javni_kljuc = rsa.ToXmlString(false);
        }

        private void AES_sifriraj() {
            if (imamo_kljuc == true)
            {
                AesCryptoServiceProvider AES = new AesCryptoServiceProvider();
                byte[] kljuc_iv = new byte[16];

                Buffer.BlockCopy(AESkljuc, 0, kljuc_iv, 0, 16);
                AES.Key = AESkljuc;

                AES.IV = kljuc_iv;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (
                        CryptoStream cryptoStream = new CryptoStream(memoryStream, AES.CreateEncryptor(), CryptoStreamMode.Write)
                        )
                    {
                        cryptoStream.Write(nesif_dat, 0, nesif_dat.Length);
                        cryptoStream.Close();
                        sif_dat = new byte[memoryStream.ToArray().Length];
                        sif_dat = memoryStream.ToArray();
                    }
                }
            }
            else
            {
                MessageBox.Show("Izberite kljuc ki ga zelite uporabljati.");
            }
        }

        private void AES_desifriraj()
        {
            if (imamo_kljuc == true)
            {
                AesCryptoServiceProvider AES = new AesCryptoServiceProvider();

                byte[] kljuc_iv = new byte[16];
                Buffer.BlockCopy(AESkljuc, 0, kljuc_iv, 0, 16);

                AES.Key = AESkljuc;
                AES.IV = kljuc_iv;

                try
                {

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(sif_dat, 0, sif_dat.Length);
                            cs.Close();
                            nesif_dat = new byte[ms.ToArray().Length];
                            nesif_dat = ms.ToArray();
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Kljuc ni pravilen.");
                }
            }
            else
            {
                MessageBox.Show("Izberite kljuc ki ga zelite uporabljati.");
            }

        }

        private void RSA_sifriranje()
        {
            if (imamo_kljuc_RSA_javni ==  true)
            {
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

                RSA.FromXmlString(RSA_javni_kljuc);
                sif_dat = RSA.Encrypt(nesif_dat,false);
            }
            else
            {
                MessageBox.Show("Izberite kljuc ki ga zelite uporabljati(RSA javni kljuc).");
            }
        }

        private void RSA_desifriranje()
        {
            if (imamo_kljuc_RSA_zasebni == true)
            {
                
                try
                {
                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

                    RSA.FromXmlString(RSA_zasebni_kljuc);

                    nesif_dat = RSA.Decrypt(sif_dat, false);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Kljuc ni pravilen.");
                }
            }
            else
            {
                MessageBox.Show("Izberite kljuc ki ga zelite uporabljati (RSA zasebni kljuc).");
            }
        }

        private void btnNalozi_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog datoteka = new OpenFileDialog();
            if (datoteka.ShowDialog() == true) {
                //datoteko zapisemo v polje byte
                tip = System.IO.Path.GetExtension(datoteka.FileName);
                if (Nalaganje.SelectedItem.ToString().Contains("Nesifrirana"))
                {
                    string podatki = datoteka.FileName;
                    nesif_dat = new byte[Encoding.UTF8.GetBytes(podatki).Length];
                    nesif_dat = File.ReadAllBytes(datoteka.FileName);
                }
                else if (Nalaganje.SelectedItem.ToString().Contains("Sifrirana"))
                {
                    string podatki = datoteka.FileName;
                    sif_dat = new byte[Encoding.UTF8.GetBytes(podatki).Length];
                    sif_dat = File.ReadAllBytes(datoteka.FileName);
                }
            }
            Console.WriteLine(Nalaganje.SelectedItem.ToString());
        }

        private void btnShrani_Click(object sender, RoutedEventArgs e)
        {
            
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            if (saveFileDialog1.ShowDialog() == true)
            {
                if (shranjevanje.SelectedItem.ToString().Contains("Nesifrirana"))
                {
                    if (nesif_dat == null)
                    {
                        MessageBox.Show("Nesifrirana datoteka ni nalozena.");
                    }
                    else
                    {
                        File.WriteAllBytes(saveFileDialog1.FileName + tip, nesif_dat);
                    }
                }
                else if (shranjevanje.SelectedItem.ToString().Contains("Sifrirana"))
                {
                    if (sif_dat == null)
                    {
                        MessageBox.Show("Sifrirana datoteka ni nalozena.");
                    }
                    else
                    {
                        File.WriteAllBytes(saveFileDialog1.FileName + tip, sif_dat);
                    }
                }
            }
        }

        private void btnKljuc_Click(object sender, RoutedEventArgs e)
        {
            if (Kljuc_Combo.SelectedItem.ToString().Contains("Naloži AES"))
            {
                imamo_kljuc = true;
                OpenFileDialog datoteka = new OpenFileDialog();
                datoteka.Filter = "text |*.txt";

                if (datoteka.ShowDialog() == true)
                {
                    AESkljuc = File.ReadAllBytes(datoteka.FileName);
                    Console.WriteLine(AESkljuc.ToString());
                }
            }

            if (Kljuc_Combo.SelectedItem.ToString().Contains("Nalozi RSA javni"))
            {
                imamo_kljuc_RSA_javni = true;
                OpenFileDialog datoteka = new OpenFileDialog();
                datoteka.Filter = "text |*.txt";

                if (datoteka.ShowDialog() == true)
                {
                    RSA_javni_kljuc = File.ReadAllText(datoteka.FileName);
                    Console.WriteLine(RSA_javni_kljuc);
                }
            }

            if (Kljuc_Combo.SelectedItem.ToString().Contains("Nalozi RSA zasebni"))
            {
                imamo_kljuc_RSA_zasebni = true;
                OpenFileDialog datoteka = new OpenFileDialog();
                datoteka.Filter = "text |*.txt";

                if (datoteka.ShowDialog() == true)
                {
                    RSA_zasebni_kljuc = File.ReadAllText(datoteka.FileName);
                    Console.WriteLine(RSA_zasebni_kljuc);
                }
            }


            if (Kljuc_Combo.SelectedItem.ToString().Contains("Shrani AES"))
            {
               
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "text |*.txt";
                saveFileDialog1.DefaultExt = "txt";
                saveFileDialog1.AddExtension = true;
                if (saveFileDialog1.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveFileDialog1.FileName, AESkljuc);
                }
            }

            if (Kljuc_Combo.SelectedItem.ToString().Contains("Shrani javni RSA"))
            {

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "text |*.txt";
                saveFileDialog1.DefaultExt = "txt";
                saveFileDialog1.AddExtension = true;
                if (saveFileDialog1.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog1.FileName, RSA_javni_kljuc);
                }
            }

            if (Kljuc_Combo.SelectedItem.ToString().Contains("Shrani zasebni RSA"))
            {

                SaveFileDialog saveFileDialog1 = new SaveFileDialog();
                saveFileDialog1.Filter = "text |*.txt";
                saveFileDialog1.DefaultExt = "txt";
                saveFileDialog1.AddExtension = true;
                if (saveFileDialog1.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog1.FileName, RSA_zasebni_kljuc);
                }
            }


            if (Kljuc_Combo.SelectedItem.ToString().Contains("Generiraj AES"))
            {
                AESkljuc = generiraj_kljuc();
            }
            if (Kljuc_Combo.SelectedItem.ToString().Contains("Generiraj RSA"))
            {
                generiraj_kljuc_RSA();
            }

        }

        private void btnDESsif_Click(object sender, RoutedEventArgs e)
        {
            AES_sifriraj();
        }

        private void btnDESdesif_Click(object sender, RoutedEventArgs e)
        {
            AES_desifriraj();
        }

        private void btnDES3sif_Click(object sender, RoutedEventArgs e)
        {
            RSA_sifriranje();
        }

        private void btnDES3desif_Click(object sender, RoutedEventArgs e)
        {
           RSA_desifriranje();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dolzina_kljuca_AES = 128;
            LBdolzina_kl.Content = "Izbrana: 128";
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            dolzina_kljuca_AES = 192;
            LBdolzina_kl.Content = "Izbrana: 192";
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            dolzina_kljuca_AES = 256;
            LBdolzina_kl.Content = "Izbrana: 256";
        }

        private void btmRSA1_Click(object sender, RoutedEventArgs e)
        {
            dolzina_kljuca_RSA = 1024;
            Lrsa.Content = "Izbrana: 1024";
        }

        private void btnRSA2_Click(object sender, RoutedEventArgs e)
        {
            dolzina_kljuca_RSA = 2048;
            Lrsa.Content = "Izbrana: 2048";
        }
    }
}
