# Visão geral

O sistema diz se uma carga cabe num caminhão e quanto do espaço ela ocupa. Isso ajuda a escolher o veículo certo e evita mandar caminhão meio vazio ou com excesso.

## O problema

Empresas que transportam mercadoria precisam decidir, antes de carregar:

- se a carga cabe no baú (espaço);
- se o caminhão aguenta o peso;
- quanto vai custar o frete, que muitas vezes é cobrado pelo volume e não só pelo peso.

Fazer essa conta na mão, em planilha ou "no olho", gera erro: caminhão que volta para buscar o resto, frete pago a mais, multa por excesso de peso.

## O que o sistema faz (escopo do PIM)

- Cadastrar caminhões com as medidas internas do baú e a capacidade de peso.
- Cadastrar itens de carga com medidas, peso e quantidade.
- Calcular volume, ocupação (%) e peso total de cada carregamento.
- Avisar quando um item não cabe nas medidas do baú, quando o volume passa do limite ou quando o peso passa da capacidade.
- Calcular o peso cubado, que é o valor usado pelas transportadoras para cobrar frete.
- Funcionar pela web (Blazor) e pelo celular (Flutter), com os dados num só banco, acessado só pela API.

## O que fica de fora

- **Arrumação ideal das caixas em 3D.** Descobrir a melhor posição de cada caixa é um problema matemático difícil (empacotamento 3D, ou "bin packing"). Sistemas comerciais usam algoritmos pesados para isso. No PIM, usamos regras simples e mostramos essa limitação no trabalho escrito, como possível melhoria futura.
- Rota, rastreamento, nota fiscal, integração com transportadora.
- Distribuição de peso por eixo.

## Quem usa

| Perfil | O que faz |
| --- | --- |
| Administrador | Cadastra caminhões e usuários |
| Operador / conferente | Monta o carregamento, adiciona os itens e confere se cabe |
| Motorista (opcional, pelo app) | Consulta o carregamento do seu caminhão |
