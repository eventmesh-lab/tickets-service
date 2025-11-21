using FluentValidation;

namespace tickets_service.Application.Tickets.Commands.ValidarTicket;

public class ValidarTicketCommandValidator : AbstractValidator<ValidarTicketCommand>
{
    public ValidarTicketCommandValidator()
    {
        RuleFor(x => x.CodigoQr).NotEmpty().WithMessage("El código QR es obligatorio.");
        RuleFor(x => x.UbicacionValidacion).NotEmpty().WithMessage("La ubicación de validación es obligatoria.");
        RuleFor(x => x.UsuarioValidadorId).NotEmpty().WithMessage("El usuario validador es obligatorio.");
    }
}

