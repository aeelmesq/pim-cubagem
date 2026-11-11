# Arquitetura

Web e mobile falam só com a API. Só a API fala com o banco. Isso é exigência do projeto e também a forma certa: as regras de cálculo e de segurança ficam num lugar só.

```mermaid
flowchart LR
    W[Web - Blazor] -->|HTTP / JSON| A[API - ASP.NET Core]
    M[Mobile - Flutter] -->|HTTP / JSON| A
    A -->|Entity Framework Core| D[(SQL Server)]
    subgraph Docker
        A
        D
    end
```

## Tecnologias e o papel de cada uma

| Parte | Tecnologia | Linguagem | O que faz |
| --- | --- | --- | --- |
| Web | Blazor (Interactive Server) | C# | Telas de cadastro, carregamento e relatórios |
| Mobile | Flutter | Dart | Consulta e conferência pelo celular |
| API | ASP.NET Core (minimal API, .NET 10) | C# | Regras de negócio, validação, cálculo, acesso ao banco |
| Domínio | Biblioteca de classes | C# | Entidades (`Truck`, `CargoItem`, `Driver`) e as regras de cálculo (`CubageCalculator`) |
| Infraestrutura | Entity Framework Core | C# | Mapeamento das tabelas (`CubagemDbContext`) |
| Banco | SQL Server 2022 | SQL | Guarda os dados |
| Ambiente | Docker Compose | YAML | Sobe API e banco com um comando |

## Por que separar em projetos

- **Domain** não depende de nada. Dá para testar o cálculo sem banco e sem API.
- **Infrastructure** sabe falar com o banco.
- **Api** junta tudo e expõe os endpoints.

As contas (volume, ocupação, peso cubado, "cabe?") ficam em `Domain/CubageCalculator.cs`, e não nos endpoints. Por isso dá para testá-las sem banco e sem API (`tests/CubagemCaminhao.Domain.Tests`).

## Endpoints atuais

| Método | Rota | O que faz |
| --- | --- | --- |
| GET | `/api/trucks` | Lista caminhões com cargas e cálculos |
| POST | `/api/trucks` | Cria caminhão |
| DELETE | `/api/trucks/{id}` | Exclui caminhão (e suas cargas) |
| PUT | `/api/trucks/{id}/driver` | Troca o motorista do caminhão (`driverId: null` deixa sem motorista) |
| POST | `/api/trucks/{id}/cargo` | Adiciona item de carga |
| DELETE | `/api/trucks/{id}/cargo/{cargoId}` | Remove item |
| GET | `/api/drivers` | Lista motoristas, com a placa do caminhão de cada um |
| POST | `/api/drivers` | Cria motorista |
| DELETE | `/api/drivers/{id}` | Exclui motorista (o caminhão dele fica sem motorista) |
| GET | `/api/status` | Status da API |
| GET | `/health` | Health check |

## Portas

| Como roda | API |
| --- | --- |
| Docker Compose | `http://localhost:8080` |
| `dotnet run` | `http://localhost:5242` |
| Emulador Android acessando a máquina | `http://10.0.2.2:8080` |

O Blazor aponta para `8080` (API no Docker). Para usar a API com `dotnet run`, troque `ApiBaseUrl` no `appsettings.json` do Blazor para `http://localhost:5242`.
