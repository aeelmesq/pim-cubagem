# Estado atual do código

Revisão de 07/10/2026, feita sobre `PIM_IV.zip` (hoje extraído em `projeto/`), atualizada em 11/11/2026. A solução .NET compila sem erros nem avisos (`dotnet build CubagemCaminhao.slnx`) e os testes do cálculo passam (`dotnet test CubagemCaminhao.slnx`).

## O que funciona

- **API**: cadastro e exclusão de caminhões e itens de carga. Valida nome, placa única, medidas, capacidade e peso. Recusa item que não cabe no baú (nem girando). Calcula volume, volume útil, ocupação (%), peso total, peso cubado e peso para frete.
- **Motoristas**: cadastro só com o nome (API `/api/drivers` e página `/motoristas`). Cada caminhão pode ter um motorista, escolhido no cadastro ou trocado na lista de `/caminhoes`.
- **Login do motorista**: o motorista é cadastrado com usuário e senha (só o hash é gravado). Ele entra em `/entrar` e vê em `/minha-carga` o caminhão dele, a carga atual e o histórico. A API (`/api/auth/login`, `/api/me`) usa JWT num cookie HttpOnly e limita as tentativas de login.
- **Histórico de cargas**: o botão "Registrar saída" no Início grava a carga do caminhão em `Loads`/`LoadItems`, com cópia da placa, do motorista e dos resultados, e esvazia o caminhão. A página `/historico` lista as saídas, com filtro por caminhão. O histórico continua mesmo se o caminhão ou o motorista forem excluídos.
- **Situação da carga**: `CubageCalculator` (Domain) devolve OK / Atenção / Excede pelo volume e pelo peso; web e mobile só mostram a cor. Testes em `tests/CubagemCaminhao.Domain.Tests`.
- **Banco**: tabelas `Trucks`, `CargoItems`, `Drivers`, `Loads` e `LoadItems`, criadas quando a API sobe.
- **Blazor**: página inicial (`Home.razor`) com resumo (total de caminhões, volume, ocupação média) e cargas por caminhão; página `/caminhoes` (`Trucks.razor`) com cadastro, lista e exclusão de caminhões. Chama a API.
- **Flutter**: uma tela (`truck_dashboard_screen.dart`) com as mesmas funções, cliente HTTP (`truck_api.dart`) e testes.
- **Docker**: `docker-compose.yml` sobe SQL Server e API na porta 8080.

## Problemas encontrados

| # | Problema | Onde | Gravidade |
| --- | --- | --- | --- |
| 1 | ~~Blazor apontava para a porta 5242; com a API no Docker deveria ser 8080~~ Corrigido | `src/CubagemCaminhao.Blazor/appsettings.json` | — |
| 2 | ~~Cálculo só soma volumes: não confere se o item cabe nas medidas, não tem peso, aceita ocupação acima de 100% sem alerta~~ Corrigido | `Domain/CubageCalculator.cs` | — |
| 3 | Carga ligada direto ao caminhão. ~~Sem histórico~~ Ao registrar a saída, a carga vai para o histórico (`Loads`) | `Domain/Entities` | Baixa |
| 4 | Banco criado com `EnsureCreatedAsync()`, sem migrations | `Api/Program.cs` | Média |
| 5 | ~~Sem login~~ Motorista tem login (JWT em cookie HttpOnly). As telas de cadastro ainda não pedem login. ~~Senha do banco no código~~ (corrigido: `.env`) | API, Blazor | Média |
| 6 | Não dá para editar caminhão nem item, só criar e excluir | API, Blazor, Flutter | Média |
| 7 | ~~`database/01-create-database.sql` não rodava~~ Removido | — | — |
| 8 | ~~Sobras do modelo inicial (`WeatherForecast.cs`, textos em inglês no Blazor)~~ Removidas | — | — |
| 9 | Blazor não está no `docker-compose.yml` | compose | Baixa (requisito pede só API e banco) |
| 10 | O zip original tinha 80 MB por levar `bin/`, `obj/` e `.gradle` | `PIM_IV.zip` | Baixa: gerar o zip sem essas pastas |

## Resumo

As regras de cubagem do tema (volume, peso, encaixe, alertas) estão prontas e testadas. O motorista tem login e as saídas ficam no histórico. Ficaram de fora, como melhoria futura: login para quem cadastra (Admin/Operador), edição de caminhão e item, login no app Flutter e migrations.
