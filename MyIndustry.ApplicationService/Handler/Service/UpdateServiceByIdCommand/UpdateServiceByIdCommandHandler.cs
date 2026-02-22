using Microsoft.EntityFrameworkCore;
using MyIndustry.Domain.ValueObjects;

namespace MyIndustry.ApplicationService.Handler.Service.UpdateServiceByIdCommand;

public class UpdateServiceByIdCommandHandler : IRequestHandler<UpdateServiceByIdCommand, UpdateServiceByIdCommandResult>
{
    private readonly IGenericRepository<Domain.Aggregate.Service> _serviceRepository;
    private readonly IGenericRepository<Domain.Aggregate.Seller> _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateServiceByIdCommandHandler(
        IGenericRepository<Domain.Aggregate.Service> serviceRepository, 
        IGenericRepository<Domain.Aggregate.Seller> sellerRepository,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateServiceByIdCommandResult> Handle(UpdateServiceByIdCommand request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository
            .GetAllQuery()
            .Include(s => s.Seller)
                .ThenInclude(s => s.SellerSubscription)
            .FirstOrDefaultAsync(p => p.Id == request.ServiceDto.Id && p.SellerId == request.ServiceDto.SellerId, cancellationToken);

        if (service is not { IsActive: true })
        {
            throw new BusinessRuleException("Servis bulunamadı.");
        }

        // Check if changing from non-featured to featured
        if (!service.IsFeatured && request.ServiceDto.IsFeatured)
        {
            if (service.Seller?.SellerSubscription == null)
                throw new BusinessRuleException("Abonelik planı bulunamadı.");
            
            if (service.Seller.SellerSubscription.RemainingFeaturedQuota <= 0)
                throw new BusinessRuleException("Kalan öne çıkan ilan kotası dolu. Lütfen abonelik planınızı yükseltin.");
            
            service.Seller.SellerSubscription.RemainingFeaturedQuota--;
        }
        // If changing from featured to non-featured, restore quota
        else if (service.IsFeatured && !request.ServiceDto.IsFeatured)
        {
            if (service.Seller?.SellerSubscription != null)
            {
                service.Seller.SellerSubscription.RemainingFeaturedQuota++;
            }
        }
        
        service.Title = request.ServiceDto.Title;
        service.Description = request.ServiceDto.Description;
        service.ImageUrls = request.ServiceDto.ImageUrls.ToString();
        service.Price = new Amount(request.ServiceDto.Price).ToDecimal();
        service.EstimatedEndDay = request.ServiceDto.EstimatedEndDay;
        service.IsFeatured = request.ServiceDto.IsFeatured;

        _serviceRepository.Update(service);
        if (service.Seller != null)
        {
            _sellerRepository.Update(service.Seller);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateServiceByIdCommandResult().ReturnOk();
    }
}