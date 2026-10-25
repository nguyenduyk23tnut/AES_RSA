#nullable disable
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace BAITAP2
{
    public partial class Form1 : Form
    {
        private RSA rsa;
        private string currentPublicKeyPem = "";
        private string currentPrivateKeyPem = "";

        // Tab Controls
        private TabControl tabControl;
        private TabPage tabKeyAndPem;
        private TabPage tabEncryptDecrypt;
        private TabPage tabBenchmark;
        private TabPage tabHashAndSign;

        // Tab 1 Controls
        private Button btnGenKey, btnExportPem, btnImportPub, btnImportPriv;
        private TextBox txtPemViewer;

        // Tab 2 Controls
        private TextBox txtInputData, txtEncryptedData, txtDecryptedData;
        private Label lblDataInfo;
        private Button btnEncryptData, btnDecryptData, btnSaveEncFile, btnLoadEncFile;

        // Tab 3 Controls
        private TextBox txtBenchmarkLog;
        private Button btnRunBenchmark;

        // Tab 4 Controls
        private TextBox txtHashInput, txtHashOutput, txtSignature;
        private Button btnGenHash, btnSignData, btnVerifySign;
        private Label lblSignNote;

        public Form1()
        {
            InitializeComponentCustom();
            rsa = RSA.Create(2048);
        }

        private void InitializeComponentCustom()
        {
            this.Text = "Bai 2: Ma hoa RSA, PEM, Benchmark & Chu ky so";
            this.Size = new System.Drawing.Size(850, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            tabControl = new TabControl { Dock = DockStyle.Fill };

            tabKeyAndPem = new TabPage("1. Khoa & PEM");
            tabEncryptDecrypt = new TabPage("2. Ma hoa & Giai ma");
            tabBenchmark = new TabPage("3. Danh gia toc do");
            tabHashAndSign = new TabPage("4. SHA256 & Chu ky so");

            InitTab1();
            InitTab2();
            InitTab3();
            InitTab4();

            tabControl.TabPages.Add(tabKeyAndPem);
            tabControl.TabPages.Add(tabEncryptDecrypt);
            tabControl.TabPages.Add(tabBenchmark);
            tabControl.TabPages.Add(tabHashAndSign);

            this.Controls.Add(tabControl);
        }

        private void InitTab1()
        {
            btnGenKey = new Button { Text = "1. Sinh cap khoa RSA", Location = new System.Drawing.Point(20, 20), Size = new System.Drawing.Size(180, 35) };
            btnGenKey.Click += BtnGenKey_Click;

            btnExportPem = new Button { Text = "2. Xuat khoa ra file PEM", Location = new System.Drawing.Point(210, 20), Size = new System.Drawing.Size(180, 35) };
            btnExportPem.Click += BtnExportPem_Click;

            btnImportPub = new Button { Text = "3. Nhap Public Key (PEM)", Location = new System.Drawing.Point(400, 20), Size = new System.Drawing.Size(180, 35) };
            btnImportPub.Click += BtnImportPub_Click;

            btnImportPriv = new Button { Text = "4. Nhap Private Key (PEM)", Location = new System.Drawing.Point(590, 20), Size = new System.Drawing.Size(180, 35) };
            btnImportPriv.Click += BtnImportPriv_Click;

            var lblView = new Label { Text = "View thong tin Key & parameters (n, e, d):", Location = new System.Drawing.Point(20, 70), AutoSize = true };
            txtPemViewer = new TextBox { Location = new System.Drawing.Point(20, 95), Size = new System.Drawing.Size(780, 520), Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new System.Drawing.Font("Consolas", 10) };

            tabKeyAndPem.Controls.AddRange(new Control[] { btnGenKey, btnExportPem, btnImportPub, btnImportPriv, lblView, txtPemViewer });
        }

        private void InitTab2()
        {
            var lbl1 = new Label { Text = "Du lieu dau vao (<= 190 byte / hoac 32 byte AES key):", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            txtInputData = new TextBox { Location = new System.Drawing.Point(20, 45), Size = new System.Drawing.Size(780, 60), Multiline = true };

            lblDataInfo = new Label { Text = "So byte hien tai: 0 byte", Location = new System.Drawing.Point(20, 110), AutoSize = true, ForeColor = System.Drawing.Color.DarkGreen };
            txtInputData.TextChanged += (s, e) => { lblDataInfo.Text = $"So byte hien tai: {Encoding.UTF8.GetByteCount(txtInputData.Text)} byte"; };

            btnEncryptData = new Button { Text = "Ma hoa (Public Key)", Location = new System.Drawing.Point(20, 140), Size = new System.Drawing.Size(180, 35) };
            btnEncryptData.Click += BtnEncryptData_Click;

            btnSaveEncFile = new Button { Text = "Luu file da ma hoa", Location = new System.Drawing.Point(210, 140), Size = new System.Drawing.Size(160, 35) };
            btnSaveEncFile.Click += BtnSaveEncFile_Click;

            var lbl2 = new Label { Text = "Ket qua ma hoa (Base64):", Location = new System.Drawing.Point(20, 190), AutoSize = true };
            txtEncryptedData = new TextBox { Location = new System.Drawing.Point(20, 215), Size = new System.Drawing.Size(780, 100), Multiline = true, ScrollBars = ScrollBars.Vertical };

            btnLoadEncFile = new Button { Text = "Tai file ma hoa", Location = new System.Drawing.Point(20, 330), Size = new System.Drawing.Size(160, 35) };
            btnLoadEncFile.Click += BtnLoadEncFile_Click;

            btnDecryptData = new Button { Text = "Giai ma (Private Key)", Location = new System.Drawing.Point(190, 330), Size = new System.Drawing.Size(180, 35) };
            btnDecryptData.Click += BtnDecryptData_Click;

            var lbl3 = new Label { Text = "Ket qua giai ma & so sanh nguyen ven:", Location = new System.Drawing.Point(20, 380), AutoSize = true };
            txtDecryptedData = new TextBox { Location = new System.Drawing.Point(20, 405), Size = new System.Drawing.Size(780, 150), Multiline = true, ScrollBars = ScrollBars.Vertical };

            tabEncryptDecrypt.Controls.AddRange(new Control[] { lbl1, txtInputData, lblDataInfo, btnEncryptData, btnSaveEncFile, lbl2, txtEncryptedData, btnLoadEncFile, btnDecryptData, lbl3, txtDecryptedData });
        }

        private void InitTab3()
        {
            btnRunBenchmark = new Button { Text = "Chay danh gia toc do Ma hoa / Giai ma", Location = new System.Drawing.Point(20, 20), Size = new System.Drawing.Size(300, 40) };
            btnRunBenchmark.Click += BtnRunBenchmark_Click;

            txtBenchmarkLog = new TextBox { Location = new System.Drawing.Point(20, 80), Size = new System.Drawing.Size(780, 540), Multiline = true, ScrollBars = ScrollBars.Vertical, Font = new System.Drawing.Font("Consolas", 10) };

            tabBenchmark.Controls.AddRange(new Control[] { btnRunBenchmark, txtBenchmarkLog });
        }

        private void InitTab4()
        {
            var lbl1 = new Label { Text = "Nhap van ban / Du lieu tao bam SHA256:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            txtHashInput = new TextBox { Location = new System.Drawing.Point(20, 45), Size = new System.Drawing.Size(780, 60), Multiline = true };
            txtHashInput.TextChanged += TxtHashInput_TextChanged;

            btnGenHash = new Button { Text = "Tao chuoi bam SHA256", Location = new System.Drawing.Point(20, 115), Size = new System.Drawing.Size(180, 30) };

            var lbl2 = new Label { Text = "Gia tri bam (Hex):", Location = new System.Drawing.Point(20, 155), AutoSize = true };
            txtHashOutput = new TextBox { Location = new System.Drawing.Point(20, 175), Size = new System.Drawing.Size(780, 40), ReadOnly = true, Font = new System.Drawing.Font("Consolas", 9) };

            btnSignData = new Button { Text = "Tao Chu ky so (Private Key)", Location = new System.Drawing.Point(20, 230), Size = new System.Drawing.Size(200, 35) };
            btnSignData.Click += BtnSignData_Click;

            btnVerifySign = new Button { Text = "Xac thuc Chu ky (Public Key)", Location = new System.Drawing.Point(230, 230), Size = new System.Drawing.Size(210, 35) };
            btnVerifySign.Click += BtnVerifySign_Click;

            var lbl3 = new Label { Text = "Chu ky so (Base64):", Location = new System.Drawing.Point(20, 280), AutoSize = true };
            txtSignature = new TextBox { Location = new System.Drawing.Point(20, 305), Size = new System.Drawing.Size(780, 70), Multiline = true, ScrollBars = ScrollBars.Vertical };

            lblSignNote = new Label
            {
                Text = "Giai thich yeu cau: Vi sao cung 1 du lieu, cung 1 private key ma cac lan ky cho ra chu ky khac nhau?\n" +
                       "-> Do chuan ky so RSA (nhu PKCS#1 v1.5 hoac PSS) su dung co che Padding ngau nhien (Salt/Random bytes)\n" +
                       "o moi lan ky, giup dam bao tinh bao mat tuyet doi, tranh bi tan cong lap lai (Replay attack).",
                Location = new System.Drawing.Point(20, 395),
                Size = new System.Drawing.Size(780, 100),
                ForeColor = System.Drawing.Color.DarkBlue
            };

            tabHashAndSign.Controls.AddRange(new Control[] { lbl1, txtHashInput, btnGenHash, lbl2, txtHashOutput, btnSignData, btnVerifySign, lbl3, txtSignature, lblSignNote });
        }

        // --- TAB 1 LOGIC ---
        private void BtnGenKey_Click(object sender, EventArgs e)
        {
            rsa = RSA.Create(2048);
            var parameters = rsa.ExportParameters(true);

            currentPublicKeyPem = ExportPublicKeyToPem(rsa);
            currentPrivateKeyPem = ExportPrivateKeyToPem(rsa);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== THONG TIN CAP KHOA RSA 2048-BIT ===");
            sb.AppendLine($"Modulus (n): {BitConverter.ToString(parameters.Modulus ?? Array.Empty<byte>()).Replace("-", "")}");
            sb.AppendLine($"Exponent cong khai (e): {BitConverter.ToString(parameters.Exponent ?? Array.Empty<byte>()).Replace("-", "")}");
            sb.AppendLine($"Prime P: {BitConverter.ToString(parameters.P ?? Array.Empty<byte>()).Replace("-", "")}");
            sb.AppendLine($"Prime Q: {BitConverter.ToString(parameters.Q ?? Array.Empty<byte>()).Replace("-", "")}");
            sb.AppendLine("\n------------------- PUBLIC KEY (PEM) -------------------");
            sb.AppendLine(currentPublicKeyPem);
            sb.AppendLine("--------------------------------------------------------");

            txtPemViewer.Text = sb.ToString();
            MessageBox.Show("Sinh cap khoa RSA thanh cong!", "Thong bao", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnExportPem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentPublicKeyPem)) { MessageBox.Show("Vui long sinh khoa truooc!"); return; }
            using (var sfd = new SaveFileDialog() { Filter = "PEM Files (*.pem)|*.pem" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(sfd.FileName + "_public.pem", currentPublicKeyPem);
                    File.WriteAllText(sfd.FileName + "_private.pem", currentPrivateKeyPem);
                    MessageBox.Show("Xuat file PEM thanh cong!", "Thanh cong");
                }
            }
        }

        private void BtnImportPub_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog() { Filter = "PEM Files (*.pem)|*.pem|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    currentPublicKeyPem = File.ReadAllText(ofd.FileName);
                    rsa.ImportFromPem(currentPublicKeyPem);
                    txtPemViewer.Text = "--- IMPORTED PUBLIC KEY ---\n" + currentPublicKeyPem;
                    MessageBox.Show("Nhap Public Key tu PEM thanh cong!");
                }
            }
        }

        private void BtnImportPriv_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog() { Filter = "PEM Files (*.pem)|*.pem|All Files (*.*)|*.*" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    currentPrivateKeyPem = File.ReadAllText(ofd.FileName);
                    rsa.ImportFromPem(currentPrivateKeyPem);
                    var p = rsa.ExportParameters(true);
                    string modStr = BitConverter.ToString(p.Modulus ?? Array.Empty<byte>());
                    if (modStr.Length > 30) modStr = modStr.Substring(0, 30);
                    txtPemViewer.Text = $"--- IMPORTED PRIVATE KEY ---\nn = {modStr}...\ne = {BitConverter.ToString(p.Exponent ?? Array.Empty<byte>())}\n\n{currentPrivateKeyPem}";
                    MessageBox.Show("Nhap Private Key tu PEM thanh cong!");
                }
            }
        }

        private string ExportPublicKeyToPem(RSA rsaObj)
        {
            return "-----BEGIN PUBLIC KEY-----\n" + Convert.ToBase64String(rsaObj.ExportSubjectPublicKeyInfo()) + "\n-----END PUBLIC KEY-----";
        }

        private string ExportPrivateKeyToPem(RSA rsaObj)
        {
            return "-----BEGIN PRIVATE KEY-----\n" + Convert.ToBase64String(rsaObj.ExportPkcs8PrivateKey()) + "\n-----END PRIVATE KEY-----";
        }

        // --- TAB 2 LOGIC ---
        private void BtnEncryptData_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(txtInputData.Text);
                if (data.Length > 190)
                {
                    MessageBox.Show($"Canh bao: Du lieu qua dai ({data.Length} bytes). RSA 2048 OAEP chi ma hoa toi da ~190 byte (hoac khoa AES 32 byte).", "Canh bao", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                byte[] encrypted = rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA256);
                txtEncryptedData.Text = Convert.ToBase64String(encrypted);
                MessageBox.Show("Ma hoa thanh cong!");
            }
            catch (Exception ex) { MessageBox.Show("Loi ma hoa: " + ex.Message); }
        }

        private void BtnSaveEncFile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtEncryptedData.Text)) { MessageBox.Show("Khong co du lieu ma hoa de luu!"); return; }
            using (var sfd = new SaveFileDialog() { Filter = "Bin Files (*.bin)|*.bin|All Files (*.*)|*.*" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(sfd.FileName, Convert.FromBase64String(txtEncryptedData.Text));
                    MessageBox.Show("Luu file ma hoa thanh cong!");
                }
            }
        }

        private void BtnLoadEncFile_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    byte[] bytes = File.ReadAllBytes(ofd.FileName);
                    txtEncryptedData.Text = Convert.ToBase64String(bytes);
                    MessageBox.Show("Tai file ma hoa thanh cong!");
                }
            }
        }

        private void BtnDecryptData_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] encrypted = Convert.FromBase64String(txtEncryptedData.Text);
                byte[] decrypted = rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
                string result = Encoding.UTF8.GetString(decrypted);

                txtDecryptedData.Text = $"Ket qua giai ma:\r\n{result}\r\n\r\n--- So sanh voi nguon goc ---\r\nTrung khop hoan toan: {result == txtInputData.Text}";
                MessageBox.Show("Giai ma thanh cong!");
            }
            catch (Exception ex) { MessageBox.Show("Loi giai ma (kiem tra lai Private Key): " + ex.Message); }
        }

        // --- TAB 3 LOGIC ---
        private void BtnRunBenchmark_Click(object sender, EventArgs e)
        {
            byte[] testData = Encoding.UTF8.GetBytes("Thong diep test toc do ma hoa RSA 2048-bit cho mon an toan thong tin!");
            Stopwatch sw = new Stopwatch();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("=== KET QUA DANH GIA TOC DO (BENCHMARK) ===");
            sb.AppendLine($"- Kich thuoc du lieu test: {testData.Length} bytes");
            sb.AppendLine($"- Loai khoa: RSA 2048-bit\n");

            sw.Start();
            for (int i = 0; i < 100; i++) rsa.Encrypt(testData, RSAEncryptionPadding.OaepSHA256);
            sw.Stop();
            sb.AppendLine($"1. Ma hoa 100 lan (Public Key - e nho): {sw.ElapsedMilliseconds} ms (Trung binh: {sw.ElapsedMilliseconds / 100.0} ms/lan)");

            byte[] cipher = rsa.Encrypt(testData, RSAEncryptionPadding.OaepSHA256);
            sw.Restart();
            for (int i = 0; i < 100; i++) rsa.Decrypt(cipher, RSAEncryptionPadding.OaepSHA256);
            sw.Stop();
            sb.AppendLine($"2. Giai ma 100 lan (Private Key - d to): {sw.ElapsedMilliseconds} ms (Trung binh: {sw.ElapsedMilliseconds / 100.0} ms/lan)");

            sb.AppendLine("\nNhan xet: Phep toan dung Private Key (chua so d lon) thuong ton nhieu thoi gian hon so voi Public Key (e nho).");
            txtBenchmarkLog.Text = sb.ToString();
        }

        // --- TAB 4 LOGIC ---
        private void TxtHashInput_TextChanged(object sender, EventArgs e)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(txtHashInput.Text);
            byte[] hash = SHA256.HashData(bytes);
            txtHashOutput.Text = BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private void BtnSignData_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(txtHashInput.Text);
                byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                txtSignature.Text = Convert.ToBase64String(signature);
                MessageBox.Show("Tao chu ky so thanh cong!");
            }
            catch (Exception ex) { MessageBox.Show("Loi ky so: " + ex.Message); }
        }

        private void BtnVerifySign_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(txtHashInput.Text);
                byte[] signature = Convert.FromBase64String(txtSignature.Text);
                bool valid = rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                MessageBox.Show(valid ? "Xac thuc chu ky: HOP LE!" : "Xac thuc chu ky: KHONG HOP LE!", "Ket qua", MessageBoxButtons.OK, valid ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex) { MessageBox.Show("Loi xac thuc: " + ex.Message); }
        }
    }
}