using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;

namespace BaitapAES_WinForms
{
    public partial class Form1 : Form
    {
        private TextBox txtPlainText;
        private TextBox txtCipherText;
        private TextBox txtLog;
        private Button btnGenerateKey;
        private Button btnEncryptText;
        private Button btnEncryptFile;
        private Button btnDecryptFile;
        private Label lblKeyStatus;

        private byte[] currentKey;
        private byte[] currentIV;
        private string keyFilePath = "aes_key.bin";

        public Form1()
        {
            this.Text = "Ứng Dụng Mã Hóa AES - Bài Tập ATBMTT";
            this.Size = new Size(720, 560);
            this.StartPosition = FormStartPosition.CenterScreen;

            btnGenerateKey = new Button() { Text = "1&2. Sinh & Lưu Khóa AES", Left = 20, Top = 20, Width = 190, Height = 35 };
            btnGenerateKey.Click += BtnGenerateKey_Click;

            lblKeyStatus = new Label() { Text = "Trạng thái khóa: Chưa có", Left = 230, Top = 28, AutoSize = true };

            Label lblInput = new Label() { Text = "Văn bản cần mã hóa:", Left = 20, Top = 70, AutoSize = true };
            txtPlainText = new TextBox() { Left = 20, Top = 90, Width = 660, Height = 30, Text = "Xin chào, đây là bài tập An toàn thông tin!" };

            btnEncryptText = new Button() { Text = "4. Mã Hóa Chuỗi Văn Bản", Left = 20, Top = 130, Width = 190, Height = 35 };
            btnEncryptText.Click += BtnEncryptText_Click;

            Label lblOutput = new Label() { Text = "Kết quả mã hóa (Base64):", Left = 20, Top = 175, AutoSize = true };
            txtCipherText = new TextBox() { Left = 20, Top = 195, Width = 660, Height = 30, ReadOnly = true };

            btnEncryptFile = new Button() { Text = "5. Chọn File Mã Hóa", Left = 20, Top = 240, Width = 190, Height = 35 };
            btnEncryptFile.Click += BtnEncryptFile_Click;

            btnDecryptFile = new Button() { Text = "5. Chọn File Giải Mã", Left = 230, Top = 240, Width = 190, Height = 35 };
            btnDecryptFile.Click += BtnDecryptFile_Click;

            Label lblLog = new Label() { Text = "Lịch sử xử lý & Thời gian đo đạc:", Left = 20, Top = 285, AutoSize = true };
            txtLog = new TextBox() { Left = 20, Top = 305, Width = 660, Height = 180, Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true };

            this.Controls.Add(btnGenerateKey);
            this.Controls.Add(lblKeyStatus);
            this.Controls.Add(lblInput);
            this.Controls.Add(txtPlainText);
            this.Controls.Add(btnEncryptText);
            this.Controls.Add(lblOutput);
            this.Controls.Add(txtCipherText);
            this.Controls.Add(btnEncryptFile);
            this.Controls.Add(btnDecryptFile);
            this.Controls.Add(lblLog);
            this.Controls.Add(txtLog);
        }

        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }

        private void BtnGenerateKey_Click(object sender, EventArgs e)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.GenerateKey();
                aes.GenerateIV();
                currentKey = aes.Key;
                currentIV = aes.IV;

                byte[] keyAndIv = currentKey.Concat(currentIV).ToArray();
                File.WriteAllBytes(keyFilePath, keyAndIv);

                lblKeyStatus.Text = "Trạng thái khóa: Đã sinh & lưu (AES-256)";
                Log($"Đã sinh khóa AES-256 thành công và lưu vào file: {keyFilePath}");
            }
        }

        private bool EnsureKeyLoaded()
        {
            if (currentKey != null && currentIV != null) return true;

            if (File.Exists(keyFilePath))
            {
                byte[] keyAndIv = File.ReadAllBytes(keyFilePath);
                currentKey = keyAndIv.Take(32).ToArray();
                currentIV = keyAndIv.Skip(32).ToArray();
                lblKeyStatus.Text = "Trạng thái khóa: Đã import từ file";
                Log("Đã tự động import khóa từ file aes_key.bin.");
                return true;
            }

            MessageBox.Show("Vui lòng bấm 'Sinh & Lưu Khóa AES' trước!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void BtnEncryptText_Click(object sender, EventArgs e)
        {
            if (!EnsureKeyLoaded()) return;

            using (Aes aes = Aes.Create())
            {
                aes.Key = currentKey;
                aes.IV = currentIV;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(txtPlainText.Text);
                    }
                    string base64Result = Convert.ToBase64String(ms.ToArray());
                    txtCipherText.Text = base64Result;
                    Log("Mã hóa văn bản thành công.");
                }
            }
        }

        private void BtnEncryptFile_Click(object sender, EventArgs e)
        {
            if (!EnsureKeyLoaded()) return;

            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string sourceFile = ofd.FileName;
                string destFile = sourceFile + ".enc";

                Stopwatch sw = Stopwatch.StartNew();
                using (FileStream fsIn = new FileStream(sourceFile, FileMode.Open))
                using (FileStream fsOut = new FileStream(destFile, FileMode.Create))
                using (Aes aes = Aes.Create())
                {
                    aes.Key = currentKey;
                    aes.IV = currentIV;
                    using (CryptoStream cs = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        fsIn.CopyTo(cs);
                    }
                }
                sw.Stop();
                long sizeMB = new FileInfo(sourceFile).Length / (1024 * 1024);
                Log($"[⏱️] Mã hóa File ({sizeMB} MB) trong: {sw.ElapsedMilliseconds} ms. File: {destFile}");
            }
        }

        private void BtnDecryptFile_Click(object sender, EventArgs e)
        {
            if (!EnsureKeyLoaded()) return;

            OpenFileDialog ofd = new OpenFileDialog() { Title = "Chọn file mã hóa (.enc) để giải mã" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string encryptedFile = ofd.FileName;
                string decryptedFile = encryptedFile.Replace(".enc", "_decrypted.dat");

                Stopwatch sw = Stopwatch.StartNew();
                using (FileStream fsIn = new FileStream(encryptedFile, FileMode.Open))
                using (FileStream fsOut = new FileStream(decryptedFile, FileMode.Create))
                using (Aes aes = Aes.Create())
                {
                    aes.Key = currentKey;
                    aes.IV = currentIV;
                    using (CryptoStream cs = new CryptoStream(fsIn, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        cs.CopyTo(fsOut);
                    }
                }
                sw.Stop();
                Log($"[⏱️] Giải mã File trong: {sw.ElapsedMilliseconds} ms. File: {decryptedFile}");
            }
        }
    }
}