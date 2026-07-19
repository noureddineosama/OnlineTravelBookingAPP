using Application.Features.CarBookings.Commands;
using FluentValidation;
using System;

namespace Application.Features.CarBookings.Handlers
{
    public class CreateCarBookingValidator : AbstractValidator<CreateCarBookingCommand>
    {
        public CreateCarBookingValidator()
        {
            RuleFor(x => x.requestDTO.car_id)
                .GreaterThan(0).WithMessage("CarId must be a valid ID.");

            RuleFor(x => x.requestDTO.pickup_location_id)
                .GreaterThan(0).WithMessage("PickupLocationId must be a valid ID.");

            RuleFor(x => x.requestDTO.dropoff_location_id)
                .GreaterThan(0).WithMessage("DropoffLocationId must be a valid ID.");

            RuleFor(x => x.requestDTO.pickup_at)
                .GreaterThan(DateTime.UtcNow).WithMessage("Pickup time must be in the future.");

            RuleFor(x => x.requestDTO.dropoff_at)
                .GreaterThan(x => x.requestDTO.pickup_at).WithMessage("Dropoff time must be after pickup time.");

            RuleForEach(x => x.requestDTO.extras)
                .ChildRules(extra =>
                {
                    extra.RuleFor(x => x.extra_id)
                        .GreaterThan(0).WithMessage("ExtraId must be a valid ID.");

                    extra.RuleFor(x => x.quantity)
                        .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
                });
        }
    }
}
