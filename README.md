# API de agendamentos para barbearias

Web API REST em ASP.NET Core 10, com Entity Framework Core, SQLite, ASP.NET Core Identity e FluentValidation.

## Executar

É necessário ter o SDK do .NET 10 instalado. Na pasta do projeto:

```powershell
dotnet restore
dotnet run
```

O SQLite cria e inicializa `barbearia.db` sem dados fictícios em instalações novas. Bancos anteriores são atualizados preservando os registros existentes, que ficam associados à barbearia inicial. Faça um backup do banco antes da primeira inicialização com esta versão. No ambiente Development, o documento OpenAPI fica em `http://localhost:5194/openapi/v1.json`; exemplos estão em [aspnet-api-scheduling.http](./aspnet-api-scheduling.http).

## Primeira conta proprietária

O cadastro público de contas fica fechado. Em desenvolvimento local, os segredos ficam no User Secrets do .NET, fora do repositório. Para configurar ou substituir os segredos:

```powershell
dotnet user-secrets init
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$bootstrapBytes = New-Object byte[] 48
$signingBytes = New-Object byte[] 64
$rng.GetBytes($bootstrapBytes)
$rng.GetBytes($signingBytes)
$bootstrapToken = [Convert]::ToBase64String($bootstrapBytes).TrimEnd('=').Replace('+','-').Replace('/','_')
$signingKey = [Convert]::ToBase64String($signingBytes)
dotnet user-secrets set 'Bootstrap:Token' $bootstrapToken
dotnet user-secrets set 'Authentication:SigningKey' $signingKey
$rng.Dispose()
```

Os segredos de desenvolvimento já podem ser definidos localmente com `dotnet user-secrets list`; esse comando mostra os valores, portanto não compartilhe nem cole sua saída. Inicie a API com `dotnet run` e chame `POST /api/provisionamento/dono` com o valor de `Bootstrap:Token` no cabeçalho `X-Provisioning-Key`. A rota só funciona enquanto não houver usuários. Depois, autentique em `POST /api/auth/login`, informando também o `barbeariaId`, para obter um token Bearer. Não publique a chave de provisionamento.

Em produção, injete `Bootstrap__Token` e `Authentication__SigningKey` por um gerenciador de segredos da plataforma. A chave de assinatura deve ser Base64 de pelo menos 32 bytes aleatórios; configure também `Authentication__Issuer` e `Authentication__Audience` para o ambiente. Não use User Secrets como armazenamento de segredos de produção.

Uma conta pode administrar várias barbearias. Com um token de proprietário, `GET /api/barbearias` lista os vínculos e `POST /api/barbearias` cria outra unidade. Para operar na nova unidade, autentique novamente selecionando seu ID.

## Isolamento entre barbearias

- Dados operacionais (clientes, barbeiros, serviços, produtos e agendamentos) são filtrados pelo tenant selecionado.
- Rotas públicas de catálogo e agendamento exigem `X-Barbearia-Id`.
- Rotas administrativas exigem token Bearer com papel `Dono`; o token seleciona a barbearia e não pode ser redirecionado para outra por cabeçalho.
- As consultas e gravações são escopadas pelo tenant; em bancos novos, chaves estrangeiras compostas também impedem relações entre agendamentos e registros de outra barbearia.
- Serviços e produtos são inativados, não removidos. Agendamentos guardam nome e preço do serviço no momento da reserva para preservar o histórico.
- Produto é um item de catálogo sem controle de estoque.

## Endpoints principais

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| POST | `/api/provisionamento/dono` | Chave inicial, uma vez | Provisiona a primeira conta e barbearia |
| POST | `/api/auth/login` | Público | Emite token Bearer para uma barbearia vinculada |
| GET/POST | `/api/barbearias` | Proprietário | Lista/cria barbearias da conta |
| GET | `/api/barbearias/{id}` | Proprietário | Consulta uma barbearia vinculada à conta |
| GET | `/api/servicos` | Público + `X-Barbearia-Id` | Lista serviços ativos |
| POST | `/api/servicos` | Proprietário | Cadastra serviço |
| PUT | `/api/servicos/{id}` | Proprietário | Altera nome, preço e duração |
| PATCH | `/api/servicos/{id}/inativar` | Proprietário | Inativa serviço |
| GET | `/api/produtos` | Público + `X-Barbearia-Id` | Lista produtos ativos |
| POST | `/api/produtos` | Proprietário | Cadastra produto |
| PUT | `/api/produtos/{id}` | Proprietário | Altera produto e preço |
| PATCH | `/api/produtos/{id}/inativar` | Proprietário | Inativa produto |
| GET | `/api/barbeiros` | Público + `X-Barbearia-Id` | Lista barbeiros ativos |
| POST | `/api/barbeiros` | Proprietário | Cadastra barbeiro |
| GET/POST | `/api/clientes` | Admin / público | Consulta clientes ou cadastra cliente |
| GET | `/api/agendamentos/horarios-disponiveis` | Público + `X-Barbearia-Id` | Consulta horários |
| POST | `/api/agendamentos` | Público + `X-Barbearia-Id` | Cria agendamento |
| GET | `/api/agendamentos/{id}` | Proprietário | Consulta agendamento |
| PATCH | `/api/agendamentos/{id}/cancelar` | Proprietário | Cancela agendamento |

O POST de agendamento recebe `clienteId`, `servicoId`, `dataHoraInicio`, `naoQueroConversar`, `observacao` e `barbeiroId` opcional. A duração do serviço determina `dataHoraFim`.

## Regras da agenda

- A grade parte de 08:00 e avança em passos fixos de 40 minutos; o último início gerado é 19:20.
- Horários cujo serviço cruza o intervalo de almoço (12:00–13:00) não são oferecidos.
- Sem `barbeiroId`, basta que um barbeiro ativo esteja livre durante todo o serviço; na criação, o primeiro barbeiro livre é selecionado.
- Sobreposições são rejeitadas para o mesmo barbeiro. Agendamentos cancelados não bloqueiam horários.
- Os horários de negócio usam `Scheduling:TimeZoneId` (padrão `America/Sao_Paulo`).

## Organização

- `Controllers/`: endpoints REST e respostas HTTP.
- `Data/`: contexto EF Core e atualização compatível do SQLite existente.
- `DTOs/`: contratos de entrada e saída.
- `Middleware/`: resolução do tenant e tratamento global de erros.
- `Models/`: entidades de domínio.
- `Services/`: autenticação, regras de negócio e persistência.
- `Validators/`: validação de payloads.
