using AbsurdCourt.Domain.Matches;
using Microsoft.EntityFrameworkCore;

namespace AbsurdCourt.Infrastructure.Persistence.Seed;

public static class CaseBankSeeder
{
    public static async Task SeedAsync(CourtDbContext db, CancellationToken ct = default)
    {
        var existingAutos = await db.CaseFiles
            .Select(caseFile => caseFile.Autos)
            .ToHashSetAsync(ct);
        var missingCases = Cases
            .Where(caseFile => !existingAutos.Contains(caseFile.Autos))
            .Select(caseFile => new CaseFile(Guid.NewGuid(), caseFile.Autos, caseFile.Hint))
            .ToList();
        if (missingCases.Count == 0) return;

        db.CaseFiles.AddRange(missingCases);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>A diverse catalogue keeps independent matches from recycling the same absurd premise too soon.</summary>
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
        ("Sonegação de migalhas", "Consta nos autos que a parte escondeu migalhas de pão no bolso para alegar, perante os pombos da praça, inexistência de patrimônio alimentar."),
        ("Usucapião de tomada", "O Ministério do Cotidiano sustenta que a parte ocupou a única tomada livre da sala por tempo suficiente para declará-la bem de família."),
        ("Lavagem de louça", "Segundo denúncia anônima, a parte lavou apenas o lado visível dos pratos e tentou apresentar a pia como prova de honestidade doméstica."),
        ("Desvio de rota", "Apura-se que a parte pediu ao aplicativo de mapas um caminho mais longo exclusivamente para terminar uma conversa constrangedora no carro."),
        ("Furto de feriado", "A parte responde por anunciar numa segunda-feira comum que era feriado municipal, sem indicar município, santo padroeiro ou respaldo constitucional."),
        ("Sabotagem cafeeira", "Relatório da copa informa que a parte substituiu o café por cevada e chamou a experiência de 'reestruturação sensorial'."),
        ("Impostura felina", "A acusação afirma que a parte assinou uma encomenda usando a pata do gato, que posteriormente se recusou a prestar esclarecimentos."),
        ("Evasão de reunião", "Há indícios de que a parte manteve a câmera desligada numa videoconferência e respondeu 'concordo' a intervalos calculados, sem saber o assunto em pauta."),
        ("Tumulto bibliográfico", "Foi lavrado termo contra a parte por devolver um livro com quarenta e duas abas adesivas e alegar que eram melhorias de navegação."),
        ("Contravenção térmica", "A parte é investigada por abrir a janela do ônibus em pleno ar-condicionado e justificar o ato como consulta pública sobre correntes de ar."),
        ("Receptação de planta", "Segundo os autos, a parte adotou uma planta deixada no corredor e passou a exigir pensão dos vizinhos em troca de regá-la."),
        ("Fraude de senha", "A corregedoria apura o uso da senha 'senha123' seguido da alegação de que a previsibilidade era uma camada avançada de segurança."),
        ("Obstrução de elevador", "A parte teria segurado a porta do elevador por sete andares para concluir uma história que não possuía desfecho verificável."),
        ("Diplomacia de condomínio", "Registra-se que a parte enviou uma nota diplomática ao apartamento 402 solicitando cessar-fogo imediato na disputa pelo varal coletivo."),
        ("Perícia sonora", "A parte foi surpreendida medindo, com fita métrica, o volume da música do vizinho e juntando o resultado aos autos como prova técnica."),
        ("Clonagem de cupom", "A denúncia descreve a reutilização de um cupom fiscal como marcador de livro, comprovante de presença e documento de identidade provisório."),
        ("Omissão culinária", "O laudo aponta que a parte prometeu trazer sobremesa para o almoço e apresentou, como alternativa razoável, uma bala de hortelã encontrada na bolsa."),
        ("Desacato à impressora", "Testemunhas relatam que a parte chamou a impressora de 'tirana de cartucho' diante de três estagiários e uma folha A4 ainda em branco."),
        ("Patrimônio invisível", "A parte reivindica a propriedade exclusiva de uma vaga de estacionamento que só existe quando ninguém está olhando para ela."),
        ("Adulteração meteorológica", "Em boletim informal, a parte culpou a previsão do tempo por levar guarda-chuva num dia seco e exigiu retratação das nuvens."),
        ("Coação recreativa", "Foi instaurado inquérito porque a parte obrigou os convidados a participar de um jogo de tabuleiro cujas regras ela inventava a cada derrota."),
        ("Desvio de correspondência", "A parte é suspeita de guardar convites de casamento alheios por meses para responder 'acho que não vou conseguir ir' com antecedência dramática."),
        ("Anistia capilar", "Conforme relato do salão, a parte pediu corte de emergência e declarou que a franja anterior estava sob proteção de programa de testemunhas."),
        ("Posse de ventilador", "A acusação sustenta que a parte girou o ventilador para si durante uma reunião e chamou a medida de redistribuição climática."),
        ("Ofensa ao calendário", "Há registro de que a parte riscou uma terça-feira inteira da agenda por considerá-la insuficientemente inspiradora para existir."),
        ("Burocracia marítima", "A parte tentou protocolar uma reclamação formal contra a maré por ter molhado seus sapatos antes do horário comercial."),
        ("Apropriação de playlist", "Ocorre que a parte declarou ser autora de uma playlist colaborativa após adicionar uma única canção de cinco minutos e vinte e um segundos."),
        ("Crime de etiqueta", "A parte compareceu a um jantar temático vestida de 'traje esporte fino' e apresentou um agasalho de time como jurisprudência favorável."),
        ("Fuga do despertador", "Segundo perícia doméstica, a parte programou seis alarmes, ignorou todos e acusou o último de conspiração contra sua carreira."),
        ("Invasão aromática", "A parte é responsabilizada por acender uma vela com cheiro de chuva na sala de reuniões e provocar pânico entre os que tinham compromissos externos."),
        ("Suborno de mascote", "Consta que a parte ofereceu biscoitos ao cachorro do porteiro para obter acesso antecipado às fofocas do prédio."),
        ("Litígio de travesseiro", "Foi apresentada petição alegando que o travesseiro da parte ocupava metade da cama por direito histórico e necessidade cervical."),
        ("Extorsão de Wi-Fi", "A parte teria alterado a senha da internet e condicionado sua revelação à aprovação unânime de sua escolha de filme."),
        ("Dano à reputação lunar", "Relatório noturno registra que a parte acusou a lua cheia de interferir em sua produtividade e solicitou indenização por claridade excessiva."),
        ("Receita apócrifa", "A parte é acusada de atribuir à avó uma receita de macarrão instantâneo para encerrar discussões culinárias sem apresentar certidão de autoria."),
        ("Supressão de soneca", "Há fortes indícios de que a parte apertou o botão soneca no despertador alheio e depois alegou atuação em legítima defesa do sono coletivo."),
        ("Licenciamento de piada", "A corregedoria recebeu notícia de que a parte cobrou royalties por uma piada contada no churrasco, embora ela mesma a tivesse esquecido pela metade."),
        ("Rebelião de post-its", "A parte responde por colar tantos lembretes na mesa que um relatório oficial classificou o ambiente como arquivo em estado de insurreição."),
        ("Vistoria de geladeira", "Foi aberto procedimento após a parte etiquetar alimentos da geladeira compartilhada com datas de validade inventadas e níveis de ameaça."),
        ("Desobediência semáforica", "A parte é investigada por agradecer em voz alta ao semáforo verde e ofender o vermelho, perturbando a neutralidade do trânsito."),
        ("Falsidade botânica", "Segundo a denúncia, a parte apresentou um cacto de plástico como herdeiro legítimo de uma coleção de plantas para escapar da obrigação de regá-las."),
        ("Insubordinação algorítmica", "A parte contestou a recomendação de uma rede social, curtiu o próprio protesto e declarou vitória sobre o sistema de sugestões."),
        ("Peculato de guardanapo", "Em restaurante por quilo, a parte levou dezenove guardanapos alegando que estavam incluídos no preço e constituíam reserva estratégica."),
        ("Atentado ao silêncio", "A parte foi vista mastigando salgadinhos crocantes durante uma sessão de meditação guiada e chamando o ruído de contribuição percussiva."),
        ("Contrabando de nostalgia", "A parte tentou passar uma fita cassete pela segurança do aeroporto afirmando que a memória afetiva não se submete a raio-X."),
        ("Imunidade de chinelo", "A defesa sustenta que a parte compareceu a uma chamada de vídeo em chinelos por estar sob jurisdição exclusiva da própria sala."),
        ("Sumiço de caneta", "Apura-se o desaparecimento de uma caneta azul que a parte emprestou, cobrou três vezes e depois encontrou atrás da própria orelha."),
        ("Fraude de estacionamento", "A parte estacionou a bicicleta numa vaga de carro e registrou o feito como eficiência urbana de alta densidade."),
        ("Retenção de troco", "Segundo o caixa, a parte guardou cinco centavos de troco por vinte minutos enquanto avaliava se cabia recurso administrativo."),
        ("Cerimônia de micro-ondas", "A parte foi denunciada por bater palmas quando o micro-ondas apitou e exigir que os presentes respeitassem o encerramento do aquecimento."),
        ("Conspiração de cabide", "A ocorrência relata que a parte culpou cabides por amassarem suas roupas e abriu investigação doméstica sem ouvir o armário."),
        ("Desfalque de gelo", "Em reunião familiar, a parte esvaziou a forma de gelo para resfriar refrigerante próprio e classificou a ação como empréstimo térmico."),
        ("Desvio de sobremesa", "A parte teria escondido a última fatia de bolo atrás de vegetais congelados, confiando que ninguém procuraria alimento naquele setor."),
        ("Sinalização indevida", "Há indícios de que a parte colocou um cone de trânsito diante da porta do quarto para declarar horário de manutenção pessoal."),
        ("Coleta seletiva", "A parte separou o lixo por signos zodiacais e se recusou a entregar o saco de Virgem antes de consultar o horóscopo."),
        ("Embargo de sofá", "A parte interditou o sofá da sala por obras imaginárias e transferiu os moradores para cadeiras de plástico sem aviso prévio."),
        ("Pirataria de receita", "A acusação aponta que a parte copiou uma receita de família e atribuiu a autoria a um chef fictício chamado Ernesto Panela."),
        ("Voto de silêncio", "Durante discussão sobre louça, a parte declarou voto de silêncio seletivo e respondeu apenas às perguntas que lhe favoreciam."),
        ("Confisco de controle", "Foi instaurado procedimento porque a parte escondeu as pilhas do controle remoto e passou a negociar canais por ordem de preferência."),
        ("Fabricação de urgência", "A parte marcou uma tarefa como urgente, importantíssima e apocalíptica para conseguir que alguém respondesse uma mensagem de bom dia."),
        ("Acordo de elevador", "A parte propôs dividir o elevador em zona social e zona de contemplação, cobrando silêncio de quem apertasse o térreo."),
        ("Greve de talheres", "Segundo o boletim da cozinha, a parte declarou greve de garfos até que o restante da casa reconhecesse sua contribuição no almoço."),
        ("Ocupação de sombra", "A parte fincou uma toalha na única sombra da piscina e alegou possuir título de propriedade emitido pelo guarda-sol."),
        ("Desvio de assunto", "Em audiência doméstica, a parte respondeu a uma cobrança sobre contas com uma palestra de quarenta minutos sobre dinossauros."),
        ("Peculato de guardachuva", "A parte recolheu guarda-chuvas esquecidos no escritório e passou a alugá-los aos proprietários em dias de chuva."),
        ("Objeção ao espelho", "A parte apresentou reclamação contra o espelho do elevador por refletir uma expressão que, segundo ela, não havia autorizado."),
        ("Disputa de tomada", "Relatório técnico acusa a parte de desconectar o carregador alheio aos 99% para sustentar prioridade absoluta do próprio aparelho."),
        ("Alarme cerimonial", "A parte programou uma música épica como alarme e exigiu que todos permanecessem imóveis até o refrão terminar."),
        ("Subtração de senha", "A parte mudou a senha do streaming para uma charada de doze etapas e chamou a medida de alfabetização cultural."),
        ("Falso ferimento", "Testemunhas afirmam que a parte usou um curativo no dedo mínimo para escapar de lavar uma panela sem antiaderente."),
        ("Protocolo de fila", "A parte distribuiu senhas para usar o banheiro durante uma festa e manteve um painel de chamados escrito à mão."),
        ("Sonegação de elogio", "Consta que a parte elogiou uma receita apenas depois de descobrir que outra pessoa a havia preparado, provocando crise diplomática."),
        ("Falsificação de pausa", "A parte deixou uma apresentação em tela cheia para parecer ocupada durante uma hora e chamou a ausência de produtividade de estratégia visual."),
        ("Tratado de travesseiros", "A parte assinou um tratado de não agressão com o companheiro de quarto para impedir o avanço territorial de almofadas."),
        ("Jurisdição de varanda", "A parte proibiu o vizinho de regar plantas após as 17h alegando que a varanda possuía fuso horário próprio."),
        ("Apropriação de fila", "Foi juntado aos autos que a parte guardou lugar numa fila para seis pessoas e apresentou guarda-chuvas como testemunhas."),
        ("Censura de legenda", "A parte silenciou a televisão durante um filme legendado para impedir que os demais descobrissem o final antes dela."),
        ("Infração de etiqueta", "A parte colocou etiqueta com seu nome em um pote vazio e passou a chamar qualquer alimento nele guardado de patrimônio pessoal."),
        ("Inspeção de biscoito", "A parte abriu todos os pacotes de biscoito do mercado para comparar crocância e alegou estar exercendo controle de qualidade cidadão."),
        ("Habeas corpus felino", "A parte peticionou pela liberdade imediata do gato preso no banheiro, embora o animal tivesse entrado voluntariamente para dormir."),
        ("Selo de aprovação", "A parte carimbou documentos domésticos com um desenho de batata e declarou que o símbolo tinha validade protocolar."),
        ("Negligência de agenda", "A parte ignorou um lembrete de aniversário e alegou que o calendário havia sido excessivamente insistente e, portanto, suspeito."),
        ("Fuga de carrinho", "No supermercado, a parte abandonou o carrinho no corredor de massas e deixou uma lista de compras como declaração de intenções."),
        ("Inquérito de chinelo", "A parte organizou uma busca pela origem de um chinelo perdido e interrogou todos os moradores, inclusive os que estavam descalços."),
        ("Desrespeito ao tostador", "A parte retirou o pão antes do fim do ciclo e acusou a torradeira de lentidão deliberada perante o café da manhã."),
    ];
}
