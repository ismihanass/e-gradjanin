using eGradjaninFBiH.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QRCoder;
namespace eGradjaninFBiH.Services;
public class PdfDocumentService(IWebHostEnvironment env)
{
    public byte[] Create(ServiceRequest request, IssuedDocument document, string verifyUrl)
    {
        QuestPDF.Settings.License=LicenseType.Community;
        using var qr=new QRCodeGenerator(); using var data=qr.CreateQrCode(verifyUrl,QRCodeGenerator.ECCLevel.Q); var png=new PngByteQRCode(data); var qrBytes=png.GetGraphic(8);
        return Document.Create(container=>container.Page(page=>{
            page.Size(PageSizes.A4); page.Margin(42); page.DefaultTextStyle(x=>x.FontSize(11));
            page.Header().Column(c=>{c.Item().Text("eGrađanin FBiH").FontSize(22).Bold().FontColor("#B7192B"); c.Item().Text("DIGITALNI ARHIV • DEMONSTRACIONI PRIMJERAK").FontSize(9).FontColor(Colors.Grey.Darken1); c.Item().PaddingTop(8).LineHorizontal(1).LineColor("#B7192B");});
            page.Content().PaddingVertical(28).Column(c=>{
                c.Item().AlignCenter().Text("POTVRDA O DEMONSTRACIJI ZAHTJEVA").FontSize(16).Bold();
                c.Item().PaddingTop(20).Text($"Vrsta usluge: {request.AdministrativeService?.Name}");
                c.Item().Text($"Referentni broj zahtjeva: {request.Reference}"); c.Item().Text($"Referenca demo dokumenta: {document.DocumentReference}");
                c.Item().Text($"Datum izdavanja: {document.IssuedAt:dd.MM.yyyy.}");
                c.Item().PaddingTop(20).Background("#FFF4E5").Padding(12).Text("VAŽNO: Ovaj PDF je demonstracioni primjerak. Nije javna isprava, nije potpisan kvalifikovanim elektronskim potpisom/pečatom i nema pravnu snagu službenog dokumenta.").Bold();
                c.Item().PaddingTop(25).AlignCenter().Width(115).Image(qrBytes); c.Item().AlignCenter().Text("Skenirajte QR kod za demo provjeru").FontSize(9);
            });
            page.Footer().AlignCenter().Text("eGrađanin FBiH • Demo okruženje • Nije službeni dokument").FontSize(8).FontColor(Colors.Grey.Darken1);
        })).GeneratePdf();
    }
}
