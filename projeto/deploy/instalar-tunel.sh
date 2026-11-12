#!/usr/bin/env bash
# Instala o túnel do banco neste PC como serviço do usuário (liga junto com a sessão).
set -euo pipefail
cd "$(dirname "$0")/.."
source deploy/.env

[ -f ~/.ssh/pim_tunnel ] || ssh-keygen -q -t ed25519 -N "" -C pim-tunnel -f ~/.ssh/pim_tunnel
echo "Chave pública do túnel (vai na VPS, em /home/tunnel/.ssh/authorized_keys):"
cat ~/.ssh/pim_tunnel.pub

mkdir -p ~/.config/systemd/user
sed "s/VPS_HOST/${VPS_HOST}/" deploy/pim-tunnel.service > ~/.config/systemd/user/pim-tunnel.service
systemctl --user daemon-reload
systemctl --user enable --now pim-tunnel
systemctl --user --no-pager status pim-tunnel | head -5
