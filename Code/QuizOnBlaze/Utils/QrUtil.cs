using QRCoder;

namespace QuizOnBlaze.Utils
{
    public static class QrUtil
    {
        /// <summary>
        /// Generate QR CODE for Game Pin
        /// </summary>
        /// <param name="link">URL</param>
        /// <returns>QR CODE in PNG format</returns>
        public static string GenerateQrCodeBase64(string link)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(link, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);
            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
        }
    }
}
