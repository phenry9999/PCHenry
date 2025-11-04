using System;
using System.IO;
using System.Text;
using HtmlAgilityPack;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PdfToHtmlConverter
{
	public class PdfConverter
	{
		static void Main(string[] args)
		{
			var pdfPath = "c:\\temp\\AuditOfRevenueManagementAndCostRecovery2017.pdf";
			var pdfFileInfo = new FileInfo(pdfPath);

			var pdfConverter = new PdfToHtmlConverter();

			var html = pdfConverter.ConvertPdfToHtml(pdfFileInfo.FullName);

			// Write the HTML to a file
			var htmlPath = pdfFileInfo.FullName.Replace(".pdf", ".html");
			File.WriteAllText(htmlPath, html);

			Console.WriteLine("PDF converted to HTML successfully.");
		}
	}

	public class PdfToHtmlConverter
	{
		public string ConvertPdfToHtml(string pdfFilePath)
		{
			if (!File.Exists(pdfFilePath))
			{
				throw new FileNotFoundException("PDF file not found.", pdfFilePath);
			}

			var htmlBuilder = new StringBuilder();
			htmlBuilder.Append("<html><head><style>");
			htmlBuilder.Append("body { font-family: Arial, sans-serif; }");
			htmlBuilder.Append(".page { position: relative; margin-bottom: 20px; border: 1px solid #ccc; padding: 10px; }");
			htmlBuilder.Append(".text { position: absolute; white-space: pre; }");
			htmlBuilder.Append("</style></head><body>");

			using (var document = PdfDocument.Open(pdfFilePath))
			{
				foreach (var page in document.GetPages())
				{
					htmlBuilder.Append($"<div class=\"page\" style=\"width: {page.Width}px; height: {page.Height}px;\">");
					//htmlBuilder.Append($"<h2>Page {page.Number}</h2>");
					ExtractPageContent(page, htmlBuilder);
					htmlBuilder.Append("</div>" + Environment.NewLine);
				}
			}

			htmlBuilder.Append("</body></html>");

			return htmlBuilder.ToString();
		}

		private void ExtractPageContent(Page page, StringBuilder htmlBuilder)
		{
			//htmlBuilder.Append($"{page.GetWords()}{Environment.NewLine}");

			foreach (var word in page.GetWords())
			{
				var left = word.BoundingBox.Left;
				var top = page.Height - word.BoundingBox.Top;
				var fontSize = word.BoundingBox.Height;
				var fontFamily = word.FontName;

				var style = $"left: {left}px; top: {top}px; font-size: 10px;";
				htmlBuilder.Append($"<span class=\"text\" style=\"{style}\">{word.Text}</span>" + Environment.NewLine);
				//htmlBuilder.Append($"{word.Text}{Environment.NewLine}");
			}

		}
	}
}