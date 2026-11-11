namespace CubagemCaminhao.Domain;

// Regras de cubagem. Ver base-conhecimento/03-regras-de-calculo.md.
public static class CubageCalculator
{
    // Parte do baú que dá para usar na prática (85%).
    public const decimal UsableFactor = 0.85m;

    // Quantos kg cada m³ "vale" no frete rodoviário.
    public const decimal CubingFactorKgPerM3 = 300m;

    public const string StatusOk = "OK";
    public const string StatusWarning = "Atenção";
    public const string StatusExceeded = "Excede";

    // Converte cm³ em m³.
    public static decimal GetVolumeM3(decimal lengthCm, decimal widthCm, decimal heightCm) =>
        lengthCm * widthCm * heightCm / 1_000_000m;

    public static decimal GetUsableVolumeM3(decimal truckVolumeM3) =>
        truckVolumeM3 * UsableFactor;

    public static decimal GetOccupancyPercent(decimal cargoVolumeM3, decimal truckVolumeM3) =>
        truckVolumeM3 == 0 ? 0 : cargoVolumeM3 / truckVolumeM3 * 100;

    // Peso cubado = volume da carga × fator de cubagem.
    public static decimal GetCubedWeightKg(decimal cargoVolumeM3) =>
        cargoVolumeM3 * CubingFactorKgPerM3;

    // O frete é cobrado pelo maior entre o peso real e o peso cubado.
    public static decimal GetFreightWeightKg(decimal totalWeightKg, decimal cubedWeightKg) =>
        Math.Max(totalWeightKg, cubedWeightKg);

    // Excede: passa do volume do baú ou da capacidade de peso.
    // Atenção: passa do volume útil (85%), mas não do baú.
    // OK: cabe no volume útil e no peso.
    public static string GetStatus(decimal cargoVolumeM3, decimal truckVolumeM3, decimal totalWeightKg, decimal maxLoadKg)
    {
        if (cargoVolumeM3 > truckVolumeM3 || totalWeightKg > maxLoadKg)
        {
            return StatusExceeded;
        }

        if (cargoVolumeM3 > GetUsableVolumeM3(truckVolumeM3))
        {
            return StatusWarning;
        }

        return StatusOk;
    }

    // Confere se um item cabe sozinho no baú. Devolve null se cabe, ou a mensagem dizendo o que passou.
    // A altura não gira ("este lado para cima"). No chão, o item pode girar 90°.
    public static string? GetFitError(
        decimal itemLengthCm, decimal itemWidthCm, decimal itemHeightCm,
        decimal truckLengthCm, decimal truckWidthCm, decimal truckHeightCm)
    {
        if (itemHeightCm > truckHeightCm)
        {
            return $"A altura do item ({itemHeightCm:0.##} cm) é maior que a do baú ({truckHeightCm:0.##} cm).";
        }

        var fitsStraight = itemLengthCm <= truckLengthCm && itemWidthCm <= truckWidthCm;
        var fitsRotated = itemWidthCm <= truckLengthCm && itemLengthCm <= truckWidthCm;
        if (!fitsStraight && !fitsRotated)
        {
            return $"O item ({itemLengthCm:0.##} × {itemWidthCm:0.##} cm) não cabe no chão do baú " +
                   $"({truckLengthCm:0.##} × {truckWidthCm:0.##} cm), nem girando.";
        }

        return null;
    }
}
