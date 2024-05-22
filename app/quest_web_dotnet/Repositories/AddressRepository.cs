using System;
using quest_web.Contexts;

namespace quest_web.Repositories
{
    public class AddressRepository
    {

        private readonly APIDbContext _context = null;

        public AddressRepository(APIDbContext context)
        {
            this._context = context;
        }


    }
}
