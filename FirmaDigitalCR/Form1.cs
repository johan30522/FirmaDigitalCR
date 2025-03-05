using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using iText.Commons.Bouncycastle.Crypto;
using iText.Commons.Bouncycastle.Cert;
using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Kernel.Crypto;
using static iText.IO.Codec.TiffWriter;
using System.Security.Policy;


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

            // 1️⃣ **Primero se firma digitalmente el PDF**
            string pdfFirmado = FirmarDocumento(txtDocumento.Text);

            // 2️⃣ **Luego, se agrega la marca de tiempo TSA**
            if (!string.IsNullOrEmpty(pdfFirmado))
            {
                FirmarDocumentoConMarcaTiempo(pdfFirmado);
            }
        }


        public string FirmarDocumento(string rutaPDF)
        {
            X509Certificate2 cert = ObtenerCertificadoDigital();
            if (cert == null)
            {
                MessageBox.Show("No se encontró un certificado digital válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
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
                    //PdfSigner signer = new PdfSigner(reader, outputStream, new StampingProperties());
                    PdfSigner signer = new PdfSigner(reader, outputStream, new StampingProperties().UseAppendMode());

                    IExternalDigest digest = new BouncyCastleDigest();
                    signer.SignDetached(digest, pks, new IX509Certificate[] { iTextCert }, null, null, null, 0, PdfSigner.CryptoStandard.CMS);
                }
                MessageBox.Show($"Documento firmado exitosamente:\n{outputPdf}", "Firma Digital", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return outputPdf;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al firmar: {ex.Message}\n\nDetalles:\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.Empty;
            }
        }


        public void FirmarDocumentoConMarcaTiempo(string rutaPDF)
        {


            try
            {
                string tsaURL = "http://tsa.sinpe.fi.cr/tsahttp/";
                string outputPdf = Path.Combine(Path.GetDirectoryName(rutaPDF), "Documento_Firmado_TS.pdf");

                string fieldName;
                byte[] hash;

                // 1️⃣ **Leer el documento firmado y obtener la última firma**
                using (PdfReader readerTemp = new PdfReader(rutaPDF))
                using (PdfDocument pdfDoc = new PdfDocument(readerTemp))
                {
                    SignatureUtil signatureUtil = new SignatureUtil(pdfDoc);
                    var signatureNames = signatureUtil.GetSignatureNames();
                    fieldName = signatureNames[signatureNames.Count - 1]; // Última firma

                    // ✅ Extraer el contenido firmado usando ByteRange
                    PdfDictionary sigDict = signatureUtil.GetSignatureDictionary(fieldName);
                    PdfArray byteRange = sigDict.GetAsArray(PdfName.ByteRange);

                    if (byteRange == null || byteRange.Size() != 4)
                    {
                        throw new Exception("No se encontró la estructura ByteRange en la firma.");
                    }

                    long offset1 = byteRange.GetAsNumber(0).LongValue();
                    long length1 = byteRange.GetAsNumber(1).LongValue();
                    long offset2 = byteRange.GetAsNumber(2).LongValue();
                    long length2 = byteRange.GetAsNumber(3).LongValue();

                    using (FileStream fs = new FileStream(rutaPDF, FileMode.Open, FileAccess.Read))
                    using (MemoryStream ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[length1 + length2];
                        fs.Seek(offset1, SeekOrigin.Begin);
                        fs.Read(buffer, 0, (int)length1);
                        fs.Seek(offset2, SeekOrigin.Begin);
                        fs.Read(buffer, (int)length1, (int)length2);

                        using (SHA256 sha256 = SHA256.Create())
                        {
                            hash = sha256.ComputeHash(buffer);
                        }
                    }
                } // 🔹 Se cierra `pdfDoc` aquí

                // 2️⃣ **Solicitar el sello de tiempo (TSA)**
                ITSAClient tsaClient = new TSAClientBouncyCastle(tsaURL, null, null, 4096, DigestAlgorithms.SHA256);
                byte[] timestampToken = tsaClient.GetTimeStampToken(hash);

                // 3️⃣ **Agregar la marca de tiempo al PDF**
                using (PdfReader readerFinal = new PdfReader(rutaPDF))
                using (FileStream outputStream = new FileStream(outputPdf, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    //IExternalSignatureContainer timestampContainer = new TimeStampContainer(timestampToken);
                    //PdfSigner.SignDeferred(readerFinal, fieldName, outputStream, timestampContainer);
                    PdfSigner signer = new PdfSigner(readerFinal, outputStream, new StampingProperties().UseAppendMode());
                    IExternalSignatureContainer timestampContainer = new TimeStampContainer(timestampToken);
                    signer.SignExternalContainer(timestampContainer, 8192);
                }

                MessageBox.Show($"Documento firmado con marca de tiempo:\n{outputPdf}", "Firma Digital", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar marca de tiempo: {ex.Message}\n\nDetalles:\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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


public class TimeStampContainer : IExternalSignatureContainer
{
    private readonly byte[] timestampToken;

    public TimeStampContainer(byte[] timestampToken)
    {
        this.timestampToken = timestampToken;
    }

    public void ModifySigningDictionary(PdfDictionary signDic)
    {
        // No modificar el diccionario de firma
        signDic.Put(PdfName.Filter, PdfName.Adobe_PPKLite);
        signDic.Put(PdfName.SubFilter, PdfName.ETSI_RFC3161);
    }

    public byte[] Sign(Stream data)
    {
        return timestampToken;
    }
}