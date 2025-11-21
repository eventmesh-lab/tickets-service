using FluentValidation;

namespace tickets_service.Application.Tickets.Commands.GenerarTickets;

public class GenerarTicketsCommandValidator : AbstractValidator<GenerarTicketsCommand>
{
    public GenerarTicketsCommandValidator()
    {
        RuleFor(x => x.EventoId).NotEmpty().WithMessage("EventoId es obligatorio.");
        RuleFor(x => x.ReservaId).NotEmpty().WithMessage("ReservaId es obligatorio.");
        RuleFor(x => x.AsistenteId).NotEmpty().WithMessage("AsistenteId es obligatorio.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Debe existir al menos un ticket a generar.");
        
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Precio).GreaterThan(0).WithMessage("El precio debe ser mayor a cero.");
            item.RuleFor(x => x.CodigoQrValor).NotEmpty().WithMessage("El valor del código QR es obligatorio.");
            item.RuleFor(x => x.CodigoQrImagen).NotEmpty().WithMessage("La imagen del código QR es obligatoria.");
            
            item.RuleFor(x => x.SeccionNombre)
                .NotEmpty()
                .When(x => x.AsientoId.HasValue)
                .WithMessage("Un asiento numerado requiere una sección asignada.");
        });
    }
}

