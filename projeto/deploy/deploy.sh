#!/usr/bin/env bash
# Publica API, Blazor e app web na VPS. Rode da pasta projeto/: ./deploy/deploy.sh
# Precisa do deploy/.env (copie do deploy/.env.example). As imagens são montadas aqui
# e enviadas prontas, assim a VPS não precisa do código nem de acesso ao GitHub.
set -euo pipefail
cd "$(dirname "$0")/.."
source deploy/.env

# IdentitiesOnly: sem isso o ssh testa todas as chaves do agente e o fail2ban bloqueia o IP.
export SSH_OPTS="-o IdentitiesOnly=yes -i $HOME/.ssh/id_ed25519"
vps="deploy@${VPS_HOST}"
ssh() { command ssh $SSH_OPTS "$@"; }
scp() { command scp $SSH_OPTS "$@"; }
export RSYNC_RSH="ssh $SSH_OPTS"
remote_dir=cubagem

echo "== Montando as imagens"
docker build -q -t cubagem-api:latest -f src/CubagemCaminhao.Api/Dockerfile .
docker build -q -t cubagem-blazor:latest src/CubagemCaminhao.Blazor

echo "== Montando o app web"
(cd src/CubagemCaminhao.Mobile && flutter build web --release --base-href /app/ \
  --dart-define=API_BASE_URL="https://${NGINX_HOST}")

echo "== Enviando para a VPS"
docker save cubagem-api:latest cubagem-blazor:latest | gzip | ssh "$vps" 'gunzip | docker load -q'
ssh "$vps" "mkdir -p $remote_dir/nginx $remote_dir/flutter-web"
rsync -a --delete src/CubagemCaminhao.Mobile/build/web/ "$vps:$remote_dir/flutter-web/"
scp -q deploy/docker-compose.prod.yml "$vps:$remote_dir/docker-compose.yml"
scp -q deploy/nginx/default.conf.template "$vps:$remote_dir/nginx/"

# Na VPS vai só o hash da senha do site, nunca a senha.
printf '%s:%s\n' "$BASIC_AUTH_USER" "$(openssl passwd -apr1 -stdin <<< "$BASIC_AUTH_PASSWORD")" \
  | ssh "$vps" "cat > $remote_dir/nginx/htpasswd"
printf 'NGINX_HOST=%s\nDB_USER=%s\nDB_PASSWORD=%s\n' "$NGINX_HOST" "$DB_USER" "$DB_PASSWORD" \
  | ssh "$vps" "umask 077; cat > $remote_dir/.env"

echo "== Subindo"
ssh "$vps" "cd $remote_dir && docker compose up -d --remove-orphans && docker compose restart nginx && docker image prune -f >/dev/null"
echo "Pronto: https://${NGINX_HOST}"
