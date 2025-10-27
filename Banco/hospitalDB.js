// Acessa o banco de dados. Se ele não existir, o MongoDB o criará.
// use hospitalDB;

// DEFINIÇÃO MANUAL DOS UUIDs COMO STRINGS
// As variáveis de ID (referências) não mudam, pois elas só armazenam o valor UUID.
const dietaLeveId = '3a96e1b0-9f5a-4c28-8e6f-7128f7d98301';
const dietaRegularId = 'c0f3d4a2-1b8e-4f7c-9d0a-526e0b1c9a23';
const dietaSodioId = 'e92d7b6f-4a3c-4e8a-b1d0-659f2c3d4a57';

const copeiraMariaId = '1b8e4f7c-c0f3-4d4a-9d0a-526e0b1c9a24';
const copeiraJoaoId = '4e8a3c4f-e92d-4b1d-0659-f2c3d4a57b6f';

const pacienteCarlosId = '6f7128f7-d983-401a-96e1-b09f5a4c28e6';
const pacienteAnaId = '8e6f7128-f7d9-4830-91a9-6e1b09f5a4c2';

const entrega1Id = '9d0a526e-0b1c-49a2-c0f3-d4a21b8e4f7c';
const entrega2Id = 'b1d0659f-2c3d-4a57-e92d-7b6f4a3c4e8a';

// Novos IDs para Bandejas
const bandeja1Id = 'c7e8f9a0-d1b2-43c4-9e5f-10d2e3f4a5b6'; // Para entrega 1 (Dieta Leve)
const bandeja2Id = 'a1b2c3d4-e5f6-47a8-9b0c-d1e2f3a4b5c6'; // Para entrega 2 (Dieta Regular)


// ----- 1. POPULAÇÃO DAS COLEÇÕES INICIAIS (COM NOMES EM PASCALCASE) -----

// --- DIETAS ---
const dietaLeve = db.Dietas.insertOne({
    _id: dietaLeveId,
    NomeDieta: "Dieta Leve",
    ItensAlimentares: [
      "200g de Sopa de legumes",
      "150g de Purê de batata",
      "100g de Gelatina de morango"
    ]
});

const dietaRegular = db.Dietas.insertOne({
    _id: dietaRegularId,
    NomeDieta: "Dieta Regular",
    ItensAlimentares: [
      "120g de Arroz integral",
      "180g de Frango grelhado",
      "50g de Salada verde",
      "300ml de Suco natural"
    ]
});

const dietaSodio = db.Dietas.insertOne({
    _id: dietaSodioId,
    NomeDieta: "Dieta com restrição de sódio",
    ItensAlimentares: [
      "150g de Salmão assado",
      "100g de Brócolis cozido no vapor",
      "100g de Quinoa",
      "200ml de Água de coco"
    ]
});

// --- COPEIRAS ---
const copeiraMaria = db.Copeiras.insertOne({
    _id: copeiraMariaId,
    Nome: "Maria Aparecida"
});

const copeiraJoao = db.Copeiras.insertOne({
    _id: copeiraJoaoId,
    Nome: "João Silva"
});

// ----- 1.5. POPULAÇÃO DA COLEÇÃO BANDEJAS (NOVA SEÇÃO) -----
// Utiliza os IDs das bandejas recém-definidos
db.Bandejas.insertMany([
  {
    _id: bandeja1Id,
    CodBandeja: "B12345-L-201",
    TipoDieta: dietaLeveId // Referência ao ID da dieta leve
  },
  {
    _id: bandeja2Id,
    CodBandeja: "B67890-R-305",
    TipoDieta: dietaRegularId // Referência ao ID da dieta regular
  }
]);

// ----- 2. POPULAÇÃO E VINCULAÇÃO DOS PACIENTES (COM NOMES EM PASCALCASE) -----

const pacienteCarlos = db.Pacientes.insertOne({
    _id: pacienteCarlosId,
    Nome: "Carlos Eduardo",
    NumQuarto: 201,
    CodPulseira: "P12345",
    IdDieta: dietaLeveId
});

const pacienteAna = db.Pacientes.insertOne({
    _id: pacienteAnaId,
    Nome: "Ana Beatriz",
    NumQuarto: 305,
    CodPulseira: "P67890",
    IdDieta: dietaRegularId
});

// ----- 3. POPULAÇÃO E VINCULAÇÃO DAS ENTREGAS (COM NOMES EM PASCALCASE) -----

db.Entregas.insertMany([
  {
    _id: entrega1Id,
    IdPaciente: pacienteCarlosId,
    IdCopeira: copeiraMariaId,
    HoraInicio: new Date("2025-09-25T08:00:00Z"),
    HoraFim: new Date("2025-09-25T08:05:00Z"),
    IdBandeja: bandeja1Id, // <-- ALTERADO: Agora é o ID da bandeja
    StatusValidacao: "Correta",
    Observacao: "Entrega bem-sucedida"
  },
  {
    _id: entrega2Id,
    IdPaciente: pacienteAnaId,
    IdCopeira: copeiraJoaoId,
    HoraInicio: new Date("2025-09-25T08:15:00Z"),
    HoraFim: new Date("2025-09-25T08:20:00Z"),
    IdBandeja: bandeja2Id, // <-- ALTERADO: Agora é o ID da bandeja
    StatusValidacao: "Correta",
    Observacao: "Sem intercorrências"
  }
]);

// --- Consultas de Verificação ---
db.getCollection("Bandejas").find({}) // <-- NOVO: Consulta de verificação
db.getCollection("Copeiras").find({})
db.getCollection("Dietas").find({})
db.getCollection("Entregas").find({})
db.getCollection("Pacientes").find({})

// --- Consulta Aggregation Ajustada para PascalCase (Exemplo de Lookup de Paciente) ---
db.Pacientes.aggregate([
  {
    $match: {
      _id: pacienteCarlosId 
    }
  },
  {
    $lookup: {
      from: "Dietas", 
      localField: "IdDieta", 
      foreignField: "_id", 
      as: "DetalhesDieta"
    }
  },
  {
    $unwind: "$DetalhesDieta"
  },
  {
    $lookup: {
      from: "Entregas", 
      localField: "_id", 
      foreignField: "IdPaciente",
      as: "HistoricoEntregas",
      pipeline: [
        {
          $lookup: {
            from: "Bandejas", // <-- NOVO LOOKUP: Vinculando a Entrega com a Bandeja
            localField: "IdBandeja",
            foreignField: "_id",
            as: "DetalhesBandeja"
          }
        },
        {
          $unwind: "$DetalhesBandeja"
        },
        {
          $lookup: {
            from: "Copeiras", 
            localField: "IdCopeira",
            foreignField: "_id", 
            as: "CopeiraResponsavel"
          }
        },
        {
          $unwind: "$CopeiraResponsavel"
        }
      ]
    }
  }
]);