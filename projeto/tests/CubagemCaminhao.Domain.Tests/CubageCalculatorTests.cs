using CubagemCaminhao.Domain;

namespace CubagemCaminhao.Domain.Tests;

// Casos do base-conhecimento/03-regras-de-calculo.md.
// Baú de referência: caminhão toco, 700 × 250 × 260 cm, capacidade 6.000 kg.
public class CubageCalculatorTests
{
    private const decimal TruckLengthCm = 700m;
    private const decimal TruckWidthCm = 250m;
    private const decimal TruckHeightCm = 260m;
    private const decimal MaxLoadKg = 6000m;

    private static readonly decimal TruckVolumeM3 =
        CubageCalculator.GetVolumeM3(TruckLengthCm, TruckWidthCm, TruckHeightCm);

    // Exemplo resolvido: 200 caixas de 60 × 40 × 50 cm, 15 kg cada.
    [Fact]
    public void Exemplo_resolvido_da_o_resultado_do_documento()
    {
        var boxVolumeM3 = CubageCalculator.GetVolumeM3(60m, 40m, 50m);
        var cargoVolumeM3 = boxVolumeM3 * 200;
        var totalWeightKg = 15m * 200;
        var cubedWeightKg = CubageCalculator.GetCubedWeightKg(cargoVolumeM3);

        Assert.Equal(45.5m, TruckVolumeM3);
        Assert.Equal(38.68m, Math.Round(CubageCalculator.GetUsableVolumeM3(TruckVolumeM3), 2));
        Assert.Equal(0.12m, boxVolumeM3);
        Assert.Equal(24m, cargoVolumeM3);
        Assert.Equal(52.7m, Math.Round(CubageCalculator.GetOccupancyPercent(cargoVolumeM3, TruckVolumeM3), 1));
        Assert.Equal(3000m, totalWeightKg);
        Assert.Equal(7200m, cubedWeightKg);
        Assert.Equal(7200m, CubageCalculator.GetFreightWeightKg(totalWeightKg, cubedWeightKg));
        Assert.Equal(CubageCalculator.StatusOk,
            CubageCalculator.GetStatus(cargoVolumeM3, TruckVolumeM3, totalWeightKg, MaxLoadKg));
        Assert.Null(CubageCalculator.GetFitError(60m, 40m, 50m, TruckLengthCm, TruckWidthCm, TruckHeightCm));
    }

    [Fact]
    public void Peso_para_frete_e_o_peso_real_quando_ele_e_maior()
    {
        // Carga pesada e pequena: 1 m³ (300 kg cubados) pesando 2.000 kg.
        Assert.Equal(2000m, CubageCalculator.GetFreightWeightKg(2000m, CubageCalculator.GetCubedWeightKg(1m)));
    }

    [Fact]
    public void Item_mais_alto_que_o_bau_e_recusado()
    {
        var error = CubageCalculator.GetFitError(100m, 100m, 300m, TruckLengthCm, TruckWidthCm, TruckHeightCm);

        Assert.NotNull(error);
        Assert.Contains("altura", error);
    }

    [Fact]
    public void Item_que_so_cabe_girando_e_aceito()
    {
        // 300 cm de largura não cabe nos 250 do baú, mas girando fica 300 no comprimento (700) e 100 na largura.
        Assert.Null(CubageCalculator.GetFitError(100m, 300m, 100m, TruckLengthCm, TruckWidthCm, TruckHeightCm));
    }

    [Fact]
    public void Item_maior_que_o_chao_do_bau_nas_duas_posicoes_e_recusado()
    {
        var error = CubageCalculator.GetFitError(800m, 300m, 100m, TruckLengthCm, TruckWidthCm, TruckHeightCm);

        Assert.NotNull(error);
        Assert.Contains("nem girando", error);
    }

    [Fact]
    public void Item_do_tamanho_exato_do_bau_cabe()
    {
        Assert.Null(CubageCalculator.GetFitError(700m, 250m, 260m, TruckLengthCm, TruckWidthCm, TruckHeightCm));
    }

    [Theory]
    [InlineData(50, CubageCalculator.StatusOk)]        // abaixo do volume útil
    [InlineData(85, CubageCalculator.StatusOk)]        // exatamente no volume útil
    [InlineData(90, CubageCalculator.StatusWarning)]   // entre o útil e o baú
    [InlineData(100, CubageCalculator.StatusWarning)]  // exatamente o baú
    [InlineData(110, CubageCalculator.StatusExceeded)] // passa do baú
    public void Situacao_pelo_volume(int percentOfTruck, string expected)
    {
        var cargoVolumeM3 = TruckVolumeM3 * percentOfTruck / 100;

        Assert.Equal(expected, CubageCalculator.GetStatus(cargoVolumeM3, TruckVolumeM3, 1000m, MaxLoadKg));
    }

    [Fact]
    public void Peso_acima_da_capacidade_excede_mesmo_com_pouco_volume()
    {
        // Ex.: sacos de cimento: 10% do volume, mas 7.000 kg num caminhão de 6.000 kg.
        var status = CubageCalculator.GetStatus(TruckVolumeM3 * 0.1m, TruckVolumeM3, 7000m, MaxLoadKg);

        Assert.Equal(CubageCalculator.StatusExceeded, status);
    }

    [Fact]
    public void Peso_igual_a_capacidade_ainda_e_ok()
    {
        Assert.Equal(CubageCalculator.StatusOk,
            CubageCalculator.GetStatus(TruckVolumeM3 * 0.5m, TruckVolumeM3, MaxLoadKg, MaxLoadKg));
    }
}
