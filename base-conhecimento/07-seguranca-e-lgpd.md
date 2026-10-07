# Segurança e LGPD

O mínimo bem feito vale mais que muita coisa pela metade. A lista abaixo cabe num PIM e rende assunto para o trabalho escrito e para a oficina de extensão.

## Dados pessoais no sistema

| Dado | É dado pessoal? | Decisão |
| --- | --- | --- |
| Medidas e peso de carga | Não | Livre |
| Placa do caminhão | Pode ser, se ligada a uma pessoa (dono autônomo) | Guardar, é necessária |
| Nome e e-mail do usuário | Sim | Guardar só para login |
| Senha | Sim, sensível para segurança | Guardar só o hash, nunca a senha |
| CPF, CNH, telefone do motorista | Sim | **Não coletar.** O sistema não precisa deles |

Princípios da LGPD que aparecem no projeto (Lei 13.709/2018, art. 6º):

- **Finalidade**: cada dado tem um motivo para existir.
- **Necessidade (minimização)**: não coletar o que não usa. Por isso nada de CPF.
- **Segurança**: proteger contra acesso indevido.
- **Transparência**: uma tela ou texto curto dizendo quais dados o sistema guarda e para quê.

## Checklist técnico

- [ ] Login com senha guardada como hash (ASP.NET Core Identity já faz isso).
- [ ] API exige token (JWT) nas rotas, exceto login e `/health`.
- [ ] Dois perfis: Admin e Operador. Só Admin exclui caminhão e cadastra usuário.
- [ ] Senha do banco fora do código: arquivo `.env` (no `.gitignore`) lido pelo `docker-compose.yml`, ou `dotnet user-secrets` em desenvolvimento.
- [ ] Banco sem porta aberta para fora em produção (no compose, a porta 1433 só é necessária para desenvolvimento).
- [ ] Usuário do banco próprio para a API, em vez do `sa`.
- [ ] Validação de entrada na API (já existe para nomes, placa e medidas).
- [ ] Consultas pelo Entity Framework, que evita SQL injection (já é assim).
- [ ] CORS restrito às origens conhecidas (já existe para desenvolvimento).
- [ ] HTTPS fora do ambiente local.
- [ ] Logs sem senha, token ou dado pessoal.
- [ ] Mensagens de erro para o usuário sem detalhe técnico (stack trace só no log).

## O que já está bom no código

- Validações claras com mensagem em português.
- Placa única no banco.
- CORS liberado só para `localhost` em desenvolvimento.

## O que precisa corrigir

- Senha `P@ssw0rd!` do `sa` está escrita em `docker-compose.yml` e `appsettings.json`.
- Nenhuma rota pede login.
