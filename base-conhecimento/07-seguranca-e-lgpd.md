# Segurança e LGPD

O mínimo bem feito vale mais que muita coisa pela metade. A lista abaixo cabe num PIM e rende assunto para o trabalho escrito e para a oficina de extensão.

## Dados pessoais no sistema

| Dado | É dado pessoal? | Decisão |
| --- | --- | --- |
| Medidas e peso de carga | Não | Livre |
| Placa do caminhão | Pode ser, se ligada a uma pessoa (dono autônomo) | Guardar, é necessária |
| Nome e e-mail do usuário | Sim | Guardar só para login |
| Nome do motorista | Sim | Guardar, é o que liga o motorista ao caminhão. Só o nome |
| Usuário e senha do motorista | Sim | Guardar só para login. A senha fica só como hash |
| Histórico de cargas (nome do motorista, placa, data) | Sim, ligado ao motorista | Guardar: é o registro da operação. Fica com cópia do nome mesmo se o motorista for excluído |
| Senha | Sim, sensível para segurança | Guardar só o hash, nunca a senha |
| CPF, CNH, telefone do motorista | Sim | **Não coletar.** O sistema não precisa deles |

Princípios da LGPD que aparecem no projeto (Lei 13.709/2018, art. 6º):

- **Finalidade**: cada dado tem um motivo para existir.
- **Necessidade (minimização)**: não coletar o que não usa. Por isso nada de CPF.
- **Segurança**: proteger contra acesso indevido.
- **Transparência**: uma tela ou texto curto dizendo quais dados o sistema guarda e para quê.

## Checklist técnico

- [x] Login do motorista com senha guardada como hash: `PasswordHasher` do ASP.NET Core (PBKDF2-HMAC-SHA512, 100 mil iterações, salt aleatório). Usuário inexistente e senha errada dão a mesma mensagem e demoram o mesmo tempo.
- [x] JWT assinado (HMAC-SHA256, vale 8 horas) num cookie HttpOnly, `SameSite=Strict` e `Secure` em HTTPS: o JavaScript não lê o token (XSS) e outros sites não conseguem usá-lo (CSRF).
- [x] Limite de 10 tentativas de login por minuto por IP (o excesso recebe 429).
- [ ] API exige token (JWT) nas rotas, exceto login e `/health`. Hoje só `/api/me` (área do motorista) exige; as rotas de cadastro ainda não.
- [ ] Dois perfis: Admin e Operador. Só Admin exclui caminhão e cadastra usuário.
- [x] Senha do banco fora do código: arquivo `.env` (no `.gitignore`) lido pelo `docker-compose.yml`, e `dotnet user-secrets` para rodar a API com `dotnet run`.
- [x] Banco sem porta aberta para fora: no PC, a 1433 só aceita o próprio PC; na VPS, o banco chega por túnel SSH que só os containers alcançam.
- [x] Usuário do banco próprio para a API em produção (`cubagem_api`), em vez do `sa`.
- [ ] Validação de entrada na API (já existe para nomes, placa e medidas).
- [ ] Consultas pelo Entity Framework, que evita SQL injection (já é assim).
- [ ] CORS restrito às origens conhecidas (já existe para desenvolvimento).
- [x] HTTPS fora do ambiente local (nginx com Let's Encrypt, HTTP redireciona). Veja `projeto/deploy/README.md`.
- [ ] Logs sem senha, token ou dado pessoal.
- [ ] Mensagens de erro para o usuário sem detalhe técnico (stack trace só no log).

## O que já está bom no código

- Validações claras com mensagem em português.
- Placa única no banco.
- CORS liberado só para `localhost` em desenvolvimento.

## O que precisa corrigir

- Só a área do motorista pede login; as telas de cadastro não. Na VPS, o nginx pede usuário e senha (Basic Auth) para o site inteiro, como proteção até existir login para quem cadastra. A exceção é a área do motorista (`/entrar`, `/minha-carga` e os arquivos de CSS/JS delas), que já tem o login próprio.
- Trocar a senha do motorista não derruba um login já feito: o JWT antigo vale até vencer (8 horas). Excluir o motorista derruba na hora, porque `/api/me` confere se ele ainda existe.
