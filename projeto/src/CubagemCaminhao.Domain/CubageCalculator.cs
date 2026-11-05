namespace CubagemCaminhao.Domain;

// Regras de cubagem. Ver base-conhecimento/03-regras-de-calculo.md.
public static class CubageCalculator
{
    // Parte do baú que dá para usar na prática (85%).
    public const decimal UsableFactor = 0.85m;

    public const string StatusOk = "OK";
    public const string StatusWarning = "Atenção";
    public const string StatusExceeded = "Excede";

    // Converte cm³ em m³.
    public static decimal GetVolumeM3(decimal lengthCm, decimal widthCm, decimal heightCm) =>
        lengthCm * widthCm * heightCm / 1_000_000m;

    public static decimal GetOccupancyPercent(decimal cargoVolumeM3, decimal truckVolumeM3) =>
        truckVolumeM3 == 0 ? 0 : cargoVolumeM3 / truckVolumeM3 * 100;

    // OK: cabe no volume útil. Atenção: passa do útil, mas não do baú. Excede: passa do baú.
    public static string GetStatus(decimal cargoVolumeM3, decimal truckVolumeM3)
    {
        if (cargoVolumeM3 > truckVolumeM3)
        {
            return StatusExceeded;
        }

        if (cargoVolumeM3 > truckVolumeM3 * UsableFactor)
        {
            return StatusWarning;
        }

        return StatusOk;
    }
}
