using CharityDonationsApp.Application.Common.Contracts;
using CharityDonationsApp.Application.Common.Contracts.Abstractions;
using CharityDonationsApp.Application.Common.Exceptions;
using CharityDonationsApp.Domain.Entities;
using FluentValidation;
using MediatR;

namespace CharityDonationsApp.Application.Features.User.Commands;

public static class AddBvn
{
    public record Command(string Bvn) : IRequest<Result<string>>;

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
                    BvnCipher = request.Bvn, BvnHash = utility.ComputeSha256Hash(request.Bvn),
                    IsBvnSuccessfullyVerified = false, UserId = userId, CreatedBy = userName,
                    UpdatedBy = userName
                };
                await uOw.KycVerificationsWriteRepository.AddAsync(kycVerification, cancellationToken);
            }
            else if (kycVerification.BvnCipher is null && kycVerification.BvnHash is null)
            {
                kycVerification.BvnCipher = request.Bvn;
                kycVerification.BvnHash = utility.ComputeSha256Hash(request.Bvn);
                kycVerification.IsBvnSuccessfullyVerified = false;
                kycVerification.UpdatedBy = userName;

                uOw.KycVerificationsWriteRepository.Update(kycVerification,
                    x => x.BvnCipher!,
                    x => x.BvnHash!,
                    x => x.IsBvnSuccessfullyVerified!,
                    x => x.UpdatedBy);
            }
            else
            {
                throw ApiException.BadRequest(new Error("User.Error",
                    "You already have a BVN attached to your profile."));
            }

            await uOw.SaveChangesAsync(cancellationToken);
            return Result.Success("Your BVN has been successfully added.");
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Bvn)
                .NotEmpty().WithMessage("BVN is required")
                .Length(11).WithMessage("BVN is invalid.")
                .Matches(@"^\d{11}$").WithMessage("BVN must contain only digits.");
        }
    }
}