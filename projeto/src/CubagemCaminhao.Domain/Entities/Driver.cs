namespace CubagemCaminhao.Domain.Entities;

// Só o nome: o sistema não precisa de CPF, CNH ou telefone (LGPD, minimização).
public sealed class Driver
{
    public int Id { get; set; }
    public required string Name { get; set; }
}
