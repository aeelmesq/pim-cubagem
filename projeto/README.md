# CubagemCaminhao

Sistema de automação de cubagem de caminhões (PIM IV).

## Arquitetura

- Blazor: interface web
- Flutter: aplicativo móvel
- ASP.NET Core API: camada de integração e regras de negócio
- Domain: entidades e conceitos centrais do sistema
- Infrastructure: acesso a dados com Entity Framework Core
- SQL Server: persistência dos dados
- Docker: orquestração do ambiente local

A comunicação entre as interfaces e o banco de dados deve ocorrer exclusivamente pela API .NET.

## Estrutura

- `src/CubagemCaminhao.Api` — API ASP.NET Core
- `src/CubagemCaminhao.Blazor` — aplicação web Blazor
- `src/CubagemCaminhao.Domain` — entidades e regras de cálculo (`CubageCalculator`)
- `src/CubagemCaminhao.Infrastructure` — contexto e configuração do Entity Framework Core
- `src/CubagemCaminhao.Mobile` — aplicação móvel Flutter
- `tests/CubagemCaminhao.Domain.Tests` — testes das regras de cálculo (xUnit)
- `docker-compose.yml` — ambiente local com API e SQL Server
- `deploy/` — publicação na VPS com nginx e HTTPS (veja [deploy/README.md](deploy/README.md))

## Como executar

1. Crie o arquivo `.env` com a senha do banco (ele não vai para o Git):

   ```bash
   cp .env.example .env
   ```

   Troque a senha dentro dele. O SQL Server exige pelo menos 8 caracteres, com maiúscula, minúscula, número e símbolo.

2. Suba o banco e a API:

   ```bash
   docker compose up --build
   ```

   A API fica em `http://localhost:8080` (teste com `http://localhost:8080/api/status`).

3. Suba a web:

   ```bash
   cd src/CubagemCaminhao.Blazor
   dotnet run
   ```

   A web fica em `http://localhost:5107`.

Para conferir o código: `dotnet build CubagemCaminhao.slnx`, `dotnet test CubagemCaminhao.slnx` e `flutter analyze src/CubagemCaminhao.Mobile`.

Se alguma tabela mudou (campo ou tabela nova), o banco antigo não é atualizado sozinho. Apague e suba de novo (os dados são perdidos):

```bash
docker compose down -v
docker compose up --build
```

### Login do motorista e histórico de cargas

1. Em **Motoristas** (`/motoristas`), cadastre o motorista com nome, usuário e senha (mínimo 8 caracteres). Coloque-o num caminhão em **Caminhões**.
2. O motorista entra em `/entrar` e vê, em `/minha-carga`, o caminhão dele com a carga atual e o histórico das cargas que já levou.
3. No **Início**, o botão **Registrar saída** grava a carga atual do caminhão no histórico (com placa, motorista, itens e resultados do cálculo daquele momento) e deixa o caminhão vazio para o próximo carregamento. Todas as saídas ficam em **Histórico de cargas** (`/historico`).

Como o login funciona:

- A senha é guardada só como hash (PBKDF2 com salt, `PasswordHasher` do ASP.NET Core). Nem o banco tem a senha.
- `POST /api/auth/login` confere usuário e senha e devolve um JWT (vale 8 horas) num cookie `cubagem_token` **HttpOnly**, `SameSite=Strict` e `Secure` em HTTPS. São no máximo 10 tentativas por minuto por IP.
- `GET /api/me` exige o token (cookie ou `Authorization: Bearer`) e devolve só os dados do motorista logado.
- O Blazor chama a API pelo servidor: ele recebe o token, guarda no cookie HttpOnly do site e manda no cabeçalho nas chamadas a `/api/me`. Sair apaga o cookie.
- A chave que assina o JWT fica em `Jwt:Key`. Em desenvolvimento ela já está no `appsettings.Development.json` da API; em produção vem da variável `JWT_KEY` (veja [deploy/README.md](deploy/README.md)).

### Login do administrador

As telas de cadastro (Início, Caminhões, Motoristas e Histórico) exigem o login de administrador em `/admin/entrar`.

- Em desenvolvimento (Docker Compose ou `dotnet run`), o usuário é `admin` e a senha é `admin12345` (em `appsettings.Development.json` da API).
- Em produção, defina `ADMIN_USER` e `ADMIN_PASSWORD` no `deploy/.env`. Sem senha configurada, o login de administrador fica desligado.
- A API confere usuário e senha (`POST /api/auth/admin/login`, com o mesmo limite de 10 tentativas por minuto) e o Blazor guarda a sessão num cookie HttpOnly (`cubagem_admin`, 8 horas).
- As rotas da API continuam abertas para o app Flutter; no servidor elas seguem protegidas pela senha do nginx.

### Aplicativo mobile

Com o SQL Server e a API iniciados pelo Docker Compose, execute:

```powershell
cd src\CubagemCaminhao.Mobile
flutter pub get
flutter run
```

O app usa `http://10.0.2.2:8080` por padrão no emulador Android e
`http://localhost:8080` no Flutter Web. Para executar no navegador:

```powershell
flutter run -d chrome
```

Em um dispositivo físico, informe o IP da máquina na rede local, por exemplo:

```powershell
flutter run --dart-define=API_BASE_URL=http://192.168.0.10:8080
```

Em desenvolvimento, a API permite chamadas CORS de origens locais para o app rodar no navegador.

### API com `dotnet run` (sem Docker)

Suba só o banco (`docker compose up sqlserver`) e guarde a connection string fora do código, com a senha do `.env`:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CubagemCaminhaoDb;User Id=sa;Password=<senha do .env>;TrustServerCertificate=True;" --project src/CubagemCaminhao.Api
cd src/CubagemCaminhao.Api
dotnet run
```

Assim a API fica na porta `5242`. Troque `ApiBaseUrl` no `appsettings.json` do Blazor para `http://localhost:5242` e, no app, use `--dart-define=API_BASE_URL=http://localhost:5242`.

## Estado atual

Veja [base-conhecimento/08-estado-atual.md](../base-conhecimento/08-estado-atual.md).
