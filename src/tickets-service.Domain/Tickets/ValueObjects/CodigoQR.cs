using System;

namespace tickets_service.Domain.Tickets.ValueObjects;

/// <summary>
/// Valor que encapsula la información del código QR emitido para un ticket.
/// </summary>
public sealed class CodigoQR
{
    private CodigoQR(string valor, byte[] imagen)
    {
        Valor = valor;
        Imagen = imagen;
    }

    public string Valor { get; }

    public byte[] Imagen { get; }

    /// <summary>Crea una instancia validando el formato básico.</summary>
    public static CodigoQR Create(string valor, byte[] imagen)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("El valor del código QR es obligatorio.", nameof(valor));
        }

        if (imagen is null || imagen.Length == 0)
        {
            throw new ArgumentException("La representación del código QR es obligatoria.", nameof(imagen));
        }

        return new CodigoQR(valor, imagen);
    }
}

