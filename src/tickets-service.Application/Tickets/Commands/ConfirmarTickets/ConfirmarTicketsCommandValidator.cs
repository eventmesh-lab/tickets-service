using FluentValidation;

namespace tickets_service.Application.Tickets.Commands.ConfirmarTickets;

public class ConfirmarTicketsCommandValidator : AbstractValidator<ConfirmarTicketsCommand>
{
    public ConfirmarTicketsCommandValidator()
    {
        RuleFor(x => x.PagoId).NotEmpty().WithMessage("PagoId es obligatorio.");
        RuleFor(x => x.TicketIds).NotEmpty().WithMessage("Se requieren tickets para confirmar.");
    }
}

