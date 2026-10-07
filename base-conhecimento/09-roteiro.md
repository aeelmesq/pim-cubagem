# Roteiro

Ordem sugerida até a postagem (26/10 a 14/11/2026). Primeiro o que faz o sistema funcionar e cumprir o tema, depois o que melhora a nota.

## Etapa 1 — Arrumar a base (até ~12/10)

- [ ] Corrigir a porta do Blazor para a API do Docker (8080).
- [ ] Remover `Counter`, `Weather`, `WeatherForecast.cs` e ajustar o título do menu.
- [ ] Trocar `EnsureCreatedAsync()` por migrations.
- [ ] Remover ou corrigir `database/01-create-database.sql` e o mapeamento no compose.
- [ ] Senha do banco num `.env` fora do código.

## Etapa 2 — Regras de cubagem (até ~19/10)

- [ ] Adicionar `MaxLoadKg` e `Type` no caminhão; `WeightKg` e `CanRotate` no item.
- [ ] Criar a classe de cálculo no Domain com as regras de [Regras de cálculo](03-regras-de-calculo.md).
- [ ] Testes unitários dessa classe com os casos de teste listados lá.
- [ ] API devolve situação (OK / Atenção / Excede), peso total, peso cubado.
- [ ] Web e mobile mostram a situação com cor e mensagem.

## Etapa 3 — Carregamento e edição (até ~26/10)

- [ ] Entidade `Load` entre caminhão e itens.
- [ ] Endpoints de edição (PUT) de caminhão e item.
- [ ] Telas de edição na web e no app.

## Etapa 4 — Segurança (até ~02/11)

- [ ] Login com perfis Admin e Operador.
- [ ] Rotas da API protegidas por token.
- [ ] Texto curto de privacidade (quais dados guarda e por quê).

## Etapa 5 — Entrega (até ~10/11, com folga antes de 14/11)

- [ ] Trabalho escrito: problema, conceitos, regras, arquitetura, modelo de dados, segurança/LGPD, limitações.
- [ ] Prints das telas web e mobile.
- [ ] README com passo a passo para rodar.
- [ ] Zip limpo, sem `bin/`, `obj/`, `.gradle`, `build/`, `.dart_tool/`.
- [ ] Ensaio da apresentação.

Se o tempo apertar, a ordem de corte é: Etapa 4 parcial (manter só login simples), depois edição da Etapa 3. As Etapas 1 e 2 não devem ser cortadas.
