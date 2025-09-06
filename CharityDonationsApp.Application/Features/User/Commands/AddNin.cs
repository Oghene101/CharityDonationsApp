using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Exceptions;
using CharityDonationsApp.Domain.Entities;
using FluentValidation;
using MediatR;

namespace CharityDonationsApp.Application.Features.User.Commands;

public class AddNin
{
    public record Command(string Nin) : IRequest<Result<string>>;

    public class Handler(
        IAuthService auth,
        IUtilityService utility,
        IUnitOfWork uOw) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(auth.GetSignedInUserId());
            var userName = auth.GetSignedInUserName();
            var kycVerification = await uOw.KycVerificationsReadRepository.GetKycVerificationWithAddressesAsync(userId);

            if (kycVerification is null)
            {
                kycVerification = new KycVerification
                {
                    NinCipher = request.Nin, NinHash = utility.ComputeSha256Hash(request.Nin),
                    IsNinSuccessfullyVerified = false, UserId = userId, CreatedBy = userName,
                    UpdatedBy = userName
                };
                await uOw.KycVerificationsWriteRepository.AddAsync(kycVerification, cancellationToken);
            }
            else if (kycVerification.NinCipher is null && kycVerification.NinHash is null)
            {
                kycVerification.NinCipher = request.Nin;
                kycVerification.NinHash = utility.ComputeSha256Hash(request.Nin);
                kycVerification.IsNinSuccessfullyVerified = false;
                kycVerification.UpdatedBy = userName;

                uOw.KycVerificationsWriteRepository.Update(kycVerification,
                    x => x.NinCipher!,
                    x => x.NinHash!,
                    x => x.IsNinSuccessfullyVerified!,
                    x => x.UpdatedBy);
            }
            else
            {
                throw ApiException.BadRequest(new Error("User.Error",
                    "You already have an NIN attached to your profile."));
            }

            await uOw.SaveChangesAsync(cancellationToken);
            return Result.Success("Your NIN has been successfully added.");
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Nin)
                .NotEmpty().WithMessage("NIN is required")
                .Length(11).WithMessage("NIN is invalid.")
                .Matches(@"^\d{11}$").WithMessage("NIN must contain only digits.");
        }
    }
}