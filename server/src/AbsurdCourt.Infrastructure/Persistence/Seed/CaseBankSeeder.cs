using AbsurdCourt.Domain.Matches;
using Microsoft.EntityFrameworkCore;

namespace AbsurdCourt.Infrastructure.Persistence.Seed;

public static class CaseBankSeeder
{
    public static async Task SeedAsync(CourtDbContext db, CancellationToken ct = default)
    {
        if (await db.CaseFiles.AnyAsync(ct)) return;

        db.CaseFiles.AddRange(Cases.Select(c => new CaseFile(Guid.NewGuid(), c.Autos, c.Hint)));
        await db.SaveChangesAsync(ct);
    }

    /// <summary>The first three are the prototype's canonical examples, verbatim; the rest extend the same absurd-bureaucratic tone so a match doesn't repeat cases too soon.</summary>
    private static readonly (string Hint, string Autos)[] Cases =
    [
        ("Flagrante noturno", "A parte é acusada de levar um flamingo de plástico para dentro de uma agência bancária às 3h14 da manhã e apresentá-lo como fiador."),
        ("Testemunha alada", "A parte é acusada de ensinar o papagaio do vizinho a prestar falso testemunho em audiência pública."),
        ("Comércio circular", "A parte é acusada de vender o mesmo guarda-chuva quatorze vezes à mesma pessoa, sempre em dias de sol."),
        ("Motim doméstico", "A parte é acusada de organizar um motim silencioso entre os eletrodomésticos da própria casa, incitando a geladeira e o micro-ondas contra o forno."),
        ("Contrabando afetivo", "A parte é acusada de importar clandestinamente uma saudade de tamanho industrial de outro estado, sem declará-la na alfândega emocional do relacionamento."),
        ("Falsificação astral", "A parte é acusada de forjar a assinatura do próprio horóscopo para justificar três atrasos consecutivos ao trabalho, alegando 'Mercúrio retrógrado'."),
        ("Perturbação cósmica", "A parte é acusada de interromper a assembleia de condomínio gritando sobre alinhamentos planetários sem apresentar um único mapa astral como prova."),
        ("Fraude gastronômica", "A parte é acusada de servir água com gás como 'espumante importado' na festa de aniversário de um gato chamado Aristóteles."),
        ("Cativeiro têxtil", "A parte é acusada de manter vinte e três meias solteiras em cativeiro doméstico, sem jamais devolver o par correspondente à sociedade."),
        ("Anexação territorial", "A parte é acusada de declarar o controle remoto da televisão território soberano e cobrar pedágio dos demais moradores da casa para trocar de canal."),
        ("Estelionato fitness", "A parte é acusada de se matricular em uma academia e declarar sob juramento, por quatorze meses consecutivos, que 'ia amanhã'."),
        ("Difamação de pombo", "A parte é acusada de espalhar o boato de que um pombo específico da praça central trabalha disfarçado para órgãos governamentais."),
        ("Abandono vegetal", "A parte é acusada de negligência grave contra uma samambaia doméstica, alegando força maior após quarenta dias sem regá-la."),
        ("Crachá apócrifo", "A parte é acusada de portar um crachá de 'Funcionário do Mês' impresso em casa, sem jamais ter sido funcionária daquela ou de qualquer outra empresa."),
        ("Violação de fila", "A parte é acusada de furar a fila da padaria alegando ser descendente direta do primeiro cliente do estabelecimento, fundado em 1987."),
    ];
}
