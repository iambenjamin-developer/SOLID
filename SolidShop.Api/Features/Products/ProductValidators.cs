using FluentValidation;

namespace SolidShop.Api.Features.Products;

// Single Responsibility: validation rules live here, not in the controller or the service

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Sku)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[A-Za-z0-9-]+$").WithMessage("SKU can only contain letters, numbers and dashes.");
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

public class SetStockRequestValidator : AbstractValidator<SetStockRequest>
{
    public SetStockRequestValidator()
    {
        RuleFor(x => x.Stock).InclusiveBetween(0, 100_000);
    }
}
