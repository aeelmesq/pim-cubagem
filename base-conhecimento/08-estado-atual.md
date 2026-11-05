# Estado atual do código

Revisão de 07/10/2026, feita sobre `PIM_IV.zip` (hoje extraído em `projeto/`). A solução .NET compila sem erros nem avisos (`dotnet build CubagemCaminhao.slnx`).

## O que funciona

- **API**: cadastro e exclusão de caminhões e itens de carga. Valida nome, placa única e medidas. Calcula volume do caminhão, volume da carga e ocupação (%).
- **Motoristas**: cadastro só com o nome (API `/api/drivers` e página `/motoristas`). Cada caminhão pode ter um motorista, escolhido no cadastro ou trocado na lista de `/caminhoes`.
- **Situação da carga**: `CubageCalculator` (Domain) devolve OK / Atenção / Excede pelo volume; web e mobile só mostram a cor.
- **Banco**: tabelas `Trucks`, `CargoItems` e `Drivers`, criadas quando a API sobe.
- **Blazor**: página inicial (`Home.razor`) com resumo (total de caminhões, volume, ocupação média) e cargas por caminhão; página `/caminhoes` (`Trucks.razor`) com cadastro, lista e exclusão de caminhões. Chama a API.
- **Flutter**: uma tela (`truck_dashboard_screen.dart`) com as mesmas funções, cliente HTTP (`truck_api.dart`) e testes.
- **Docker**: `docker-compose.yml` sobe SQL Server e API na porta 8080.

## Problemas encontrados

| # | Problema | Onde | Gravidade |
| --- | --- | --- | --- |
| 1 | ~~Blazor apontava para a porta 5242; com a API no Docker deveria ser 8080~~ Corrigido | `src/CubagemCaminhao.Blazor/appsettings.json` | — |
| 2 | Cálculo só soma volumes: não confere se o item cabe nas medidas, não tem peso, aceita ocupação acima de 100% sem alerta | `Endpoints/TruckEndpoints.cs` | Alta: é o coração do tema |
| 3 | Carga ligada direto ao caminhão, sem "carregamento" | `Domain/Entities` | Média |
| 4 | Banco criado com `EnsureCreatedAsync()`, sem migrations | `Api/Program.cs` | Média |
| 5 | Sem login e senha do banco no código | compose e appsettings | Média |
| 6 | Não dá para editar caminhão nem item, só criar e excluir | API, Blazor, Flutter | Média |
| 7 | `database/01-create-database.sql` não roda: o SQL Server não executa `/docker-entrypoint-initdb.d` (isso é de Postgres e MySQL). E o script só cria uma tabela `SchemaVersion` sem uso | `database/`, compose | Baixa |
| 8 | Sobra do modelo inicial: `WeatherForecast.cs` na API (as páginas `Counter` e `Weather` do Blazor já foram removidas) | API | Baixa |
| 9 | Blazor não está no `docker-compose.yml` | compose | Baixa (requisito pede só API e banco) |
| 10 | O zip original tinha 80 MB por levar `bin/`, `obj/` e `.gradle` | `PIM_IV.zip` | Baixa: gerar o zip sem essas pastas |

## Resumo

A base técnica está montada e organizada. O que falta é a parte que dá sentido ao tema: regras de cubagem completas (peso, encaixe, alertas), o conceito de carregamento e o mínimo de segurança.
