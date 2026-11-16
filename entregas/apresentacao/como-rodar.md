# Como rodar o sistema na apresentação

O site e a API rodam na VPS, mas o banco fica no notebook. **Sem o notebook ligado, o site abre mas não mostra dados.**

- Web: `https://191-252-184-167.sslip.io`
- App no navegador do celular: `https://191-252-184-167.sslip.io/app/`
- Usuário e senha do site: em `projeto/deploy/.env` (`BASIC_AUTH_USER` e `BASIC_AUTH_PASSWORD`)

## Na véspera

1. Grave a senha do site no navegador do notebook e do celular (entre uma vez e mande salvar). Assim ninguém vê a senha sendo digitada no projetor.
2. Instale o APK no celular, se for mostrar o app nativo (veja [o README do deploy](../../projeto/deploy/README.md#apk)).
3. Faça o teste completo do dia (abaixo) uma vez.
4. Se quiser começar do zero, apague os caminhões de teste pela própria tela de Caminhões.

## No dia, 30 minutos antes

No notebook, na pasta `projeto/`:

```bash
# 1. Sobe o banco
docker compose up -d sqlserver

# 2. Confere o túnel até a VPS (tem que aparecer "active")
systemctl --user status pim-tunnel --no-pager | head -3
```

Se o túnel não estiver `active`: `systemctl --user restart pim-tunnel` e espere 1 minuto.

Depois:

3. Abra `https://191-252-184-167.sslip.io/caminhoes` no notebook. A lista tem que carregar.
4. Abra `https://191-252-184-167.sslip.io/app/` no celular, de preferência no 4G (Wi-Fi da faculdade pode bloquear).
5. Desligue a suspensão automática do notebook. Se ele dormir, o túnel cai e o site para de mostrar dados.

## Roteiro da demonstração

Os números são os do exemplo de [03-regras-de-calculo](../../base-conhecimento/03-regras-de-calculo.md):

1. **Motoristas:** cadastre um motorista.
2. **Caminhões:** cadastre o caminhão "Toco", baú 700 × 250 × 260 cm, capacidade 6.000 kg, com o motorista.
3. **Início:** abra o Toco e adicione 200 caixas de 60 × 40 × 50 cm com 15 kg cada. Resultado: **OK · 52,7%**, 3.000 / 6.000 kg.
4. Adicione 80 sacos de cimento de 40 × 30 × 10 cm com 50 kg cada. Resultado: **Excede** (passa do peso).
5. Tente adicionar um item de 300 cm de altura: o sistema recusa porque não cabe no baú.
6. Mostre o mesmo caminhão no celular.

## Se algo der errado

| O que aparece | O que fazer |
| --- | --- |
| Site pede senha e não aceita | Confira usuário e senha no `projeto/deploy/.env` |
| Site abre, mas a lista dá erro | Banco ou túnel parado: rode os passos 1 e 2 de novo |
| Site não abre | VPS ou internet fora: use o plano B |

## Plano B: tudo no notebook, sem internet

Na pasta `projeto/`, em três terminais:

```bash
# Terminal 1: banco e API
docker compose up --build

# Terminal 2: web (abre em http://localhost:5107)
cd src/CubagemCaminhao.Blazor
dotnet run

# Terminal 3: app no Chrome
cd src/CubagemCaminhao.Mobile
flutter run -d chrome
```

Os dados são os mesmos, porque o banco é o do notebook nos dois casos.

## Publicar uma versão nova antes da apresentação

```bash
cd projeto
./deploy/deploy.sh
```
