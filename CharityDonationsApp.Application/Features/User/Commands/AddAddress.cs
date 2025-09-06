using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Exceptions;
using CharityDonationsApp.Application.Extensions;
using CharityDonationsApp.Domain.Entities;
using FluentValidation;
using MediatR;

namespace CharityDonationsApp.Application.Features.User.Commands;

public static class AddAddress
{
    public record Command(
        string HouseNumber,
        string Landmark,
        string Street,
        string Lga,
        string City,
        string State,
        string Country) : IRequest<Result<string>>;

    public class Handler(
        IAuthService authService,
        IUnitOfWork uOw) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var address = request.ToEntity();
            var userId = Guid.Parse(authService.GetSignedInUserId());
            var userName = authService.GetSignedInUserName();
            address.CreatedBy = userName;
            address.UpdatedBy = userName;

            var kycVerification = await uOw.KycVerificationsReadRepository.GetKycVerificationWithAddressesAsync(userId);
            if (kycVerification is null)
            {
                kycVerification = new KycVerification
                {
                    UserId = userId,
                    CreatedBy = userName,
                    UpdatedBy = userName
                };
                kycVerification.Addresses.Add(address);
                await uOw.KycVerificationsWriteRepository.AddAsync(kycVerification, cancellationToken);
            }
            else
            {
                var addressExists = kycVerification.Addresses.Any(x =>
                    x.KycVerificationId == kycVerification.Id &&
                    x.HouseNumber == request.HouseNumber &&
                    x.Street == request.Street &&
                    x.City == request.City &&
                    x.State == request.State &&
                    x.Country == request.Country);
                if (addressExists) throw ApiException.BadRequest(new Error("User.Error", "Address already exists"));

                address.KycVerificationId = kycVerification.Id;
                await uOw.AddressesWriteRepository.AddAsync(address, cancellationToken);
            }

            await uOw.SaveChangesAsync(cancellationToken);
            return Result.Success("Your address has been successfully added");
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.HouseNumber)
                .NotEmpty().WithMessage("House number is required.")
                .MaximumLength(10);

            RuleFor(x => x.Landmark)
                .MaximumLength(100);

            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street is required.")
                .MaximumLength(100);

            RuleFor(x => x.Lga)
                .NotEmpty().WithMessage("LGA is required.")
                .MaximumLength(100);

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(100);

            RuleFor(x => x.State)
                .NotEmpty().WithMessage("State is required.")
                .MaximumLength(100);

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required.")
                .MaximumLength(100);
        }
    }
}