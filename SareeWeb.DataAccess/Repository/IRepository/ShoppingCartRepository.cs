using SareeApp.Data;
using SareeWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SareeWeb.DataAccess.Repository.IRepository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public int Decrement(ShoppingCart shoppingCart, int Count)
        {
            shoppingCart.Count -= Count;
            return shoppingCart.Count;
        }

        public int Increment(ShoppingCart shoppingCart, int Count)
        {
            shoppingCart.Count += Count;
            return shoppingCart.Count;
        }
    }
}
