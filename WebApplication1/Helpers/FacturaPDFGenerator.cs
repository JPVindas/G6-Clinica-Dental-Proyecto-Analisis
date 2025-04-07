using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebApplication1.Models;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.IO;

public static class FacturaPDFGenerator
{
    public static byte[] GenerarFactura(CompraModel compra)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var culture = new CultureInfo("es-CR");
        var factura = compra.Facturas?.FirstOrDefault();

        // Calcular totales
        decimal subtotal = factura?.DetallesFactura?.Sum(d => d.Subtotal) ?? 0;
        decimal impuestos = factura?.DetallesFactura?.Sum(d => d.Impuestos) ?? 0;
        decimal totalSinDescuento = subtotal + impuestos;
        decimal porcentajeDescuento = factura?.Descuento?.PorcentajeDescuento ?? 0;
        decimal montoDescuento = porcentajeDescuento > 0 ? totalSinDescuento * porcentajeDescuento / 100 : 0;
        decimal totalConDescuento = totalSinDescuento - montoDescuento;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);

                // Encabezado
                page.Header().Column(header =>
                {
                    header.Item().AlignCenter().Text("Clínica Dental San Rafael")
                        .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                    header.Item().AlignCenter().Text("Factura Electrónica")
                        .FontSize(14).FontColor(Colors.Grey.Darken1);

                    header.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Blue.Medium);
                });

                // Contenido
                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Text($"Factura #: {factura?.IdFactura ?? compra.IdCompra}")
                        .FontSize(12).Bold();
                    col.Item().Text($"Fecha: {compra.FechaCompra:dd/MM/yyyy}");
                    col.Item().Text($"Cliente: {compra.Paciente?.Nombre} {compra.Paciente?.Apellidos}");
                    col.Item().Text($"Cédula: {compra.Paciente?.Usuario?.Cedula}");
                    col.Item().Text($"Correo: {compra.Paciente?.Usuario?.CorreoElectronico}");

                    col.Item().PaddingVertical(10);

                    if (factura != null)
                    {
                        col.Item().Text($"Método de Pago: {factura.MetodoPago?.Nombre ?? "No especificado"}");

                        if (factura.Descuento != null)
                        {
                            col.Item().Text($"Descuento aplicado: {factura.Descuento.PorcentajeDescuento.ToString("N2", culture)}% ({factura.Descuento.Codigo})");
                        }

                        col.Item().PaddingTop(15).Element(GenerateTablaDetalle(factura, culture));

                        col.Item().PaddingTop(20).AlignRight().Column(totals =>
                        {
                            totals.Item().Text($"Subtotal: ₡{subtotal.ToString("N2", culture)}").FontSize(12);
                            totals.Item().Text($"Impuestos: ₡{impuestos.ToString("N2", culture)}").FontSize(12);

                            if (porcentajeDescuento > 0)
                            {
                                totals.Item().Text($"Descuento ({porcentajeDescuento.ToString("N2", culture)}%): -₡{montoDescuento.ToString("N2", culture)}")
                                    .FontSize(12).FontColor(Colors.Red.Medium);
                                totals.Item().Text($"Total con descuento: ₡{totalConDescuento.ToString("N2", culture)}")
                                    .FontSize(14).Bold().FontColor(Colors.Green.Darken2);
                            }
                            else
                            {
                                totals.Item().Text($"Total: ₡{totalSinDescuento.ToString("N2", culture)}")
                                    .FontSize(14).Bold().FontColor(Colors.Green.Darken2);
                            }
                        });
                    }
                });

                // Pie de página
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span($"Clínica Dental San Rafael © {DateTime.Now.Year} - Gracias por su compra, por favor venga a nuestra clínica y presente su factura.")
                     .FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            });
        }).GeneratePdf();
    }

    private static Action<IContainer> GenerateTablaDetalle(FacturaModel factura, CultureInfo culture)
    {
        return container =>
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Tipo").FontColor(Colors.White).Bold();
                    header.Cell().Element(CellStyle).Text("Nombre").FontColor(Colors.White).Bold();
                    header.Cell().Element(CellStyle).Text("Cantidad").FontColor(Colors.White).Bold();
                    header.Cell().Element(CellStyle).Text("Subtotal").FontColor(Colors.White).Bold();
                    header.Cell().Element(CellStyle).Text("Impuestos").FontColor(Colors.White).Bold();
                    header.Cell().Element(CellStyle).Text("Total").FontColor(Colors.White).Bold();

                    IContainer CellStyle(IContainer container) =>
                        container.Background(Colors.Blue.Medium).Padding(5).AlignCenter();
                });

                foreach (var item in factura.DetallesFactura)
                {
                    var nombre = item.Tipo == "producto" ? item.Producto?.Nombre : item.Servicio?.Nombre;

                    table.Cell().Element(CellStyle).Text(item.Tipo);
                    table.Cell().Element(CellStyle).Text(nombre ?? "N/A");
                    table.Cell().Element(CellStyle).Text(item.Cantidad.ToString());
                    table.Cell().Element(CellStyle).Text($"₡{item.Subtotal.ToString("N2", culture)}");
                    table.Cell().Element(CellStyle).Text($"₡{item.Impuestos.ToString("N2", culture)}");
                    table.Cell().Element(CellStyle).Text($"₡{item.Total.ToString("N2", culture)}");

                    IContainer CellStyle(IContainer container) =>
                        container.Padding(5).AlignCenter();
                }
            });
        };
    }

    public static void EnviarFacturaPorCorreo(CompraModel compra, byte[] pdfFactura, EmailSettings emailSettings)
    {
        try
        {
            var paciente = compra.Paciente;
            var correoDestino = paciente?.Usuario?.CorreoElectronico;

            if (string.IsNullOrWhiteSpace(correoDestino))
                throw new ArgumentException("El correo del paciente no está definido.");

            using var message = new MailMessage();
            message.From = new MailAddress(emailSettings.From, "Clínica Dental San Rafael");
            message.To.Add(correoDestino);
            message.Subject = "Factura electrónica de tu compra";
            message.Body = $"Hola {paciente.Nombre},\n\nAdjunto encontrarás la factura de tu compra realizada en la Clínica Dental San Rafael.\n\n¡Gracias por tu preferencia!";

            message.Attachments.Add(new Attachment(new MemoryStream(pdfFactura), $"Factura_{compra.IdCompra}.pdf", "application/pdf"));

            using var smtpClient = new SmtpClient(emailSettings.SmtpServer)
            {
                Port = int.Parse(emailSettings.Port),
                Credentials = new NetworkCredential(emailSettings.Username, emailSettings.Password),
                EnableSsl = true
            };

            smtpClient.Send(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al enviar la factura por correo: {ex.Message}");
        }
    }
}
