using FluentValidation;
using PuestosApi.DTOs;

namespace PuestosApi.Validators;

public class PuestoCreateValidator : AbstractValidator<PuestoCreateDto>
{
    public PuestoCreateValidator()
    {
        RuleFor(x => x.Area)
            .NotEmpty().WithMessage("El área es obligatoria.")
            .MaximumLength(200);

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del puesto es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Nivel)
            .IsInEnum().WithMessage("El nivel no es válido.");

        RuleFor(x => x.Modalidad)
            .IsInEnum().WithMessage("La modalidad no es válida.");

        RuleFor(x => x.Jornada)
            .IsInEnum().WithMessage("La jornada no es válida.");

        RuleFor(x => x.SalarioReferencia)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SalarioReferencia.HasValue)
            .WithMessage("El salario de referencia debe ser mayor o igual a 0.");
    }
}

public class PuestoUpdateValidator : AbstractValidator<PuestoUpdateDto>
{
    public PuestoUpdateValidator()
    {
        RuleFor(x => x.Area)
            .NotEmpty().WithMessage("El área es obligatoria.")
            .MaximumLength(200);

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del puesto es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Nivel)
            .IsInEnum().WithMessage("El nivel no es válido.");

        RuleFor(x => x.Modalidad)
            .IsInEnum().WithMessage("La modalidad no es válida.");

        RuleFor(x => x.Jornada)
            .IsInEnum().WithMessage("La jornada no es válida.");

        RuleFor(x => x.SalarioReferencia)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SalarioReferencia.HasValue)
            .WithMessage("El salario de referencia debe ser mayor o igual a 0.");
    }
}
