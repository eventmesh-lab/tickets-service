using FluentValidation;

namespace tickets_service.Application.Tickets.Commands.CancelarTicket;

public class CancelarTicketCommandValidator : AbstractValidator<CancelarTicketCommand>
{
    public CancelarTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty().WithMessage("TicketId es obligatorio.");
        RuleFor(x => x.Razon).NotEmpty().WithMessage("Debe indicarse la razón de cancelación.");
    }
}

