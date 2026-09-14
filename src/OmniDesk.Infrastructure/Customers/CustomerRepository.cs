using OmniDesk.Application.Customers;
using OmniDesk.Domain.Customers;
using OmniDesk.Infrastructure.Persistence;

namespace OmniDesk.Infrastructure.Customers;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly OmniDeskDbContext _dbContext;

    public CustomerRepository(OmniDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddCustomer(Customer customer)
    {
        _dbContext.Customers.Add(customer);
    }
}
