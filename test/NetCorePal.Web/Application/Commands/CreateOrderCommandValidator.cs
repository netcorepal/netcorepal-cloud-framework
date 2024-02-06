using FluentValidation;

namespace NetCorePal.Web.Application.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        /// <summary>
        /// 
        /// </summary>
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(10);
            RuleFor(x => x.Price).InclusiveBetween(18, 60);
        }
    }
}
