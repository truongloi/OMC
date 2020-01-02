using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Telnet_OMC
{
    public partial class frmLogin : Form
    {
        private int count = 0;
        public frmLogin()
        {
            InitializeComponent();
            DateTime now = DateTime.Now;
            logFile(now.ToString(), "Mo ung dung");
        }
        private void logFile(string time, string noidung)
        {
            try
            {
                //String filepath = Application.StartupPath + @"\log\log_sms.log";// đường dẫn của file
                String filepath = @"d:\OMC CAMERA\log\log_sms.log";// đường dẫn của file
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("-- " + time + ": " + noidung);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string key = txtKey.Text;
            login(key);
        }
        private void login(string key)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                // Ket noi file ini
                INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Config.ini");
                string hash = inif.Read("CONFIG", "KEY");

                if (VerifyMd5Hash(md5Hash, key, hash))
                {
                    frmMain frm = new frmMain(this);
                    frm.Show();
                    this.Hide();
                    count = 0;
                    txtKey.Text = "";
                    DateTime now = DateTime.Now;
                    logFile(now.ToString(), "Dang nhap");
                }
                else
                {
                    if(count >= 10)
                    {
                        MessageBox.Show("Bạn nhập key sai quá số lần quy định! Vui lòng liên hệ Quản trị!", "Cảnh báo");
                    }
                    else
                    {
                        MessageBox.Show("Sai key! Vui lòng nhập lại key!", "Cảnh báo");
                    }
                    count++;
                }
            }
        }
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, GetMd5Hash(md5Hash, input));

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void txtKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string key = txtKey.Text;
                login(key);
            }
        }

        private void frmLogin_FormClosed(object sender, FormClosedEventArgs e)
        {
            DateTime now = DateTime.Now;
            logFile(now.ToString(), "Dong ung dung");
            Application.Exit();
        }
    }
}
