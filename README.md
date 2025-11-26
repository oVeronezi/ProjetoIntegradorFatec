# DOCUMENTAÇÃO DO PI

## 1. Introdução

O projeto **Controle de Dieta Hospitalar** tem como objetivo desenvolver um protótipo funcional para o monitoramento e validação da entrega de refeições no ambiente hospitalar. Atualmente, a ausência de um sistema para registrar e validar este processo pode levar a falhas operacionais, como atrasos, extravios de bandejas ou entregas incorretas de dietas a pacientes.

A solução proposta utiliza tecnologia para garantir a segurança alimentar, o controle operacional e o rastreamento eficiente das refeições, desde a preparação até a entrega final ao paciente.

---

### • Objetivos

#### Objetivo Geral
Desenvolver um protótipo funcional para monitorar e validar o processo de entrega de refeições hospitalares, garantindo a segurança do paciente e a eficiência operacional.

#### Objetivos Específicos
- Monitorar o tempo de entrega das refeições (início e fim).
- Registrar qual copeira realizou a entrega.
- Validar se a bandeja corresponde ao paciente certo, utilizando códigos de barras.
- Gerar relatórios simples (ex: tempo médio de entrega, erros de validação, histórico por paciente).
- Estar preparado para futura integração com o prontuário eletrônico.

---

### • Metodologia

Para o desenvolvimento deste projeto, foram utilizadas as seguintes tecnologias e ferramentas:

- **Linguagens de Programação:** C#, HTML, CSS  
- **Banco de Dados:** MongoDB  
- **Controle de Versão:** Git e GitHub  
- **Planejamento:** Trello  
- **Prototipagem:** [A definir]  
- **Metodologia de Desenvolvimento:** Scrum — abordagem ágil que permite adaptações rápidas a mudanças e promove entregas incrementais.

**Link do Trello:** [Quadro de Planejamento do PI](https://trello.com/invite/b/67d96ee6c4eec72235cd27a7/ATTIcc8c394bb422c889d3f6de5c965a6044FC681C1A/pi)

---

## 2. Requisitos

### • Requisitos Funcionais
- RF1: Registrar o horário de início e fim da entrega.  
- RF2: Registrar a copeira responsável pela entrega.  
- RF3: Ler o código de barras da pulseira do paciente.  
- RF4: Ler o código de barras da etiqueta da bandeja.  
- RF5: Validar a correspondência entre paciente e bandeja.  
- RF6: Emitir aviso sonoro/visual em caso de erro.  
- RF7: Gerar relatórios de tempo médio de entrega.  
- RF8: Gerar relatórios de erros de validação.  
- RF9: Manter histórico de entregas por paciente.  
- RF10: Permitir CRUD de Paciente.  
- RF11: Permitir CRUD de Dieta.  
- RF12: Permitir CRUD de Copeira.  

### • Requisitos Não Funcionais
- RNF1: Usabilidade — interface simples e intuitiva.  
- RNF2: Confiabilidade — alta disponibilidade.  
- RNF3: Desempenho — validação e registro rápidos.  
- RNF4: Segurança — garantir privacidade dos dados do paciente.  
- RNF5: Manutenibilidade — código bem documentado.  

---

## 3. Modelo de Casos de Uso

![Diagrama de Casos de Uso](img/casosUso.jpg)

### Casos de Uso de Alto Nível
- **Cadastrar nova dieta:** Permite cadastrar e associar dietas a pacientes.  
- **Cadastrar copeira:** Registra novas copeiras responsáveis pelas entregas.  
- **Ler código de barras:** Escaneia códigos para validar bandejas e pacientes.  
- **Ver relatórios:** Visualiza relatórios de entregas e erros.  
- **Cadastrar novo paciente:** Registra novos pacientes e vincula às copeiras.  
- **Inserir dados para relatório:** Gera dados para relatórios de desempenho.  

---

## 4. Modelo do Banco de Dados

O banco de dados do projeto foi estruturado com o **MongoDB**, tecnologia NoSQL que utiliza coleções de documentos para armazenar dados com flexibilidade e escalabilidade.

> **Pacientes:** Dados básicos e referência à dieta prescrita.  
> **Dietas:** Definição detalhada das dietas.  
> **Copeiras:** Informações das copeiras e rastreabilidade.  
> **Entregas:** Registra cada evento de entrega, vinculando paciente e copeira.

![Diagrama do Banco de Dados](img/Banco.png)

---

## 5. Banco de Dados

O modelo físico segue o design baseado no diagrama acima, utilizando coleções em MongoDB para armazenar e cruzar as informações de forma eficiente.

---

## 6. Diagrama de Classes

O diagrama de classes representa a estrutura do sistema, incluindo entidades como Pacientes, Copeiras, Entregas e Dietas, e seus relacionamentos.

![Diagrama de Classes](img/DietaHospitalar.png)

### Relacionamento entre Classes
- **Pacientes ↔ Entregas:** Agregação (um paciente pode ter várias entregas).  
- **Copeiras ↔ Entregas:** Agregação (uma copeira realiza várias entregas).  
- **Pacientes ↔ Dietas:** Associação direta entre paciente e dieta.  
- **Relatórios ↔ Outras Classes:** Associação usada para coleta de dados.  

---

## 7. Estudo de Viabilidade

### Viabilidade de Mercado
O sistema propõe uma solução inovadora para controle de refeições hospitalares, um setor com alta demanda por eficiência e segurança alimentar. Não há concorrentes diretos, o que torna o projeto competitivo.

### Viabilidade de Recursos
- **Humanos:** Desenvolvedores C#, designer UI/UX e testador.  
- **Tecnológicos:** C#, MongoDB e dispositivos móveis com câmera.  
- **Financeiros:** Custos baixos, com uso de ferramentas gratuitas.  

### Viabilidade Operacional
O fluxo operacional é simples:  
1. A copeira escaneia o código da bandeja.  
2. O sistema registra o início da entrega.  
3. A copeira escaneia o código da pulseira do paciente.  
4. O sistema valida a correspondência.  
5. A entrega é registrada como concluída.  

### Conclusão
O projeto é viável em todas as dimensões: mercado, recursos e operação, apresentando inovação e aplicabilidade prática.

---

## 8. Regras de Negócio (Modelo Canvas)

### RN1: Acesso e Autenticação
- Acesso restrito a usuários autenticados.  
- Rastreabilidade das ações da copeira logada.  
- Perfis de acesso: **Copeira** e **Gestor de Nutrição**.

### RN2: Processo de Entrega
- Ciclo de entrega inicia e termina com registros de horário.  
- Entrega só pode ser concluída após validação bem-sucedida.

### RN3: Validação e Segurança
- Validação obrigatória de pulseira e bandeja.  
- Bloqueio em caso de incompatibilidade.  
- Alerta sonoro e visual em caso de erro.  
- Registro obrigatório de falhas.

### RN4: Dados e Relatórios
- Cálculo automático do tempo de entrega.  
- Proibição de exclusão de registros por copeiras.  
- Relatórios detalhados de erros e desempenho.

![Modelo de Negócio Canva](img/Canva-Pi.png)

---

## 9. Design

---

9. Design

O design do projeto segue integralmente a identidade visual da Unimed, mantendo consistência com seus padrões oficiais. Foram aplicadas:

Paleta de Cores Institucional: tons de verde característicos da marca (verde Unimed, verde secundário e variações complementares).

Tipografia Oficial: uso de fontes que se aproximam da identidade visual da Unimed, priorizando legibilidade e alinhamento com o branding da cooperativa.

Elementos Visuais Padronizados: uso de ícones, espaçamentos e componentes que refletem a simplicidade, clareza e acolhimento presentes nos materiais Unimed.

Layout Consistente: telas projetadas seguindo diretrizes modernas, minimalistas e compatíveis com a linha adotada pela empresa.


Os wireframes e protótipos foram desenvolvidos respeitando fielmente o guia de marca, garantindo que a experiência se mantenha coerente com o ecossistema visual da Unimed.


---

## 10. Protótipo

  https://www.figma.com/design/c5hez5J1n6lQmMHrCtTWZu/Untitled?fuid=1297872424791518125


---

## 11. Aplicação

A aplicação será implementada com base no protótipo aprovado e validada conforme os requisitos.  

`[A ser desenvolvido]`

---

## 12. Considerações Finais

O **Sistema de Controle de Dieta Hospitalar** representa uma solução moderna e segura para a gestão de refeições hospitalares, reduzindo erros humanos e melhorando a rastreabilidade dos processos.

---

## 13. Referências Bibliográficas

`[A ser inserido]`


