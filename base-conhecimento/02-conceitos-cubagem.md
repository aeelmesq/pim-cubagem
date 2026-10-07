# Conceitos de cubagem

Cubagem é medir o espaço que uma carga ocupa, em metros cúbicos (m³), e comparar com o espaço disponível no veículo.

## Volume

Volume de uma caixa = comprimento × largura × altura.

O sistema guarda as medidas em centímetros. Para converter cm³ em m³, divide por 1.000.000 (porque 1 m = 100 cm, e 100 × 100 × 100 = 1.000.000).

Exemplo: caixa de 60 × 40 × 50 cm = 120.000 cm³ = 0,12 m³.

## Medida interna x medida externa

Para cubagem vale a medida **interna** do baú (o espaço útil). A medida externa do caminhão serve para trânsito (altura de viaduto, largura de rua), não para saber o que cabe.

## Taxa de ocupação

Ocupação (%) = volume da carga ÷ volume do baú × 100.

Na prática, um baú nunca fica 100% cheio: sobram vãos entre as caixas, há caixas que não podem ser empilhadas, e é preciso espaço para manusear. Por isso se usa um **fator de aproveitamento**, em geral entre 80% e 90% do volume total. O sistema usa 85% como padrão (valor configurável).

## Peso real e capacidade de carga

- **Peso real**: o que a balança marca.
- **Capacidade de carga**: quanto o caminhão pode levar de mercadoria. É diferente do peso bruto total (PBT), que inclui o próprio caminhão.

Uma carga pode caber no espaço e mesmo assim passar do peso (ex.: sacos de cimento), ou estar dentro do peso e não caber no espaço (ex.: espuma, isopor). Por isso o sistema confere as duas coisas.

## Peso cubado e fator de cubagem

Transportadoras cobram o frete pelo **maior** valor entre o peso real e o **peso cubado**. Assim uma carga leve e volumosa paga pelo espaço que ocupa.

Peso cubado = volume (m³) × fator de cubagem (kg/m³).

| Modal | Fator de cubagem usual |
| --- | --- |
| Rodoviário | 300 kg/m³ |
| Aéreo | 166,7 kg/m³ (equivale a 6.000 cm³ por kg) |

O fator rodoviário de 300 kg/m³ é o padrão de mercado no Brasil, mas cada transportadora pode usar outro. Por isso ele deve ser configurável no sistema.

Exemplo: 24 m³ de caixas pesando 3.000 kg no total.
Peso cubado = 24 × 300 = 7.200 kg. Como 7.200 > 3.000, o frete é cobrado sobre 7.200 kg.
