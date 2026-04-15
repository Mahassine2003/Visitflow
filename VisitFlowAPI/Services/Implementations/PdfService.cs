using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VisitFlowAPI.Data;
using VisitFlowAPI.Services.Interfaces;

namespace VisitFlowAPI.Services.Implementations;

public class PdfService : IPdfService
{
    private readonly VisitFlowDbContext _db;
    private readonly IConfiguration _configuration;

    public PdfService(VisitFlowDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<string> GenerateBlacklistPdfAsync()
    {
        var personnel = await _db.Personnels
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Where(p => p.IsBlacklisted)
            .OrderBy(p => p.Supplier.CompanyName)
            .ThenBy(p => p.FullName)
            .ToListAsync();

        var basePath = _configuration.GetSection("Pdf")["BaseOutputPath"] ?? "C:\\VisitFlow\\Pdf";
        Directory.CreateDirectory(basePath);

        var fileName = $"Blacklist_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var fullPath = Path.Combine(basePath, fileName);

        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Margin(40);
                page.Header().Column(h =>
                {
                    h.Spacing(4);
                    h.Item().Text("VisitFlow — Liste du personnel blacklisté").FontSize(16).SemiBold();
                    h.Item().Text($"Généré le {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC").FontSize(10);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(6);

                    if (!personnel.Any())
                    {
                        col.Item().Text("Aucun personnel blacklisté pour le moment.").FontSize(11);
                        return;
                    }

                    foreach (var p in personnel)
                    {
                        col.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(4).Text(text =>
                        {
                            text.Span(p.FullName).SemiBold().FontSize(11);
                            text.Span($" — CIN: {p.Cin}").FontSize(10);
                        });
                        col.Item().Text($"Fournisseur : {p.Supplier.CompanyName}").FontSize(10);
                        col.Item().Text($"Fonction : {p.Position} / {p.JobTitle}").FontSize(10);
                        col.Item().PaddingBottom(4);
                    }
                });

                page.Footer().AlignRight().Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                    t.Span("Document généré automatiquement par VisitFlow");
                });
            });
        }).GeneratePdf(fullPath);

        return fullPath;
    }

    public async Task<string> GenerateInterventionPdfAsync(int interventionId)
    {
        var intervention = await _db.Interventions
            .AsNoTracking()
            .Include(i => i.Supplier)
            .Include(i => i.TypeOfWork)
            .Include(i => i.Visit)
            .FirstOrDefaultAsync(i => i.Id == interventionId);
        if (intervention is null)
        {
            throw new InvalidOperationException("Intervention not found");
        }

        var basePath = _configuration.GetSection("Pdf")["BaseOutputPath"] ?? "C:\\VisitFlow\\Pdf";
        Directory.CreateDirectory(basePath);

        var fileName = $"Intervention_{intervention.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var fullPath = Path.Combine(basePath, fileName);

        var zones = await _db.InterventionZones
            .Where(z => z.InterventionId == interventionId)
            .Join(_db.Zones, iz => iz.ZoneId, z => z.Id, (iz, z) => z.Name)
            .ToListAsync();

        var hseValidated = intervention.IsHSEValidated;
        var totalPages = hseValidated ? 4 : 3;

        Document.Create(document =>
        {
            document.Page(page => BuildPage(
                page,
                pageIndex: 1,
                totalPages,
                sectionTitle: "1 — Informations générales",
                hseValidated,
                col =>
                {
                    col.Spacing(10);
                    col.Item().Text($"Titre : {intervention.Title}").FontSize(11);
                    col.Item().Text($"Description : {intervention.Description}").FontSize(11);
                    col.Item().Text($"Fournisseur : {intervention.Supplier.CompanyName}").FontSize(11);
                    col.Item().Text($"Type de travail : {intervention.TypeOfWork.Name}").FontSize(11);
                    col.Item().Text($"Référence visite : {intervention.Visit.VisitId}").FontSize(11);
                    col.Item().Text($"Créé par : {intervention.CreatedBy}").FontSize(11);
                    col.Item().Text($"Période : {intervention.StartDate:dd/MM/yyyy HH:mm} → {intervention.EndDate:dd/MM/yyyy HH:mm}")
                        .FontSize(11);
                    col.Item().Text($"Statut intervention : {intervention.Status}").FontSize(11);
                    col.Item().Text($"PPI : {(string.IsNullOrWhiteSpace(intervention.Ppi) ? "—" : intervention.Ppi)}").FontSize(11);
                    col.Item()
                        .Text($"Personnel min. / Zones min. : {intervention.MinPersonnel} / {intervention.MinZone}")
                        .FontSize(11);
                    col.Item()
                        .Text($"Zones : {(zones.Count == 0 ? "—" : string.Join(", ", zones))}")
                        .FontSize(11);
                }));

            document.Page(page => BuildPage(
                page,
                pageIndex: 2,
                totalPages,
                sectionTitle: "2 — Fire permit",
                hseValidated,
                col =>
                {
                    col.Spacing(8);
                    var body = string.IsNullOrWhiteSpace(intervention.FirePermitDetails)
                        ? "(Non renseigné)"
                        : intervention.FirePermitDetails!;
                    col.Item().Text(body).FontSize(11);
                }));

            document.Page(page => BuildPage(
                page,
                pageIndex: 3,
                totalPages,
                sectionTitle: "3 — Height permit",
                hseValidated,
                col =>
                {
                    col.Spacing(8);
                    var body = string.IsNullOrWhiteSpace(intervention.HeightPermitDetails)
                        ? "(Non renseigné)"
                        : intervention.HeightPermitDetails!;
                    col.Item().Text(body).FontSize(11);
                }));

            if (hseValidated)
            {
                document.Page(page => BuildPage(
                    page,
                    pageIndex: 4,
                    totalPages,
                    sectionTitle: "4 — HSE",
                    hseValidated,
                    col =>
                    {
                        col.Spacing(8);
                        var form = string.IsNullOrWhiteSpace(intervention.HseFormDetails)
                            ? "(Formulaire HSE non renseigné)"
                            : intervention.HseFormDetails!;
                        col.Item().Text(form).FontSize(11);
                        if (!string.IsNullOrWhiteSpace(intervention.HSEComment))
                        {
                            col.Item().PaddingTop(12);
                            col.Item().Text("Commentaire HSE").FontSize(12).SemiBold();
                            col.Item().Text(intervention.HSEComment).FontSize(11);
                        }
                    }));
            }
        }).GeneratePdf(fullPath);

        return fullPath;
    }

    private static void BuildPage(
        PageDescriptor page,
        int pageIndex,
        int totalPages,
        string sectionTitle,
        bool hseValidated,
        Action<ColumnDescriptor> content)
    {
        page.Margin(40);
        page.Header().Column(h =>
        {
            h.Spacing(4);
            h.Item().Text("VisitFlow — Fiche intervention").FontSize(16).SemiBold();
            h.Item().Text(sectionTitle).FontSize(13).SemiBold().FontColor(Colors.Blue.Medium);
        });

        page.Content().Column(col =>
        {
            col.Spacing(6);
            if (!hseValidated && pageIndex <= 3)
            {
                col.Item()
                    .Background(Colors.Grey.Lighten3)
                    .Padding(8)
                    .Text(
                        "Avant validation HSE : ce document comporte 3 pages (infos générales, fire permit, height permit). La page HSE sera ajoutée après validation.")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken2);
            }

            content(col);
        });

        page.Footer().Row(row =>
        {
            row.RelativeItem().AlignLeft().Text(t =>
            {
                t.DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium));
                if (!hseValidated)
                    t.Span("Version sans validation HSE — ");
                t.Span($"Généré le {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC");
            });
            row.ConstantItem(120).AlignRight().Text($"Page {pageIndex} / {totalPages}").FontSize(9).FontColor(Colors.Grey.Darken2);
        });
    }
}
