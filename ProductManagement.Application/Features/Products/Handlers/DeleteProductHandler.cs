using MediatR;
using ProductManagement.Application.Interfaces;
using ProductManagement.Application.Features.Products.Commands;
using Shared.Logging;

namespace ProductManagement.Application.Features.Products.Handlers
{
    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductService productService;
        private readonly ILoggerService<DeleteProductHandler> logger;

        public DeleteProductHandler(IProductService productService, ILoggerService<DeleteProductHandler> logger)
        {
            this.productService = productService;
            this.logger = logger;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation($"Attempting to delete product with ID: {request.Id}");

                var product = await productService.GetByIdAsync(request.Id);

                if (product == null)
                {
                    logger.LogWarning($"Product with ID: {request.Id} not found.");
                    return false;
                }

                // Perform deletion
                var result = await productService.DeleteAsync(request.Id);

                if (result)
                {
                    logger.LogInformation($"Product with ID: {request.Id} deleted successfully.");
                }
                else
                {
                    logger.LogWarning($"Failed to delete product with ID: {request.Id}.");
                }

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred while deleting the product with ID: {request.Id}.");
                throw new ApplicationException("Error occurred while deleting the product.", ex);
            }
        }
    }
}