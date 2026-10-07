# Regras de cálculo

São as regras que o sistema deve aplicar. Ficam todas na API, para que web e mobile mostrem sempre o mesmo resultado.

## Entradas

- Caminhão: comprimento, largura e altura internos (cm) e capacidade de carga (kg).
- Item: comprimento, largura, altura (cm), peso unitário (kg), quantidade e se pode girar.
- Configuração: fator de aproveitamento (padrão 0,85) e fator de cubagem (padrão 300 kg/m³).

## Regras, em ordem

1. **Volume do baú (m³)** = C × L × A ÷ 1.000.000
2. **Volume útil (m³)** = volume do baú × fator de aproveitamento
3. **Volume do item (m³)** = C × L × A ÷ 1.000.000 × quantidade
4. **O item cabe nas medidas?** Cada item precisa caber sozinho no baú:
   - altura do item ≤ altura do baú (a altura não gira: "este lado para cima");
   - e, no chão, (comprimento ≤ comprimento do baú **e** largura ≤ largura do baú) **ou**, girando 90°, (largura ≤ comprimento do baú **e** comprimento ≤ largura do baú).
   - Se não couber, o item é recusado com a mensagem explicando qual medida passou.
5. **Volume da carga** = soma dos volumes dos itens
6. **Ocupação (%)** = volume da carga ÷ volume do baú × 100
7. **Peso total (kg)** = soma de (peso unitário × quantidade)
8. **Peso cubado (kg)** = volume da carga × fator de cubagem
9. **Peso para frete (kg)** = o maior entre peso total e peso cubado
10. **Situação do carregamento:**

| Situação | Quando |
| --- | --- |
| OK | volume da carga ≤ volume útil **e** peso total ≤ capacidade |
| Atenção | volume da carga entre o volume útil e o volume do baú (pode não caber na prática) |
| Excede | volume da carga > volume do baú **ou** peso total > capacidade |

O sistema deve **permitir** registrar um carregamento que excede, mas mostrar o alerta. Assim o usuário vê o problema em vez de só receber um erro.

## Exemplo resolvido

Caminhão toco, baú interno 700 × 250 × 260 cm, capacidade 6.000 kg.
Carga: 200 caixas de 60 × 40 × 50 cm, 15 kg cada.

| Passo | Conta | Resultado |
| --- | --- | --- |
| Volume do baú | 700 × 250 × 260 ÷ 1.000.000 | 45,50 m³ |
| Volume útil | 45,50 × 0,85 | 38,68 m³ |
| Cabe nas medidas? | 50 ≤ 260; 60 ≤ 700 e 40 ≤ 250 | Sim |
| Volume da carga | 0,12 × 200 | 24,00 m³ |
| Ocupação | 24,00 ÷ 45,50 × 100 | 52,7% |
| Peso total | 15 × 200 | 3.000 kg |
| Peso cubado | 24,00 × 300 | 7.200 kg |
| Peso para frete | maior entre 3.000 e 7.200 | 7.200 kg |
| Situação | 24,00 ≤ 38,68 e 3.000 ≤ 6.000 | OK |

## Extra opcional: quantas caixas iguais cabem

Para um item só, dá para estimar quantas unidades cabem empilhando em grade (sem misturar posições):

Unidades = (comprimento do baú ÷ comprimento da caixa, arredondado para baixo) × (largura do baú ÷ largura da caixa, arredondado para baixo) × (altura do baú ÷ altura da caixa, arredondado para baixo)

No exemplo:

- Sem girar: 700÷60 → 11; 250÷40 → 6; 260÷50 → 5. Total: 11 × 6 × 5 = **330 caixas**.
- Girando 90°: 700÷40 → 17; 250÷60 → 4; 260÷50 → 5. Total: 17 × 4 × 5 = **340 caixas**.

O sistema mostra o maior dos dois. É uma conta simples, fácil de explicar na apresentação e mais realista que só dividir volumes (45,5 ÷ 0,12 daria 379, que não cabem de verdade).

## Limitações (citar no trabalho escrito)

- Não calcula a posição de cada caixa nem mistura tamanhos diferentes de forma ideal.
- Não considera distribuição de peso por eixo, carga frágil ou ordem de descarga.
- O fator de aproveitamento é uma estimativa.

## Casos de teste sugeridos

| Caso | Esperado |
| --- | --- |
| Item com altura maior que o baú | Recusado: "altura do item maior que a do baú" |
| Item 300 × 100 cm num baú de 250 de largura e 700 de comprimento | Cabe girando (300 ≤ 700 e 100 ≤ 250) |
| Item mais largo e mais comprido que o baú nas duas posições | Recusado |
| Carga com 90% do volume do baú | Atenção |
| Carga com 110% do volume | Excede |
| Peso acima da capacidade com pouco volume | Excede |
| Medida zero ou negativa | Recusado na validação |
