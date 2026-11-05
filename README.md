# API de agendamentos para barbearia

Web API REST em ASP.NET Core 9, com Entity Framework Core, SQLite e FluentValidation.

## Executar

É necessário ter o SDK do .NET 9 instalado. Na pasta do projeto:

```powershell
dotnet restore
dotnet run
```

O SQLite cria automaticamente `barbearia.db` na primeira execução. A base é inicializada com dois barbeiros, três serviços e um cliente de demonstração (`clienteId: 1`). A configuração de fuso horário fica em `Scheduling:TimeZoneId` no `appsettings.json`.

No ambiente Development, o documento OpenAPI fica em `http://localhost:5194/openapi/v1.json`. Os exemplos de chamadas estão em [aspnet-api-scheduling.http](./aspnet-api-scheduling.http).

## Organização

- `Controllers/`: endpoints REST e respostas HTTP.
- `Data/`: contexto, configuração e dados iniciais do EF Core.
- `DTOs/`: contratos de entrada e saída da API.
- `Middleware/`: tratamento global de erros com Problem Details.
- `Models/`: entidades e enumerações de domínio.
- `Services/`: regras de negócio, consulta e persistência.
- `Validators/`: validação de payloads com FluentValidation.

## Regras da agenda

- A grade parte de 08:00 e avança em passos fixos de 40 minutos, até o limite inclusivo de 19:40. Como 19:40 não está alinhado a essa sequência, o último início gerado é 19:20.
- Horários cujo serviço cruza o intervalo de almoço (12:00–13:00) não são oferecidos.
- Sem `barbeiroId`, basta que um barbeiro ativo esteja livre durante todo o serviço; na criação, o primeiro barbeiro livre é selecionado.
- Sobreposições são rejeitadas para o mesmo barbeiro. Agendamentos cancelados não bloqueiam horários.
- Os horários de negócio usam o fuso configurado (padrão `America/Sao_Paulo`); o início da requisição deve corresponder a um horário válido da grade nesse fuso.

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/clientes` | Lista clientes cadastrados |
| GET | `/api/clientes/{id}` | Consulta cliente |
| POST | `/api/clientes` | Cadastra cliente (e-mail único) |
| GET | `/api/servicos` | Lista serviços ativos |
| GET | `/api/servicos/{id}` | Consulta serviço |
| POST | `/api/servicos` | Cadastra serviço ativo |
| GET | `/api/barbeiros` | Lista barbeiros ativos |
| GET | `/api/barbeiros/{id}` | Consulta barbeiro |
| POST | `/api/barbeiros` | Cadastra barbeiro ativo |
| GET | `/api/agendamentos/horarios-disponiveis?data=YYYY-MM-DD&servicoId=X&barbeiroId=Y` | Retorna horários agrupados em `manha`, `tarde` e `noite`; barbeiro opcional |
| POST | `/api/agendamentos` | Cria um agendamento |
| GET | `/api/agendamentos/{id}` | Consulta um agendamento |
| GET | `/api/agendamentos/barbeiro/{barbeiroId}?data=YYYY-MM-DD` | Lista a agenda do dia |
| PATCH | `/api/agendamentos/{id}/cancelar` | Cancela um agendamento |

O POST recebe `clienteId`, `servicoId`, `dataHoraInicio`, `naoQueroConversar`, `observacao` e `barbeiroId` opcional. A duração do serviço determina `dataHoraFim`.

Os endpoints `POST /api/clientes`, `POST /api/barbeiros` e `POST /api/servicos` permitem cadastrar os dados necessários pela própria API. Todos validam o payload e respondem `201 Created` com `Location` para consultar o registro; e-mail duplicado retorna `409 Conflict`. Os exemplos de cadastro estão no arquivo `.http`.
