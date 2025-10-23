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


// ----- 1. POPULAÇÃO DAS COLEÇÕES INICIAIS (COM NOMES EM PASCALCASE) -----

// --- DIETAS ---
const dietaLeve = db.Dietas.insertOne({
    _id: dietaLeveId,
    NomeDieta: "Dieta Leve", // <-- ALTERADO: PascalCase
    ItensAlimentares: [      // <-- ALTERADO: PascalCase
      "200g de Sopa de legumes",
      "150g de Purê de batata",
      "100g de Gelatina de morango"
    ]
});

const dietaRegular = db.Dietas.insertOne({
    _id: dietaRegularId,
    NomeDieta: "Dieta Regular", // <-- ALTERADO
    ItensAlimentares: [         // <-- ALTERADO
      "120g de Arroz integral",
      "180g de Frango grelhado",
      "50g de Salada verde",
      "300ml de Suco natural"
    ]
});

const dietaSodio = db.Dietas.insertOne({
    _id: dietaSodioId,
    NomeDieta: "Dieta com restrição de sódio", // <-- ALTERADO
    ItensAlimentares: [                        // <-- ALTERADO
      "150g de Salmão assado",
      "100g de Brócolis cozido no vapor",
      "100g de Quinoa",
      "200ml de Água de coco"
    ]
});

// --- COPEIRAS ---
const copeiraMaria = db.Copeiras.insertOne({
    _id: copeiraMariaId,
    Nome: "Maria Aparecida" // <-- ALTERADO
});

const copeiraJoao = db.Copeiras.insertOne({
    _id: copeiraJoaoId,
    Nome: "João Silva" // <-- ALTERADO
});

// ----- 2. POPULAÇÃO E VINCULAÇÃO DOS PACIENTES (COM NOMES EM PASCALCASE) -----

const pacienteCarlos = db.Pacientes.insertOne({
    _id: pacienteCarlosId,
    Nome: "Carlos Eduardo", // <-- ALTERADO
    NumQuarto: 201,         // <-- ALTERADO
    CodPulseira: "P12345", // <-- ALTERADO
    IdDieta: dietaLeveId    // <-- ALTERADO: Campo de referência
});

const pacienteAna = db.Pacientes.insertOne({
    _id: pacienteAnaId,
    Nome: "Ana Beatriz",    // <-- ALTERADO
    NumQuarto: 305,          // <-- ALTERADO
    CodPulseira: "P67890", // <-- ALTERADO
    IdDieta: dietaRegularId // <-- ALTERADO: Campo de referência
});

// ----- 3. POPULAÇÃO E VINCULAÇÃO DAS ENTREGAS (COM NOMES EM PASCALCASE) -----

db.Entregas.insertMany([
  {
    _id: entrega1Id,
    IdPaciente: pacienteCarlosId, // <-- ALTERADO: Campo de referência
    IdCopeira: copeiraMariaId,    // <-- ALTERADO: Campo de referência
    HoraInicio: new Date("2025-09-25T08:00:00Z"), // <-- ALTERADO
    HoraFim: new Date("2025-09-25T08:05:00Z"),     // <-- ALTERADO
    CodBandeja: "B12345-L-201",                   // <-- ALTERADO
    StatusValidacao: "Correta",                  // <-- ALTERADO
    Observacao: "Entrega bem-sucedida"           // <-- ALTERADO
  },
  {
    _id: entrega2Id,
    IdPaciente: pacienteAnaId,    // <-- ALTERADO: Campo de referência
    IdCopeira: copeiraJoaoId,     // <-- ALTERADO: Campo de referência
    HoraInicio: new Date("2025-09-25T08:15:00Z"), // <-- ALTERADO
    HoraFim: new Date("2025-09-25T08:20:00Z"),     // <-- ALTERADO
    CodBandeja: "B67890-R-305",                   // <-- ALTERADO
    StatusValidacao: "Correta",                  // <-- ALTERADO
    Observacao: "Sem intercorrências"           // <-- ALTERADO
  }
]);

// --- Consultas de Verificação ---
db.getCollection("Copeiras").find({})
db.getCollection("Dietas").find({})
db.getCollection("Entregas").find({})
db.getCollection("Pacientes").find({})

// --- Consulta Aggregation Ajustada para PascalCase ---
db.Pacientes.aggregate([
  {
    $match: {
      _id: pacienteCarlosId 
    }
  },
  {
    // Usa o novo campo de referência: IdDieta
    $lookup: {
      from: "Dietas", 
      localField: "IdDieta", 
      foreignField: "_id", 
      as: "DetalhesDieta" // <-- ALTERADO (opcional, mas recomendado)
    }
  },
  {
    $unwind: "$DetalhesDieta" // <-- CORRIGIDO
  },
  {
    $lookup: {
      from: "Entregas", 
      localField: "_id", 
      foreignField: "IdPaciente", // <-- CORRIGIDO: Campo de referência em Entregas
      as: "HistoricoEntregas",     // <-- ALTERADO (opcional)
      pipeline: [
        {
          $lookup: {
            from: "Copeiras", 
            localField: "IdCopeira", // <-- CORRIGIDO: Campo de referência em Entregas
            foreignField: "_id", 
            as: "CopeiraResponsavel" // <-- ALTERADO (opcional)
          }
        },
        {
          $unwind: "$CopeiraResponsavel" // <-- CORRIGIDO
        }
      ]
    }
  }
]);