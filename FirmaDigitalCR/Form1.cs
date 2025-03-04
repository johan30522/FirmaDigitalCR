using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using iText.Commons.Bouncycastle.Crypto;
using iText.Commons.Bouncycastle.Cert;
using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Kernel.Crypto;


namespace FirmaDigitalCR
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Archivos PDF (*.pdf)|*.pdf";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtDocumento.Text = openFileDialog1.FileName;
            }
        }





        private void btnFirmar_Click(object sender, EventArgs e)
        {
            X509Certificate2 cert = ObtenerCertificadoDigital();

            if (cert != null)
            {
                MessageBox.Show($"Certificado encontrado:\n{cert.Subject}", "Firma Digital", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No se encontró un certificado digital válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // 🔹 Verifica si el usuario seleccionó un documento antes de firmar
            if (string.IsNullOrWhiteSpace(txtDocumento.Text) || !File.Exists(txtDocumento.Text))
            {
                MessageBox.Show("Por favor, selecciona un documento válido antes de firmar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FirmarDocumento(txtDocumento.Text);
        }


        public void FirmarDocumento(string rutaPDF)
        {
            X509Certificate2 cert = ObtenerCertificadoDigital();
            if (cert == null)
            {
                MessageBox.Show("No se encontró un certificado digital válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 🔹 Obtener clave privada sin exportarla
                RSA privateKey = GetPrivateKey(cert);

                // 🔹 Convertir certificado X509Certificate2 a formato compatible con iText
                Org.BouncyCastle.X509.X509Certificate bcCert = new X509CertificateParser().ReadCertificate(cert.RawData);
                IX509Certificate iTextCert = new X509CertificateBC(bcCert);

                // 🔹 Convertir clave privada para iText
                //IExternalSignature pks = new PrivateKeySignature(privateKey, DigestAlgorithms.SHA256);
                IExternalSignature pks = new RSASignature(privateKey);

                // 🔹 Definir ruta de salida
                string outputPdf = Path.Combine(Path.GetDirectoryName(rutaPDF), "Documento_Firmado.pdf");

                // 🔹 Crear lector y escritor de PDF
                // 🔹 Crear `PdfSigner` antes de usar `PdfReader`
                using (PdfReader reader = new PdfReader(rutaPDF))
                using (FileStream outputStream = new FileStream(outputPdf, FileMode.Create, FileAccess.Write))
                {
                    // 🔹 Crear `PdfSigner` sin `using`
                    PdfSigner signer = new PdfSigner(reader, outputStream, new StampingProperties());

                    IExternalDigest digest = new BouncyCastleDigest();
                    signer.SignDetached(digest, pks, new IX509Certificate[] { iTextCert }, null, null, null, 0, PdfSigner.CryptoStandard.CMS);
                }


                MessageBox.Show($"Documento firmado exitosamente:\n{outputPdf}", "Firma Digital", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al firmar: {ex.Message}\n\nDetalles:\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private X509Certificate2 ObtenerCertificadoDigital()
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            // Buscar certificados que pueden ser usados para firma digital
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByKeyUsage, X509KeyUsageFlags.DigitalSignature, true);

            store.Close();

            if (certs.Count > 0)
            {
                return certs[0]; // Retorna el primer certificado válido
            }

            return null;
        }

        private RSA GetPrivateKey(X509Certificate2 cert)
        {
            if (!cert.HasPrivateKey)
                throw new Exception("El certificado no contiene una clave privada.");

            RSA rsa = cert.GetRSAPrivateKey();
            if (rsa == null)
                throw new Exception("No se pudo obtener la clave privada del certificado.");

            return rsa; // 🔹 Devuelve el objeto RSA en lugar de intentar exportarlo
        }


    }
}
