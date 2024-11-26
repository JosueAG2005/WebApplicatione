using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Canvas;
using iText.IO.Image;
using iText.Kernel.Font;
using WebApplication9.Models;

namespace WebApplication9.Controllers
{
    public class ConcursoesController : Controller
    {
        private readonly SistemaDeConcursosContext _context;

        public ConcursoesController(SistemaDeConcursosContext context)
        {
            _context = context;
        }

        // GET: Concursoes
        public async Task<IActionResult> Index()
        {
            var sistemaDeConcursosContext = _context.Concursos.Include(c => c.IdCategoriaNavigation).Include(c => c.IdTipoConcursoNavigation);
            return View(await sistemaDeConcursosContext.ToListAsync());
        }

        // Nueva acción para imprimir un PDF con el logo desde la URL
        public IActionResult ImprimirPDF()
        {
            // Crea un stream para el archivo PDF
            MemoryStream stream = new MemoryStream();
            PdfWriter writer = new PdfWriter(stream);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            // URL de la imagen del logo
            string imageUrl = "https://qubitoz.com/web/wp-content/uploads/bfi_thumb/logo_concursos_18-ku4gqdofzhr9hk6ftgcr2obs0u9xw1f3mg2nykahdc.png";

            try
            {
                // Descarga la imagen desde la URL
                using (WebClient webClient = new WebClient())
                {
                    byte[] imageBytes = webClient.DownloadData(imageUrl);
                    ImageData imageData = ImageDataFactory.Create(imageBytes);
                    Image logo = new Image(imageData);
                    logo.SetWidth(150); // Ajusta el tamaño del logo
                    logo.SetHorizontalAlignment(HorizontalAlignment.CENTER); // Centra el logo en el PDF
                    document.Add(logo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar la imagen desde la URL: {ex.Message}");
            }

            // Agrega un título
            document.Add(new Paragraph("Lista de Concursos")
                .SetFontSize(18)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            // Obtiene los concursos desde la base de datos
            var concursos = _context.Concursos
                .Include(c => c.IdCategoriaNavigation)
                .Include(c => c.IdTipoConcursoNavigation)
                .ToList();

            // Agrega información de cada concurso al PDF
            foreach (var concurso in concursos)
            {
                document.Add(new Paragraph($"Nombre: {concurso.Nombre}")
                    .SetFontSize(12)
                    .SetBold());
                document.Add(new Paragraph($"Categoría: {concurso.IdCategoriaNavigation?.NombreCategoria}"));
                document.Add(new Paragraph($"Tipo: {concurso.IdTipoConcursoNavigation?.NombreTipo}"));
                document.Add(new Paragraph($"Fecha de Inicio: {concurso.FechaInicio.ToShortDateString()}"));
                document.Add(new Paragraph($"Fecha de Fin: {concurso.FechaFin.ToShortDateString()}"));
                document.Add(new Paragraph($"Descripción: {concurso.Descripcion}"));
                document.Add(new Paragraph("-------------------------------"));
            }

            // Marca de agua
            PdfFont font = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                PdfCanvas pdfCanvas = new PdfCanvas(pdf.GetPage(i));
                pdfCanvas.SaveState();
                pdfCanvas.BeginText();
                pdfCanvas.SetFontAndSize(font, 50);
                pdfCanvas.SetColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, true);
                pdfCanvas.MoveText(298, 421);
                pdfCanvas.ShowText("Marca de Agua");
                pdfCanvas.EndText();
                pdfCanvas.RestoreState();
            }

            // Cierra el documento
            document.Close();

            // Configura el archivo para su descarga
            return File(stream.ToArray(), "application/pdf", "ListaDeConcursos.pdf");
        }

        // Otros métodos del controlador (Details, Create, Edit, etc.)
        private bool ConcursoExists(int id)
        {
            return _context.Concursos.Any(e => e.IdConcurso == id);
        }
    }
}
