// 1. Seleciona e Limpa o Banco
// use hospitalDB;
db.dropDatabase();

// --- FUNÇÕES AUXILIARES ---
function gerarUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function itemAleatorio(array) {
    return array[Math.floor(Math.random() * array.length)];
}

function numeroAleatorio(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

// --- DADOS PARA GERAÇÃO ALEATÓRIA ---
const nomes = ["Ana", "Bruno", "Carlos", "Daniela", "Eduardo", "Fernanda", "Gabriel", "Helena", "Igor", "Julia", "Lucas", "Mariana", "Nicolas", "Olivia", "Pedro", "Rafaela", "Samuel", "Tatiane", "Vitor", "Yasmin"];
const sobrenomes = ["Silva", "Santos", "Oliveira", "Souza", "Rodrigues", "Ferreira", "Alves", "Pereira", "Lima", "Gomes", "Costa", "Ribeiro", "Martins", "Carvalho", "Almeida"];
const alimentos = ["Arroz", "Feijão", "Frango", "Peixe", "Carne Moída", "Purê", "Salada", "Sopa", "Gelatina", "Fruta", "Legumes", "Batata Doce", "Ovos", "Iogurte"];
const tiposDieta = ["Geral", "Hipossódica", "Diabética", "Pastosa", "Líquida", "Branda", "Sem Glúten", "Hipercalórica"];

// Arrays para armazenar os IDs gerados (para criar relacionamentos)
var idsDietas = [];
var idsCopeiras = [];
var idsBandejas = [];
var idsPacientes = [];

// ============================================================
// 1. GERAR 150 DIETAS
// ============================================================
print("Gerando 150 Dietas...");
for (var i = 0; i < 150; i++) {
    var id = gerarUUID();
    idsDietas.push(id);
    
    var nomeDieta = itemAleatorio(tiposDieta) + " - Opção " + (i + 1);
    // Gera uma lista de 3 a 5 alimentos
    var qtdItens = numeroAleatorio(3, 5);
    var listaItens = [];
    for(var j=0; j<qtdItens; j++) { listaItens.push(itemAleatorio(alimentos)); }

    db.Dietas.insertOne({
        _id: id,
        NomeDieta: nomeDieta,
        ItensAlimentares: listaItens,
        Ativo: true
    });
}

// ============================================================
// 2. GERAR 150 COPEIRAS
// ============================================================
print("Gerando 150 Copeiras...");
for (var i = 0; i < 150; i++) {
    var id = gerarUUID();
    idsCopeiras.push(id);
    
    var nomeCompleto = itemAleatorio(nomes) + " " + itemAleatorio(sobrenomes);

    db.Copeiras.insertOne({
        _id: id,
        Nome: nomeCompleto,
        Ativo: true
    });
}

// ============================================================
// 3. GERAR 150 BANDEJAS
// ============================================================
print("Gerando 150 Bandejas...");
for (var i = 0; i < 150; i++) {
    var id = gerarUUID();
    idsBandejas.push(id);
    
    // Vincula a uma dieta aleatória
    var idDietaVinculada = itemAleatorio(idsDietas); 
    var codigo = "BDJ-" + (1000 + i);

    db.Bandejas.insertOne({
        _id: id,
        CodBandeja: codigo,
        TipoDieta: idDietaVinculada,
        Ativo: true
    });
}

// ============================================================
// 4. GERAR 150 PACIENTES
// ============================================================
print("Gerando 150 Pacientes...");
for (var i = 0; i < 150; i++) {
    var id = gerarUUID();
    idsPacientes.push(id);
    
    var nomeCompleto = itemAleatorio(nomes) + " " + itemAleatorio(sobrenomes);
    var idDietaVinculada = itemAleatorio(idsDietas); 

    db.Pacientes.insertOne({
        _id: id,
        Nome: nomeCompleto,
        NumQuarto: numeroAleatorio(100, 500),
        CodPulseira: "P-" + (5000 + i),
        IdDieta: idDietaVinculada,
        Ativo: true
    });
}

// ============================================================
// 5. GERAR 150 ENTREGAS
// ============================================================
print("Gerando 150 Entregas...");
for (var i = 0; i < 150; i++) {
    var id = gerarUUID();
    
    var idPaciente = itemAleatorio(idsPacientes);
    var idCopeira = itemAleatorio(idsCopeiras);
    var idBandeja = itemAleatorio(idsBandejas);

    // Lógica de Status
    var statusPossiveis = ["Concluído", "Concluído", "Concluído", "Em andamento", "Erro"]; // Mais peso para Concluído
    var statusEscolhido = itemAleatorio(statusPossiveis);
    
    // Datas
    var dataInicio = new Date();
    // Espalha as entregas nos últimos 30 dias
    dataInicio.setDate(dataInicio.getDate() - numeroAleatorio(0, 30)); 
    dataInicio.setHours(numeroAleatorio(7, 20), numeroAleatorio(0, 59), 0);
    
    var dataFim = null;
    var obs = "";

    if (statusEscolhido === "Concluído") {
        dataFim = new Date(dataInicio);
        dataFim.setMinutes(dataInicio.getMinutes() + numeroAleatorio(5, 25)); // Durou entre 5 a 25 min
        obs = "Entrega realizada com sucesso.";
    } else if (statusEscolhido === "Erro") {
        dataFim = new Date(dataInicio);
        dataFim.setMinutes(dataInicio.getMinutes() + numeroAleatorio(2, 10));
        obs = "Paciente recusou a refeição.";
    } else {
        // Em andamento (HoraFim deve ser nula)
        dataFim = null;
        obs = "Aguardando retorno.";
    }

    db.Entregas.insertOne({
        _id: id,
        IdPaciente: idPaciente,
        IdCopeira: idCopeira,
        IdBandeja: idBandeja,
        HoraInicio: dataInicio,
        HoraFim: dataFim,
        StatusValidacao: statusEscolhido,
        Observacao: obs
    });
}

print("==========================================");
print("Banco hospitalDB pronto.");
print("==========================================");

// Script dos Índices
// use hospitalDB; sempre deixamos comentado esse comando pois o vs code não identifica o comando

print("--- Iniciando Criação de Índices ---");

// ============================================================
// 1. COLEÇÃO: PACIENTES
// ============================================================
// Motivo: Usado na barra de busca (SearchString) e ordenação
db.Pacientes.createIndex({ "Nome": 1 }); 

// Motivo: Usado em quase todas as consultas do Controller (Filtro Lógico)
db.Pacientes.createIndex({ "Ativo": 1 });

// Motivo: Otimizar o $lookup que busca a Dieta do Paciente
db.Pacientes.createIndex({ "IdDieta": 1 });

// Motivo: (Opcional) Garantir que o código da pulseira seja único e rápido de achar
db.Pacientes.createIndex({ "CodPulseira": 1 });

print("Índices de Pacientes criados.");

// ============================================================
// 2. COLEÇÃO: DIETAS
// ============================================================
// Motivo: Usado para carregar os Dropdowns ordenados por nome
// Dica: Índice composto (filtra ativos E ordena por nome numa só passada)
db.Dietas.createIndex({ "Ativo": 1, "NomeDieta": 1 });

print("Índices de Dietas criados.");

// ============================================================
// 3. COLEÇÃO: COPEIRAS
// ============================================================
// Motivo: Usado para carregar os Dropdowns ordenados por nome e filtrados por ativo
db.Copeiras.createIndex({ "Ativo": 1, "Nome": 1 });

print("Índices de Copeiras criados.");

// ============================================================
// 4. COLEÇÃO: BANDEJAS
// ============================================================
// Motivo: Buscar rapidamente pelo código (ex: leitura de QR Code futuro)
db.Bandejas.createIndex({ "CodBandeja": 1 }, { unique: true });

// Motivo: Otimizar buscas de bandejas ativas
db.Bandejas.createIndex({ "Ativo": 1 });

print("Índices de Bandejas criados.");

// ============================================================
// 5. COLEÇÃO: ENTREGAS (A mais pesada)
// ============================================================
// Motivo: CRÍTICO para o "Histórico do Paciente".
// Permite achar rápido as entregas de um paciente e já devolve ordenado por data.
db.Entregas.createIndex({ "IdPaciente": 1, "HoraInicio": -1 });

// Motivo: CRÍTICO para o Relatório "Tempo Médio por Copeira".
// Acelera o filtro por copeira.
db.Entregas.createIndex({ "IdCopeira": 1 });

// Motivo: Para o Dashboard (contar entregas em andamento onde HoraFim é nulo)
db.Entregas.createIndex({ "HoraFim": 1 });

// Motivo: Para validar relacionamentos
db.Entregas.createIndex({ "IdBandeja": 1 });

print("Índices de Entregas criados.");
print("--- Concluído com Sucesso! ---");