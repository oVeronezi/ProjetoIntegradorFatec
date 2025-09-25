// Acessa o banco de dados. Se ele não existir, o MongoDB o criará.
use hospitalDB;

// ----- 1. POPULAÇÃO DAS COLEÇÕES INICIAIS -----

// Inserindo documentos de dieta e salvando seus IDs em variáveis
const dietaLeve = db.Dietas.insertOne({
    nomeDieta: "Dieta Leve",
    itensAlimentares: [
      "200g de Sopa de legumes",
      "150g de Purê de batata",
      "100g de Gelatina de morango"
    ]
});
const dietaRegular = db.Dietas.insertOne({
    nomeDieta: "Dieta Regular",
    itensAlimentares: [
      "120g de Arroz integral",
      "180g de Frango grelhado",
      "50g de Salada verde",
      "300ml de Suco natural"
    ]
});
const dietaSodio = db.Dietas.insertOne({
    nomeDieta: "Dieta com restrição de sódio",
    itensAlimentares: [
      "150g de Salmão assado",
      "100g de Brócolis cozido no vapor",
      "100g de Quinoa",
      "200ml de Água de coco"
    ]
});

// Inserindo documentos de copeiras e salvando seus IDs em variáveis
const copeiraMaria = db.Copeiras.insertOne({
    nome: "Maria Aparecida"
});
const copeiraJoao = db.Copeiras.insertOne({
    nome: "João Silva"
});

// ----- 2. POPULAÇÃO E VINCULAÇÃO DOS PACIENTES -----

// Inserindo documentos de pacientes e vinculando o ID da dieta
const pacienteCarlos = db.Pacientes.insertOne({
    nome: "Carlos Eduardo",
    numQuarto: 201,
    codPulseira: "P12345",
    idDieta: dietaLeve.insertedId // Vincula ao ID da Dieta Leve
});
const pacienteAna = db.Pacientes.insertOne({
    nome: "Ana Beatriz",
    numQuarto: 305,
    codPulseira: "P67890",
    idDieta: dietaRegular.insertedId // Vincula ao ID da Dieta Regular
});

// ----- 3. POPULAÇÃO E VINCULAÇÃO DAS ENTREGAS -----

// Inserindo documentos de entrega e vinculando os IDs de paciente e copeira
db.Entregas.insertMany([
  {
    idPaciente: pacienteCarlos.insertedId, // Vincula ao ID do Carlos
    idCopeira: copeiraMaria.insertedId,    // Vincula ao ID da Maria
    horaInicio: new Date("2025-09-25T08:00:00Z"),
    horaFim: new Date("2025-09-25T08:05:00Z"),
    codBandeja: "B12345-L-201",
    statusValidacao: "Correta",
    observacao: "Entrega bem-sucedida"
  },
  {
    idPaciente: pacienteAna.insertedId, // Vincula ao ID da Ana
    idCopeira: copeiraJoao.insertedId,    // Vincula ao ID do João
    horaInicio: new Date("2025-09-25T08:15:00Z"),
    horaFim: new Date("2025-09-25T08:20:00Z"),
    codBandeja: "B67890-R-305",
    statusValidacao: "Correta",
    observacao: "Sem intercorrências"
  }
]);

// db.getCollection("Copeiras").find({})
// db.getCollection("Dietas").find({})
// db.getCollection("Entregas").find({})
// db.getCollection("Pacientes").find({})