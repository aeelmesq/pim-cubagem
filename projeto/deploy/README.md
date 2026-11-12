# Deploy na VPS

O site fica em `https://191-252-184-167.sslip.io` (Blazor em `/`, app web em `/app/`, API em `/api/`).
O navegador pede usuário e senha, que estão no `deploy/.env` (fora do Git).

## Como funciona

```
Navegador / app ──HTTPS──> nginx (VPS) ──> Blazor ──> API ──> 172.30.0.1:1433 ──túnel SSH──> SQL Server (seu PC)
```

- **VPS (1 GB):** só nginx, API e Blazor, em Docker (`docker-compose.prod.yml`). Só o nginx abre porta (80 e 443).
- **Banco:** continua no seu PC (`docker compose up` da pasta `projeto/`). O serviço `pim-tunnel` liga o banco até a VPS por SSH. Com o PC desligado, o site abre mas a API não responde.
- **Nome do site:** o sslip.io transforma o IP em nome (`191-252-184-167.sslip.io`), sem cadastro. Se trocar de VPS, troque o nome no `deploy/.env`.

## Publicar uma versão nova

```bash
cd projeto
./deploy/deploy.sh
```

Monta as imagens e o app web aqui, envia prontos para a VPS e sobe. A VPS não precisa do código.

## Primeira vez neste PC

```bash
cp deploy/.env.example deploy/.env      # troque as senhas
./deploy/criar-usuario-banco.sh         # usuário cubagem_api no SQL Server (a API não usa o sa)
./deploy/instalar-tunel.sh              # serviço pim-tunnel (liga junto com a sessão)
```

Comandos do túnel: `systemctl --user status pim-tunnel`, `stop`, `start`.

## O que foi configurado na VPS (uma vez só)

Feito como root, com acesso por chave SSH:

- Ubuntu atualizado, atualizações de segurança automáticas (`unattended-upgrades`), fuso `America/Sao_Paulo`.
- Swap para aguentar picos de memória.
- Usuário `deploy` (sudo e docker) com a chave `~/.ssh/id_ed25519`. No PC, o atalho `ssh pim-vps`.
- SSH em `/etc/ssh/sshd_config.d/10-hardening.conf`: sem root, sem senha, só `deploy` e `tunnel`, no máximo 3 tentativas.
- Usuário `tunnel`, sem shell, que só pode abrir o túnel do banco. Linha em `/home/tunnel/.ssh/authorized_keys`:
  `restrict,port-forwarding,permitlisten="172.30.0.1:1433" <conteúdo de ~/.ssh/pim_tunnel.pub>`
- Firewall `ufw`: entrada só nas portas 22, 80 e 443, mais a porta 1433 liberada só da rede dos containers (`br-cubagem`) para o túnel.
- `fail2ban` no SSH: 5 falhas bloqueiam o IP por 1 hora.
- Docker e o plugin compose.
- Certificado Let's Encrypt com `certbot certonly --standalone`. A renovação é automática (`certbot.timer`) e para o nginx por alguns segundos.

## Segurança

- HTTPS obrigatório (HTTP redireciona), com cabeçalhos HSTS, `nosniff` e `X-Frame-Options`.
- Site, app e API pedem senha (Basic Auth do nginx), porque o sistema não tem login.
- Limite de 10 requisições por segundo por IP na API (o excesso recebe 429).
- O banco não tem porta aberta na internet: só os containers da VPS alcançam o túnel.
- Quem acessa pelo IP, sem o nome, não recebe nada.

## APK

O APK precisa da senha do site, porque o app não tem tela de login:

```bash
flutter build apk --dart-define=API_BASE_URL=https://191-252-184-167.sslip.io --dart-define=API_BASIC_AUTH=usuario:senha
```

A senha fica dentro do APK, então ele só pode ser passado para o grupo. Precisa do Android SDK instalado.
