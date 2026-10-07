# CubagemCaminhao

Estrutura inicial do sistema de automação de cubagem de caminhões.

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
- `src/CubagemCaminhao.Domain` — entidades do domínio
- `src/CubagemCaminhao.Infrastructure` — contexto e configuração do Entity Framework Core
- `src/CubagemCaminhao.Mobile` — aplicação móvel Flutter
- `database/` — scripts iniciais do banco
- `docker-compose.yml` — ambiente local com API e SQL Server

## Como executar

1. `dotnet build CubagemCaminhao.slnx`
2. `flutter analyze src/CubagemCaminhao.Mobile`
3. `docker compose up --build`

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

Se a API estiver sendo executada diretamente com `dotnet run` em vez do Docker,
use a porta HTTP configurada em `launchSettings.json` (por padrão, `5242`) no
parâmetro `API_BASE_URL`. Em desenvolvimento, a API permite chamadas CORS de
origens locais para viabilizar a execução do app no navegador.

## Estado atual

A solução .NET contém os projetos Blazor, API, Domain e Infrastructure. A API já possui endpoints iniciais para caminhões e itens de carga. As próximas etapas incluem ampliar a modelagem, adicionar migrations, autenticação e implementar o fluxo completo de cubagem.
