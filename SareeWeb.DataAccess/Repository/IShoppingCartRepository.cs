using SareeWeb.DataAccess.Repository.IRepository;
using SareeWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SareeWeb.DataAccess.Repository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        int Decrement(ShoppingCart shoppingCart,int Count);
        int Increment(ShoppingCart shoppingCart, int Count);
    }
}
