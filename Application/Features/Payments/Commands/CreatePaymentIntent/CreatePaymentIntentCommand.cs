using Application.Common.Models;
using Application.Features.Payments.DTOs;
using FluentValidation;
using MediatR;

namespace Application.Features.Payments.Commands.CreatePaymentIntent;

public sealed record CreatePaymentIntentCommand(
    long BookingId,
    long UserId) : IRequest<ApiResponse<PaymentIntentResponse>>;

public sealed class CreatePaymentIntentCommandValidator
    : AbstractValidator<CreatePaymentIntentCommand>
{
    public CreatePaymentIntentCommandValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0)
            .WithMessage("BookingId must be greater than zero.");
    }
}
