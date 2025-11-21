using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace tickets_service.Infrastructure.Gateways.Dtos;

public record ExternalEventDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("estado")] string Estado,
    [property: JsonPropertyName("secciones")] List<ExternalSectionDto>? Secciones
);

public record ExternalSectionDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("nombre")] string Nombre,
    [property: JsonPropertyName("capacidad")] int Capacidad
);
