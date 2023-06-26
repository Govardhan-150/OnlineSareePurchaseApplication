using SareeApp.Data;
using SareeWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SareeWeb.DataAccess.Repository.IRepository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Product obj)
        {
            var objFromDb=_db.Products.FirstOrDefault(x => x.Id == obj.Id);
            if(objFromDb != null)
            {
                objFromDb.ProductNumber = obj.ProductNumber;
                objFromDb.ProductName = obj.ProductName;
                objFromDb.ProductDescription = obj.ProductDescription;
                objFromDb.ListPrice = obj.ListPrice;
                objFromDb.Price50 = obj.Price50;
                objFromDb.Price=obj.Price;
                objFromDb.Price100=obj.Price100;
                objFromDb.ListPrice = obj.ListPrice;
                objFromDb.CategoryId = obj.CategoryId;
                objFromDb.CoverTypeId = obj.CoverTypeId;
                if(obj.ImageUrl!=null)
                {
                    objFromDb.ImageUrl = obj.ImageUrl;
                }
            }
        }
    }
}
