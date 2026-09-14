using OmniDesk.Domain.Customers;

namespace OmniDesk.Application.Customers;

public interface ICustomerRepository
{
    void AddCustomer(Customer customer);
}
