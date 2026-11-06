using FluentValidation;
using YadgarCafe.Application.DTOs;
using YadgarCafe.Application.DTOs.Auth;
using YadgarCafe.Common.Constants;
using YadgarCafe.Domain.Enums;

namespace YadgarCafe.Application.Validators
{
    internal static class Rules
    {
        public static IRuleBuilderOptions<T, string> Phone<T>(this IRuleBuilder<T, string> rule) =>
            rule.NotEmpty().Matches(@"^\+?[0-9]{7,15}$")
                .WithMessage("Phone number must contain 7 to 15 digits and may start with '+'.");

        public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
            rule.NotEmpty().MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain a digit.");
    }

    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Password).StrongPassword();
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
            RuleFor(x => x.PhoneNumber!).Phone().When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        }
    }

    public class CreateStaffUserRequestValidator : AbstractValidator<CreateStaffUserRequest>
    {
        public CreateStaffUserRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Password).StrongPassword();
            RuleFor(x => x.Role)
                .Must(r => r == Roles.Admin || r == Roles.Staff)
                .WithMessage("Role must be 'Admin' or 'Staff'.");
        }
    }

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
    }

    public class CategoryRequestValidator : AbstractValidator<CategoryRequest>
    {
        public CategoryRequestValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }

    public class ProductRequestValidator : AbstractValidator<ProductRequest>
    {
        public ProductRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
            RuleFor(x => x.CategoryId).NotEmpty();
        }
    }

    public class IngredientRequestValidator : AbstractValidator<IngredientRequest>
    {
        public IngredientRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
            RuleFor(x => x.MinimumStock).GreaterThanOrEqualTo(0);
        }
    }

    public class StockMovementRequestValidator : AbstractValidator<StockMovementRequest>
    {
        public StockMovementRequestValidator()
        {
            RuleFor(x => x.IngredientId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x.Type)
                .Must(t => t is StockMovementType.OpeningBalance or StockMovementType.Purchase
                    or StockMovementType.Adjustment or StockMovementType.Wastage)
                .WithMessage("Only OpeningBalance, Purchase, Adjustment and Wastage can be recorded manually.");
            RuleFor(x => x.Remarks).MaximumLength(500);
        }
    }

    public class CreateRecipeRequestValidator : AbstractValidator<CreateRecipeRequest>
    {
        public CreateRecipeRequestValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.IngredientId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }

    public class UpdateRecipeRequestValidator : AbstractValidator<UpdateRecipeRequest>
    {
        public UpdateRecipeRequestValidator() => RuleFor(x => x.Quantity).GreaterThan(0);
    }

    public class CustomerRequestValidator : AbstractValidator<CustomerRequest>
    {
        public CustomerRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.PhoneNumber).Phone();
            RuleFor(x => x.Address).MaximumLength(300);
        }
    }

    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.Items).NotEmpty().WithMessage("Order must contain at least one item.");
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty();
                item.RuleFor(i => i.Quantity).InclusiveBetween(1, 1000);
            });
        }
    }

    public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
    {
        public UpdateOrderStatusRequestValidator() => RuleFor(x => x.Status).IsInEnum();
    }

    public class UpdatePaymentStatusRequestValidator : AbstractValidator<UpdatePaymentStatusRequest>
    {
        public UpdatePaymentStatusRequestValidator() => RuleFor(x => x.Status).IsInEnum();
    }
}
