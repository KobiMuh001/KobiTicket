using FluentValidation;
using KobiMuhendislikTicket.Application.DTOs;

namespace KobiMuhendislikTicket.Application.Validators
{
    public class CreateTicketValidator : AbstractValidator<CreateTicketDto>
    {
        public CreateTicketValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Başlık zorunludur")
                .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Açıklama zorunludur")
                .MinimumLength(10).WithMessage("Açıklama en az 10 karakter olmalıdır");

            RuleFor(x => x.Priority)
                .GreaterThanOrEqualTo(0).WithMessage("Öncelik sıfır veya daha büyük bir sayı olmalıdır");

            RuleFor(x => x.ProductId)
                .NotNull().WithMessage("Ürün seçimi zorunludur")
                .GreaterThan(0).WithMessage("Geçerli bir ürün seçiniz");
        }
    }

    public class AssignTicketValidator : AbstractValidator<AssignTicketDto>
    {
        public AssignTicketValidator()
        {
            RuleFor(x => x.PersonName)
                .NotEmpty().WithMessage("Personel adı zorunludur")
                .MaximumLength(100).WithMessage("Personel adı en fazla 100 karakter olabilir");
        }
    }

    public class ResolveTicketValidator : AbstractValidator<ResolveTicketDto>
    {
        public ResolveTicketValidator()
        {
            RuleFor(x => x.SolutionNote)
                .NotEmpty().WithMessage("Çözüm notu zorunludur")
                .MinimumLength(10).WithMessage("Çözüm notu en az 10 karakter olmalıdır");

            // ResolvedBy is set server-side by the controller, no validation needed
        }
    }

    public class AddCommentValidator : AbstractValidator<AddCommentDto>
    {
        public AddCommentValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Yorum metni zorunludur")
                .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Yazar adı zorunludur");
        }
    }

    public class CustomerCommentValidator : AbstractValidator<CustomerCommentDto>
    {
        public CustomerCommentValidator()
        {
            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Yorum metni zorunludur")
                .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir");
        }
    }
}
